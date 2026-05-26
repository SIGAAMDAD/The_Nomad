/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Multiplayer.Voting;
using Nomad.Game.Sdk.Events.Multiplayer;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer
{
	internal sealed class VotingService : MultiplayerObject, IVotingService
	{
		private const int DEFAULT_VOTE_SECONDS = 30;
		private const int MAX_OPTIONS = 8;

		public VoteServiceState State => _stateMachine.State;
		public uint StateVersion => _stateMachine.StateVersion;
		public uint VoteVersion => _voteVersion;

		public bool HasActiveVote => _currentVoteId.IsValid && _stateMachine.Is( VoteServiceState.Voting );
		public bool CanRequestVote => IsSessionActive && !_stateMachine.IsAny( VoteServiceState.Disabled, VoteServiceState.Locked, VoteServiceState.Voting, VoteServiceState.Resolving );
		public bool CanCastVote => IsSessionActive && HasActiveVote;

		public VoteInfo? CurrentVote => CreateVoteInfoSnapshot();

		public IGameEvent<VoteServiceStateChangedEventArgs> VoteServiceStateChanged => _voteServiceStateChanged;
		private readonly IGameEvent<VoteServiceStateChangedEventArgs> _voteServiceStateChanged;

		public IGameEvent<VoteStartedEventArgs> VoteStarted => _voteStarted;
		private readonly IGameEvent<VoteStartedEventArgs> _voteStarted;

		public IGameEvent<VoteCastEventArgs> VoteCast => _voteCast;
		private readonly IGameEvent<VoteCastEventArgs> _voteCast;

		public IGameEvent<VoteEndedEventArgs> VoteEnded => _voteEnded;
		private readonly IGameEvent<VoteEndedEventArgs> _voteEnded;

		public IGameEvent<VoteCancelledEventArgs> VoteCancelled => _voteCancelled;
		private readonly IGameEvent<VoteCancelledEventArgs> _voteCancelled;

		private readonly MultiplayerStateMachine<VoteServiceState> _stateMachine;
		private readonly Dictionary<PeerId, VoteOptionId> _ballots = new Dictionary<PeerId, VoteOptionId>();
		private readonly List<VoteOptionInfo> _options = new List<VoteOptionInfo>( MAX_OPTIONS );

		private VoteId _currentVoteId = VoteId.Invalid;
		private VoteKind _currentKind = VoteKind.None;
		private PeerId _startedBy = PeerId.Invalid;
		private PeerId _targetPeer = PeerId.Invalid;
		private DateTime _startedUtc;
		private DateTime _endsUtc;
		private uint _voteVersion;
		private int _eligibleVoters;
		private int _requiredVotes;

		public VotingService(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry registry,
			IGameEventRegistryService eventFactory
		)
			: base( sessionService, rpcBus, eventBus, registry, eventFactory )
		{
			_stateMachine = new MultiplayerStateMachine<VoteServiceState>(
				VoteServiceState.Idle,
				IsValidStateTransition
			);

			_voteServiceStateChanged = GetEvent<VoteServiceStateChangedEventArgs>( nameof( VoteServiceStateChangedEventArgs ), "Nomad.Game.Sdk.Events.Multiplayer" );
			_voteStarted = GetEvent<VoteStartedEventArgs>( nameof( VoteStartedEventArgs ), "Nomad.Game.Sdk.Events.Multiplayer" );
			_voteCast = GetEvent<VoteCastEventArgs>( nameof( VoteCastEventArgs ), "Nomad.Game.Sdk.Events.Multiplayer" );
			_voteEnded = GetEvent<VoteEndedEventArgs>( nameof( VoteEndedEventArgs ), "Nomad.Game.Sdk.Events.Multiplayer" );
			_voteCancelled = GetEvent<VoteCancelledEventArgs>( nameof( VoteCancelledEventArgs ), "Nomad.Game.Sdk.Events.Multiplayer" );

			RegisterNetworkEvent( MessageIds.VoteServiceStateChanged, _voteServiceStateChanged );
			RegisterNetworkEvent( MessageIds.VoteStarted, _voteStarted );
			RegisterNetworkEvent( MessageIds.VoteCast, _voteCast );
			RegisterNetworkEvent( MessageIds.VoteEnded, _voteEnded );
			RegisterNetworkEvent( MessageIds.VoteCancelled, _voteCancelled );

			RegisterRpc<VoteStartGameRequestRpc>( MessageIds.VoteStartGameRequestRpc, OnStartGameVoteRequest );
			RegisterRpc<VoteCastRequestRpc>( MessageIds.VoteCastRequestRpc, OnCastVoteRequest );

			Subscribe( _voteServiceStateChanged, OnVoteServiceStateChanged );
			Subscribe( _voteStarted, OnVoteStarted );
			Subscribe( _voteCast, OnVoteCast );
			Subscribe( _voteEnded, OnVoteEnded );
			Subscribe( _voteCancelled, OnVoteCancelled );
			Subscribe( sessionService.SessionChanged, OnSessionChanged );
			Subscribe( sessionService.PeerDisconnected, OnPeerDisconnected );
		}

		public bool RequestStartGameVote()
		{
			if ( !CanRequestVote || !LocalPeerId.IsValid ) {
				return false;
			}

			if ( IsHost ) {
				return StartGameStartVote( LocalPeerId );
			}

			var rpc = new VoteStartGameRequestRpc( LocalPeerId );
			return SendRpcToHost( in rpc, NetworkSendMode.Reliable );
		}

		public bool CastVote( VoteOptionId optionId )
		{
			if ( !CanCastVote || !LocalPeerId.IsValid ) {
				return false;
			}

			if ( IsHost ) {
				return CastPeerVote( LocalPeerId, optionId );
			}

			var rpc = new VoteCastRequestRpc( LocalPeerId, _currentVoteId, optionId );
			return SendRpcToHost( in rpc, NetworkSendMode.Reliable );
		}

		public bool Abstain()
		{
			return CastVote( VoteOptionId.Abstain );
		}

		public bool StartGameStartVote( PeerId startedBy )
		{
			if ( !IsHost || !CanRequestVote || !IsPeerInSession( startedBy ) ) {
				return false;
			}

			return StartVoteInternal( VoteKind.StartGame, startedBy, PeerId.Invalid, DEFAULT_VOTE_SECONDS );
		}

		public bool CastPeerVote( PeerId peerId, VoteOptionId optionId )
		{
			if ( !IsHost || !HasActiveVote || !IsPeerInSession( peerId ) || !IsKnownOption( optionId ) ) {
				return false;
			}

			_ballots[peerId] = optionId;
			AdvanceVoteVersion();

			var payload = new VoteCastEventArgs( _currentVoteId, peerId, optionId, _voteVersion );
			PublishHostEvent( _voteCast, in payload, NetworkSendMode.Reliable );

			TryResolveEarly();
			return true;
		}

		public bool CancelVote( VoteEndReason reason = VoteEndReason.Cancelled )
		{
			if ( !IsHost || !HasActiveVote ) {
				return false;
			}

			AdvanceVoteVersion();
			VoteId voteId = _currentVoteId;

			var payload = new VoteCancelledEventArgs( voteId, reason, _voteVersion );
			PublishHostEvent( _voteCancelled, in payload, NetworkSendMode.Reliable );
			ClearVoteLocal();
			TransitionVoteState( VoteServiceState.Idle );
			return true;
		}

		public bool ResolveVote()
		{
			if ( !IsHost || !HasActiveVote ) {
				return false;
			}

			TransitionVoteState( VoteServiceState.Resolving );

			VoteOptionId winner = GetWinningOption();
			bool passed = winner == VoteOptionId.Yes;
			VoteEndReason reason = passed ? VoteEndReason.Passed : VoteEndReason.Failed;

			AdvanceVoteVersion();
			var payload = new VoteEndedEventArgs( _currentVoteId, _currentKind, passed, winner, reason, _voteVersion );
			PublishHostEvent( _voteEnded, in payload, NetworkSendMode.Reliable );

			ClearVoteLocal();
			TransitionVoteState( VoteServiceState.Cooldown );
			TransitionVoteState( VoteServiceState.Idle );
			return true;
		}

		public bool LockVoting()
		{
			if ( HasActiveVote ) {
				CancelVote( VoteEndReason.Interrupted );
			}

			return TransitionVoteState( VoteServiceState.Locked );
		}

		public bool UnlockVoting()
		{
			return TransitionVoteState( VoteServiceState.Idle );
		}

		public bool ResetVoting()
		{
			if ( !IsHost ) {
				return false;
			}

			ClearVoteLocal();
			_voteVersion = 0;
			return TransitionVoteState( VoteServiceState.Idle, true );
		}

		public bool TryGetVote( VoteId voteId, out VoteInfo info )
		{
			if ( _currentVoteId == voteId && voteId.IsValid ) {
				info = CreateVoteInfoSnapshot();
				return true;
			}

			info = null;
			return false;
		}

		public bool TryGetOption( VoteOptionId optionId, out VoteOptionInfo option )
		{
			for ( int i = 0; i < _options.Count; i++ ) {
				if ( _options[i].Id == optionId ) {
					option = CreateOptionSnapshot( _options[i] );
					return true;
				}
			}

			option = default;
			return false;
		}

		public bool TryGetPeerVote( PeerId peerId, out VoteOptionId optionId )
		{
			return _ballots.TryGetValue( peerId, out optionId );
		}

		public bool HasPeerVoted( PeerId peerId )
		{
			return _ballots.ContainsKey( peerId );
		}

		public int GetVoteCount( VoteOptionId optionId )
		{
			int count = 0;
			foreach ( VoteOptionId vote in _ballots.Values ) {
				if ( vote == optionId ) {
					count++;
				}
			}
			return count;
		}

		public int GetTotalVotesCast()
		{
			return _ballots.Count;
		}

		public int CopyOptions( VoteOptionInfo[] destination )
		{
			if ( destination == null ) {
				return 0;
			}

			int count = Math.Min( destination.Length, _options.Count );
			for ( int i = 0; i < count; i++ ) {
				destination[i] = CreateOptionSnapshot( _options[i] );
			}
			return count;
		}

		public int CopyBallots( VoteBallotInfo[] destination )
		{
			if ( destination == null ) {
				return 0;
			}

			int index = 0;
			foreach ( KeyValuePair<PeerId, VoteOptionId> pair in _ballots ) {
				if ( index >= destination.Length ) {
					break;
				}

				destination[index++] = new VoteBallotInfo( pair.Key, pair.Value, _voteVersion );
			}
			return index;
		}

		private bool StartVoteInternal( VoteKind kind, PeerId startedBy, PeerId targetPeer, int seconds )
		{
			_currentVoteId = new VoteId( Guid.NewGuid() );
			_currentKind = kind;
			_startedBy = startedBy;
			_targetPeer = targetPeer;
			_startedUtc = DateTime.UtcNow;
			_endsUtc = _startedUtc.AddSeconds( Math.Max( 1, seconds ) );
			_eligibleVoters = Math.Max( 1, GetEligibleVoterCount() );
			_requiredVotes = (_eligibleVoters / 2) + 1;

			_ballots.Clear();
			_options.Clear();
			_options.Add( new VoteOptionInfo( VoteOptionId.Yes, kind, "Yes", "yes" ) );
			_options.Add( new VoteOptionInfo( VoteOptionId.No, kind, "No", "no" ) );

			AdvanceVoteVersion();
			TransitionVoteState( VoteServiceState.Voting );

			var payload = new VoteStartedEventArgs( _currentVoteId, kind, startedBy, targetPeer, (byte)_options.Count, _voteVersion );
			PublishHostEvent( _voteStarted, in payload, NetworkSendMode.Reliable );

			CastPeerVote( startedBy, VoteOptionId.Yes );
			return true;
		}

		private void TryResolveEarly()
		{
			if ( !HasActiveVote ) {
				return;
			}

			int yes = GetVoteCount( VoteOptionId.Yes );
			int no = GetVoteCount( VoteOptionId.No );

			if ( yes >= _requiredVotes || no >= _requiredVotes || _ballots.Count >= _eligibleVoters ) {
				ResolveVote();
			}
		}

		private VoteOptionId GetWinningOption()
		{
			int yes = GetVoteCount( VoteOptionId.Yes );
			int no = GetVoteCount( VoteOptionId.No );

			return yes > no ? VoteOptionId.Yes : VoteOptionId.No;
		}

		private int GetEligibleVoterCount()
		{
			if ( CurrentSession == null ) {
				return 0;
			}

			int count = 1; // local peer
			IReadOnlyList<NetworkPeerInfo> peers = CurrentSession.Peers;
			for ( int i = 0; i < peers.Count; i++ ) {
				if ( !peers[i].IsLocal ) {
					count++;
				}
			}
			return count;
		}

		private bool IsPeerInSession( PeerId peerId )
		{
			if ( !peerId.IsValid || CurrentSession == null ) {
				return false;
			}

			if ( peerId == CurrentSession.LocalPeerId ) {
				return true;
			}

			IReadOnlyList<NetworkPeerInfo> peers = CurrentSession.Peers;
			for ( int i = 0; i < peers.Count; i++ ) {
				if ( peers[i].PeerId == peerId ) {
					return true;
				}
			}

			return false;
		}

		private bool IsKnownOption( VoteOptionId optionId )
		{
			for ( int i = 0; i < _options.Count; i++ ) {
				if ( _options[i].Id == optionId ) {
					return true;
				}
			}
			return false;
		}

		private VoteInfo? CreateVoteInfoSnapshot()
		{
			if ( !_currentVoteId.IsValid ) {
				return null;
			}

			return new VoteInfo {
				Id = _currentVoteId,
				Kind = _currentKind,
				StartedBy = _startedBy,
				TargetPeer = _targetPeer,
				RequiredVotes = _requiredVotes,
				EligibleVoters = _eligibleVoters,
				VotesCast = _ballots.Count,
				Version = _voteVersion,
				StartedUtc = _startedUtc,
				EndsUtc = _endsUtc
			};
		}

		private VoteOptionInfo CreateOptionSnapshot( VoteOptionInfo option )
		{
			return new VoteOptionInfo( option.Id, option.Kind, option.DisplayName, option.Value, GetVoteCount( option.Id ) );
		}

		private bool TransitionVoteState( VoteServiceState nextState, bool allowSameState = false )
		{
			return _stateMachine.TransitionHost( this, nextState, ReplicateVoteServiceState, allowSameState );
		}

		private void ReplicateVoteServiceState( VoteServiceState previousState, VoteServiceState currentState, uint version )
		{
			var payload = new VoteServiceStateChangedEventArgs( previousState, currentState, version );
			PublishHostEvent( _voteServiceStateChanged, in payload, NetworkSendMode.Reliable );
		}

		private uint AdvanceVoteVersion()
		{
			unchecked { _voteVersion++; }
			return _voteVersion;
		}

		private bool IsStaleVoteVersion( uint version )
		{
			return version != 0 && _voteVersion != 0 && version <= _voteVersion;
		}

		private void ClearVoteLocal()
		{
			_currentVoteId = VoteId.Invalid;
			_currentKind = VoteKind.None;
			_startedBy = PeerId.Invalid;
			_targetPeer = PeerId.Invalid;
			_startedUtc = default;
			_endsUtc = default;
			_eligibleVoters = 0;
			_requiredVotes = 0;
			_ballots.Clear();
			_options.Clear();
		}

		[RpcMethod( "VoteStartGameRequestRpc" )]
		[RpcMethodPayload( "PeerId", typeof( PeerId ), Order = 1 )]
		private void OnStartGameVoteRequest( in NetworkRpcContext conetxt, in VoteStartGameRequestRpc rpc )
		{
			if ( !IsHost ) {
				return;
			}
			StartGameStartVote( rpc.PeerId );
		}

		[RpcMethod( "VoteCastRequestRpc" )]
		[RpcMethodPayload( "PeerId", typeof( PeerId ), Order = 1 )]
		[RpcMethodPayload( "VoteId", typeof( VoteId ), Order = 2 )]
		[RpcMethodPayload( "OptionId", typeof( VoteOptionId ), Order = 3 )]
		private void OnCastVoteRequest( in NetworkRpcContext context, in VoteCastRequestRpc rpc )
		{
			if ( !IsHost || rpc.VoteId != _currentVoteId ) {
				return;
			}
			CastPeerVote( rpc.PeerId, rpc.OptionId );
		}

		private void OnSessionChanged( in NetworkSessionChangedEventArgs args )
		{
			if ( IsSessionActive ) {
				return;
			}

			if ( IsHost && HasActiveVote ) {
				CancelVote( VoteEndReason.SessionEnded );
			}

			ClearVoteLocal();
			_voteVersion = 0;
			_stateMachine.Reset( VoteServiceState.Idle );
		}

		private void OnPeerDisconnected( in PeerDisconnectedEventArgs args )
		{
			if ( !IsHost || !HasActiveVote ) {
				return;
			}

			if ( _ballots.Remove( args.PeerId ) ) {
				AdvanceVoteVersion();
			}

			_eligibleVoters = Math.Max( 1, GetEligibleVoterCount() );
			_requiredVotes = (_eligibleVoters / 2) + 1;
			TryResolveEarly();
		}

		private void OnVoteServiceStateChanged( in VoteServiceStateChangedEventArgs args )
		{
			if ( IsHost ) {
				return;
			}
			_stateMachine.ApplyReplicated( args.CurrentState, args.Version );
		}

		private void OnVoteStarted( in VoteStartedEventArgs args )
		{
			if ( IsHost || IsStaleVoteVersion( args.Version ) ) {
				return;
			}

			_currentVoteId = args.VoteId;
			_currentKind = args.Kind;
			_startedBy = args.StartedBy;
			_targetPeer = args.TargetPeer;
			_startedUtc = DateTime.UtcNow;
			_endsUtc = _startedUtc.AddSeconds( DEFAULT_VOTE_SECONDS );
			_options.Clear();
			_options.Add( new VoteOptionInfo( VoteOptionId.Yes, args.Kind, "Yes", "yes" ) );
			_options.Add( new VoteOptionInfo( VoteOptionId.No, args.Kind, "No", "no" ) );
			_ballots.Clear();
			_voteVersion = args.Version;
		}

		private void OnVoteCast( in VoteCastEventArgs args )
		{
			if ( IsHost || args.VoteId != _currentVoteId || IsStaleVoteVersion( args.Version ) ) {
				return;
			}

			_ballots[args.PeerId] = args.OptionId;
			_voteVersion = args.Version;
		}

		private void OnVoteEnded( in VoteEndedEventArgs args )
		{
			if ( IsHost || args.VoteId != _currentVoteId || IsStaleVoteVersion( args.Version ) ) {
				return;
			}

			_voteVersion = args.Version;
			ClearVoteLocal();
		}

		private void OnVoteCancelled( in VoteCancelledEventArgs args )
		{
			if ( IsHost || args.VoteId != _currentVoteId || IsStaleVoteVersion( args.Version ) ) {
				return;
			}

			_voteVersion = args.Version;
			ClearVoteLocal();
		}

		private static bool IsValidStateTransition( VoteServiceState currentState, VoteServiceState nextState )
		{
			if ( nextState == VoteServiceState.Count ) {
				return false;
			}

			if ( currentState == nextState ) {
				return true;
			}

			switch ( currentState ) {
				case VoteServiceState.Disabled:
					return nextState == VoteServiceState.Idle;
				case VoteServiceState.Idle:
					return nextState == VoteServiceState.Voting || nextState == VoteServiceState.Locked || nextState == VoteServiceState.Disabled;
				case VoteServiceState.Voting:
					return nextState == VoteServiceState.Resolving || nextState == VoteServiceState.Idle || nextState == VoteServiceState.Locked;
				case VoteServiceState.Resolving:
					return nextState == VoteServiceState.Cooldown || nextState == VoteServiceState.Idle;
				case VoteServiceState.Cooldown:
					return nextState == VoteServiceState.Idle || nextState == VoteServiceState.Locked;
				case VoteServiceState.Locked:
					return nextState == VoteServiceState.Idle || nextState == VoteServiceState.Disabled;
				default:
					return false;
			}
		}
	};
};
