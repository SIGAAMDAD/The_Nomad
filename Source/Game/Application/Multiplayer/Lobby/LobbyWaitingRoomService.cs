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
using Nomad.Game.Sdk.Multiplayer.Lobby;
using Nomad.Game.Sdk.Multiplayer.Voting;
using Nomad.Game.Sdk.Events.Multiplayer;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer
{
	/*
	===================================================================================

	LobbyWaitingRoomService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class LobbyWaitingRoomService : MultiplayerObject, ILobbyWaitingRoomService
	{
		private const int DEFAULT_MIN_PLAYERS = 1;
		private const int DEFAULT_COUNTDOWN_SECONDS = 5;

		public LobbyWaitingRoomState State => _stateMachine.State;
		public uint StateVersion => _stateMachine.StateVersion;
		public uint RosterVersion => _rosterVersion;

		public bool IsOpen => _stateMachine.IsAny( LobbyWaitingRoomState.WaitingForPlayers, LobbyWaitingRoomState.ReadyCheck, LobbyWaitingRoomState.Countdown );
		public bool CanStart => IsSessionActive && IsHost && GetPeerCount() >= _minPlayers && !_stateMachine.IsAny( LobbyWaitingRoomState.Closed, LobbyWaitingRoomState.Starting, LobbyWaitingRoomState.InGame );
		public bool CanReady => IsSessionActive && _stateMachine.IsAny( LobbyWaitingRoomState.WaitingForPlayers, LobbyWaitingRoomState.ReadyCheck, LobbyWaitingRoomState.Countdown );
		public bool CanChangeSettings => IsSessionActive && IsHost && _stateMachine.IsAny( LobbyWaitingRoomState.WaitingForPlayers, LobbyWaitingRoomState.ReadyCheck );

		public LobbyWaitingRoomInfo? Current => CreateInfoSnapshot();

		public IGameEvent<LobbyWaitingRoomStateChangedEventArgs> WaitingRoomStateChanged => _waitingRoomStateChanged;
		private readonly IGameEvent<LobbyWaitingRoomStateChangedEventArgs> _waitingRoomStateChanged;

		public IGameEvent<LobbyPeerReadyChangedEventArgs> PeerReadyChanged => _peerReadyChanged;
		private readonly IGameEvent<LobbyPeerReadyChangedEventArgs> _peerReadyChanged;

		public IGameEvent<LobbyCountdownStartedEventArgs> CountdownStarted => _countdownStarted;
		private readonly IGameEvent<LobbyCountdownStartedEventArgs> _countdownStarted;

		public IGameEvent<LobbyCountdownCancelledEventArgs> CountdownCancelled => _countdownCancelled;
		private readonly IGameEvent<LobbyCountdownCancelledEventArgs> _countdownCancelled;

		public IGameEvent<LobbyGameStartRequestedEventArgs> GameStartRequested => _gameStartRequested;
		private readonly IGameEvent<LobbyGameStartRequestedEventArgs> _gameStartRequested;

		private readonly IVotingService _votingService;
		private readonly MultiplayerStateMachine<LobbyWaitingRoomState> _stateMachine;
		private readonly Dictionary<PeerId, LobbyReadyState> _readyByPeer = new Dictionary<PeerId, LobbyReadyState>();

		private uint _rosterVersion;
		private int _minPlayers = DEFAULT_MIN_PLAYERS;
		private bool _lateJoinAllowed = true;
		private DateTime _countdownEndsUtc;
		private int _countdownSeconds;

		/*
		===============
		LobbyWaitingRoomService
		===============
		*/
		public LobbyWaitingRoomService(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory,
			IVotingService votingService
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
			_votingService = votingService ?? throw new ArgumentNullException( nameof( votingService ) );
			_stateMachine = new MultiplayerStateMachine<LobbyWaitingRoomState>(
				LobbyWaitingRoomState.Closed,
				IsValidStateTransition
			);

			_waitingRoomStateChanged = GetEvent<LobbyWaitingRoomStateChangedEventArgs>(
				LobbyWaitingRoomStateChangedEventArgs.Name,
				LobbyWaitingRoomStateChangedEventArgs.NameSpace
			);
			_peerReadyChanged = GetEvent<LobbyPeerReadyChangedEventArgs>(
				LobbyPeerReadyChangedEventArgs.Name,
				LobbyPeerReadyChangedEventArgs.NameSpace
			);
			_countdownStarted = GetEvent<LobbyCountdownStartedEventArgs>(
				LobbyCountdownStartedEventArgs.Name,
				LobbyCountdownStartedEventArgs.NameSpace
			);
			_countdownCancelled = GetEvent<LobbyCountdownCancelledEventArgs>(
				LobbyCountdownCancelledEventArgs.Name,
				LobbyCountdownCancelledEventArgs.NameSpace
			);
			_gameStartRequested = GetEvent<LobbyGameStartRequestedEventArgs>(
				LobbyGameStartRequestedEventArgs.Name,
				LobbyGameStartRequestedEventArgs.NameSpace
			);

			RegisterNetworkEvent( MessageIds.WaitingRoomStateChanged, _waitingRoomStateChanged );
			RegisterNetworkEvent( MessageIds.PeerReadyChanged, _peerReadyChanged );
			RegisterNetworkEvent( MessageIds.CountdownStarted, _countdownStarted );
			RegisterNetworkEvent( MessageIds.CountdownCancelled, _countdownCancelled );
			RegisterNetworkEvent( MessageIds.GameStartRequested, _gameStartRequested );

			RegisterRpc<LobbyReadyRequestRpc>( MessageIds.LobbyReadyRequestRpc, OnReadyRequest );
			RegisterRpc<LobbyCancelCountdownRequestRpc>( MessageIds.LobbyCancelCountdownRequestRpc, OnCancelCountdownRequest );

			Subscribe( _waitingRoomStateChanged, OnWaitingRoomStateChanged );
			Subscribe( _peerReadyChanged, OnPeerReadyChanged );
			Subscribe( _countdownStarted, OnCountdownStarted );
			Subscribe( _countdownCancelled, OnCountdownCancelled );
			Subscribe( _gameStartRequested, OnGameStartRequested );

			Subscribe( sessionService.SessionChanged, OnSessionChanged );
			Subscribe( sessionService.PeerConnected, OnPeerConnected );
			Subscribe( sessionService.PeerDisconnected, OnPeerDisconnected );
			Subscribe( votingService.VoteEnded, OnVoteEnded );
		}

		/*
		===============
		Frame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Frame()
		{
			if ( !IsHost || !_stateMachine.Is( LobbyWaitingRoomState.Countdown ) ) {
				return;
			}

			if ( !AreAllPlayersReady() ) {
				CancelCountdown();
				return;
			}

			if ( DateTime.UtcNow >= _countdownEndsUtc ) {
				StartGame( LobbyGameStartReason.AllPlayersReady );
			}
		}

		/*
		===============
		RequestReady
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool RequestReady()
		{
			return RequestReadyState( LobbyReadyState.Ready );
		}

		public bool RequestNotReady()
		{
			return RequestReadyState( LobbyReadyState.NotReady );
		}

		public bool RequestToggleReady()
		{
			if ( !TryGetPeerStatus( LocalPeerId, out LobbyPeerStatus status ) ) {
				return RequestReady();
			}

			return status.ReadyState == LobbyReadyState.Ready
				? RequestNotReady()
				: RequestReady();
		}

		public bool RequestStartGame()
		{
			if ( !IsSessionActive ) {
				return false;
			}

			if ( IsHost ) {
				return StartGame( LobbyGameStartReason.HostStarted );
			}

			return _votingService.RequestStartGameVote();
		}

		public bool RequestCancelCountdown()
		{
			if ( !IsSessionActive ) {
				return false;
			}

			if ( IsHost ) {
				return CancelCountdown();
			}

			var rpc = new LobbyCancelCountdownRequestRpc( LocalPeerId );
			return SendRpcToHost( in rpc, NetworkSendMode.Reliable );
		}

		public bool OpenWaitingRoom()
		{
			if ( !IsHost || !IsSessionActive ) {
				return false;
			}

			SyncPeersFromSession();
			bool result = TransitionWaitingRoomState( LobbyWaitingRoomState.WaitingForPlayers );
			EvaluateReadyGate();
			return result;
		}

		public bool CloseWaitingRoom()
		{
			if ( !IsHost ) {
				return false;
			}

			if ( _stateMachine.Is( LobbyWaitingRoomState.Countdown ) ) {
				CancelCountdown();
			}

			bool result = TransitionWaitingRoomState( LobbyWaitingRoomState.Closed );
			ClearRosterLocal();
			return result;
		}

		public bool MarkInGame()
		{
			return TransitionWaitingRoomState( LobbyWaitingRoomState.InGame );
		}

		public bool SetPeerReadyState( PeerId peerId, LobbyReadyState readyState )
		{
			if ( !IsHost || !CanReady || !IsPeerInSession( peerId ) || readyState == LobbyReadyState.Count ) {
				return false;
			}

			_readyByPeer.TryGetValue( peerId, out LobbyReadyState previousState );
			if ( previousState == readyState ) {
				return true;
			}

			_readyByPeer[peerId] = readyState;
			AdvanceRosterVersion();

			var payload = new LobbyPeerReadyChangedEventArgs( peerId, previousState, readyState, _rosterVersion );
			PublishHostEvent( _peerReadyChanged, in payload, NetworkSendMode.Reliable );

			EvaluateReadyGate();
			return true;
		}

		public bool ResetReadyStates()
		{
			if ( !IsHost ) {
				return false;
			}

			List<PeerId> peers = new List<PeerId>( _readyByPeer.Keys );
			for ( int i = 0; i < peers.Count; i++ ) {
				SetPeerReadyState( peers[i], LobbyReadyState.NotReady );
			}
			return true;
		}

		public bool StartCountdown( int seconds )
		{
			if ( !IsHost || !CanStart || seconds <= 0 ) {
				return false;
			}

			_countdownSeconds = seconds;
			_countdownEndsUtc = DateTime.UtcNow.AddSeconds( seconds );
			AdvanceRosterVersion();

			TransitionWaitingRoomState( LobbyWaitingRoomState.Countdown );

			var payload = new LobbyCountdownStartedEventArgs( seconds, _rosterVersion );
			return PublishHostEvent( _countdownStarted, in payload, NetworkSendMode.Reliable );
		}

		public bool CancelCountdown()
		{
			if ( !IsHost || !_stateMachine.Is( LobbyWaitingRoomState.Countdown ) ) {
				return false;
			}

			_countdownSeconds = 0;
			_countdownEndsUtc = default;
			AdvanceRosterVersion();

			var payload = new LobbyCountdownCancelledEventArgs( _rosterVersion );
			PublishHostEvent( _countdownCancelled, in payload, NetworkSendMode.Reliable );

			TransitionWaitingRoomState( LobbyWaitingRoomState.ReadyCheck );
			return true;
		}

		public bool StartGame( LobbyGameStartReason reason = LobbyGameStartReason.HostStarted )
		{
			if ( !IsHost || !CanStart ) {
				return false;
			}

			if ( _stateMachine.Is( LobbyWaitingRoomState.Countdown ) ) {
				_countdownSeconds = 0;
				_countdownEndsUtc = default;
			}

			if ( _votingService.State != VoteServiceState.Resolving ) {
				_votingService.LockVoting();
			}

			TransitionWaitingRoomState( LobbyWaitingRoomState.Starting );
			AdvanceRosterVersion();

			var payload = new LobbyGameStartRequestedEventArgs( reason, _rosterVersion );
			return PublishHostEvent( _gameStartRequested, in payload, NetworkSendMode.Reliable );
		}

		public bool SetMinPlayers( int minPlayers )
		{
			if ( !CanChangeSettings || minPlayers < 1 ) {
				return false;
			}

			_minPlayers = minPlayers;
			EvaluateReadyGate();
			return true;
		}

		public bool SetLateJoinAllowed( bool allowed )
		{
			if ( !CanChangeSettings ) {
				return false;
			}

			_lateJoinAllowed = allowed;
			return true;
		}

		public bool TryGetPeerStatus( PeerId peerId, out LobbyPeerStatus status )
		{
			if ( !peerId.IsValid || CurrentSession == null ) {
				status = default;
				return false;
			}

			if ( peerId == CurrentSession.LocalPeerId ) {
				_readyByPeer.TryGetValue( peerId, out LobbyReadyState readyState );
				status = new LobbyPeerStatus( peerId, readyState, peerId == CurrentSession.HostPeerId, true, true );
				return true;
			}

			IReadOnlyList<NetworkPeerInfo> peers = CurrentSession.Peers;
			for ( int i = 0; i < peers.Count; i++ ) {
				NetworkPeerInfo peer = peers[i];
				if ( peer.PeerId != peerId ) {
					continue;
				}

				_readyByPeer.TryGetValue( peerId, out LobbyReadyState readyState );
				status = new LobbyPeerStatus( peerId, readyState, peer.IsHost, peer.IsLocal, true );
				return true;
			}

			status = default;
			return false;
		}

		public bool IsPeerReady( PeerId peerId )
		{
			return _readyByPeer.TryGetValue( peerId, out LobbyReadyState readyState ) && readyState == LobbyReadyState.Ready;
		}

		public int GetReadyCount()
		{
			int count = 0;
			foreach ( LobbyReadyState readyState in _readyByPeer.Values ) {
				if ( readyState == LobbyReadyState.Ready ) {
					count++;
				}
			}
			return count;
		}

		public int GetPeerCount()
		{
			return _readyByPeer.Count;
		}

		public int CopyPeers( LobbyPeerStatus[] destination )
		{
			if ( destination == null ) {
				return 0;
			}

			int index = 0;
			foreach ( PeerId peerId in _readyByPeer.Keys ) {
				if ( index >= destination.Length ) {
					break;
				}

				if ( TryGetPeerStatus( peerId, out LobbyPeerStatus status ) ) {
					destination[index++] = status;
				}
			}

			return index;
		}

		private bool RequestReadyState( LobbyReadyState readyState )
		{
			if ( !CanReady || !LocalPeerId.IsValid ) {
				return false;
			}

			if ( IsHost ) {
				return SetPeerReadyState( LocalPeerId, readyState );
			}

			var rpc = new LobbyReadyRequestRpc( LocalPeerId, readyState );
			return SendRpcToHost( in rpc, NetworkSendMode.Reliable );
		}

		[RpcMethod( "LobbyReadyRequestRpc" )]
		[RpcMethodPayload( "PeerId", typeof( PeerId ), Order = 1 )]
		[RpcMethodPayload( "ReadyState", typeof( LobbyReadyState ), Order = 2 )]
		private void OnReadyRequest( in NetworkRpcContext context, in LobbyReadyRequestRpc rpc )
		{
			if ( !IsHost ) {
				return;
			}
			SetPeerReadyState( rpc.PeerId, rpc.ReadyState );
		}

		[RpcMethod( "LobbyCancelCountdownRequestRpc" )]
		[RpcMethodPayload( "PeerId", typeof( PeerId ), Order = 1 )]
		private void OnCancelCountdownRequest( in NetworkRpcContext context, in LobbyCancelCountdownRequestRpc rpc )
		{
			if ( !IsHost || !IsPeerInSession( rpc.PeerId ) ) {
				return;
			}
			CancelCountdown();
		}

		private void OnSessionChanged( in NetworkSessionChangedEventArgs args )
		{
			if ( !IsSessionActive ) {
				ClearRosterLocal();
				_stateMachine.Reset( LobbyWaitingRoomState.Closed );
				return;
			}

			SyncPeersFromSession();

			if ( IsHost && _stateMachine.Is( LobbyWaitingRoomState.Closed ) ) {
				OpenWaitingRoom();
			}
		}

		private void OnPeerConnected( in PeerConnectedEventArgs args )
		{
			if ( !IsHost ) {
				AddPeerLocal( args.PeerId, LobbyReadyState.NotReady );
				return;
			}

			if ( !_lateJoinAllowed && !_stateMachine.IsAny( LobbyWaitingRoomState.WaitingForPlayers, LobbyWaitingRoomState.ReadyCheck ) ) {
				return;
			}

			if ( AddPeerLocal( args.PeerId, LobbyReadyState.NotReady ) ) {
				var payload = new LobbyPeerReadyChangedEventArgs( args.PeerId, LobbyReadyState.NotReady, LobbyReadyState.NotReady, _rosterVersion );
				PublishHostEvent( _peerReadyChanged, in payload, NetworkSendMode.Reliable );
			}

			EvaluateReadyGate();
		}

		private void OnPeerDisconnected( in PeerDisconnectedEventArgs args )
		{
			if ( _readyByPeer.Remove( args.PeerId ) ) {
				AdvanceRosterVersion();
			}

			if ( IsHost ) {
				EvaluateReadyGate();
			}
		}

		private void OnVoteEnded( in VoteEndedEventArgs args )
		{
			if ( !IsHost || args.Kind != VoteKind.StartGame || !args.Passed ) {
				return;
			}

			StartGame( LobbyGameStartReason.VotePassed );
		}

		private void OnWaitingRoomStateChanged( in LobbyWaitingRoomStateChangedEventArgs args )
		{
			if ( IsHost ) {
				return;
			}
			_stateMachine.ApplyReplicated( args.CurrentState, args.Version );
		}

		private void OnPeerReadyChanged( in LobbyPeerReadyChangedEventArgs args )
		{
			if ( IsHost || IsStaleRosterVersion( args.Version ) ) {
				return;
			}

			_readyByPeer[args.PeerId] = args.CurrentState;
			_rosterVersion = args.Version;
		}

		private void OnCountdownStarted( in LobbyCountdownStartedEventArgs args )
		{
			if ( IsHost || IsStaleRosterVersion( args.Version ) ) {
				return;
			}

			_countdownSeconds = args.Seconds;
			_countdownEndsUtc = DateTime.UtcNow.AddSeconds( args.Seconds );
			_rosterVersion = args.Version;
		}

		private void OnCountdownCancelled( in LobbyCountdownCancelledEventArgs args )
		{
			if ( IsHost || IsStaleRosterVersion( args.Version ) ) {
				return;
			}

			_countdownSeconds = 0;
			_countdownEndsUtc = default;
			_rosterVersion = args.Version;
		}

		private void OnGameStartRequested( in LobbyGameStartRequestedEventArgs args )
		{
			if ( IsHost || IsStaleRosterVersion( args.Version ) ) {
				return;
			}

			_rosterVersion = args.Version;
		}

		private bool TransitionWaitingRoomState( LobbyWaitingRoomState nextState, bool allowSameState = false )
		{
			return _stateMachine.TransitionHost( this, nextState, ReplicateWaitingRoomState, allowSameState );
		}

		private void ReplicateWaitingRoomState( LobbyWaitingRoomState previousState, LobbyWaitingRoomState currentState, uint version )
		{
			var payload = new LobbyWaitingRoomStateChangedEventArgs( previousState, currentState, version );
			PublishHostEvent( _waitingRoomStateChanged, in payload, NetworkSendMode.Reliable );
		}

		private void EvaluateReadyGate()
		{
			if ( !IsHost || !_stateMachine.IsAny( LobbyWaitingRoomState.WaitingForPlayers, LobbyWaitingRoomState.ReadyCheck, LobbyWaitingRoomState.Countdown ) ) {
				return;
			}

			if ( GetPeerCount() < _minPlayers ) {
				if ( _stateMachine.Is( LobbyWaitingRoomState.Countdown ) ) {
					CancelCountdown();
				}
				TransitionWaitingRoomState( LobbyWaitingRoomState.WaitingForPlayers );
				return;
			}

			if ( AreAllPlayersReady() ) {
				if ( !_stateMachine.Is( LobbyWaitingRoomState.Countdown ) ) {
					TransitionWaitingRoomState( LobbyWaitingRoomState.ReadyCheck );
					StartCountdown( DEFAULT_COUNTDOWN_SECONDS );
				}
			} else {
				if ( _stateMachine.Is( LobbyWaitingRoomState.Countdown ) ) {
					CancelCountdown();
				}
				TransitionWaitingRoomState( LobbyWaitingRoomState.ReadyCheck );
			}
		}

		private bool AreAllPlayersReady()
		{
			if ( GetPeerCount() < _minPlayers || _readyByPeer.Count == 0 ) {
				return false;
			}

			foreach ( LobbyReadyState readyState in _readyByPeer.Values ) {
				if ( readyState != LobbyReadyState.Ready ) {
					return false;
				}
			}

			return true;
		}

		private void SyncPeersFromSession()
		{
			if ( CurrentSession == null ) {
				return;
			}

			AddPeerLocal( CurrentSession.LocalPeerId, LobbyReadyState.NotReady );

			IReadOnlyList<NetworkPeerInfo> peers = CurrentSession.Peers;
			for ( int i = 0; i < peers.Count; i++ ) {
				AddPeerLocal( peers[i].PeerId, LobbyReadyState.NotReady );
			}
		}

		private bool AddPeerLocal( PeerId peerId, LobbyReadyState initialReadyState )
		{
			if ( !peerId.IsValid || _readyByPeer.ContainsKey( peerId ) ) {
				return false;
			}

			_readyByPeer.Add( peerId, initialReadyState );
			AdvanceRosterVersion();
			return true;
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

		private LobbyWaitingRoomInfo CreateInfoSnapshot()
		{
			Guid lobbyId = CurrentSession != null ? CurrentSession.SessionId : Guid.Empty;
			int countdownRemaining = 0;
			if ( _stateMachine.Is( LobbyWaitingRoomState.Countdown ) && _countdownEndsUtc > DateTime.UtcNow ) {
				countdownRemaining = Math.Max( 0, (int)Math.Ceiling( (_countdownEndsUtc - DateTime.UtcNow).TotalSeconds ) );
			}

			return new LobbyWaitingRoomInfo {
				LobbyId = lobbyId,
				State = State,
				MinPlayers = _minPlayers,
				MaxPlayers = CurrentSession != null ? CurrentSession.MaxPlayers : 0,
				PlayerCount = GetPeerCount(),
				ReadyCount = GetReadyCount(),
				CanStart = CanStart,
				CanReady = CanReady,
				LateJoinAllowed = _lateJoinAllowed,
				CountdownSecondsRemaining = countdownRemaining,
				StateVersion = StateVersion,
				RosterVersion = RosterVersion
			};
		}

		private uint AdvanceRosterVersion()
		{
			unchecked { _rosterVersion++; }
			return _rosterVersion;
		}

		private bool IsStaleRosterVersion( uint version )
		{
			return version != 0 && _rosterVersion != 0 && version <= _rosterVersion;
		}

		private void ClearRosterLocal()
		{
			_readyByPeer.Clear();
			_rosterVersion = 0;
			_countdownEndsUtc = default;
			_countdownSeconds = 0;
		}

		private static bool IsValidStateTransition( LobbyWaitingRoomState currentState, LobbyWaitingRoomState nextState )
		{
			if ( nextState == LobbyWaitingRoomState.Count ) {
				return false;
			}

			if ( currentState == nextState ) {
				return true;
			}

			switch ( currentState ) {
				case LobbyWaitingRoomState.Closed:
					return nextState == LobbyWaitingRoomState.Opening || nextState == LobbyWaitingRoomState.WaitingForPlayers;
				case LobbyWaitingRoomState.Opening:
					return nextState == LobbyWaitingRoomState.WaitingForPlayers || nextState == LobbyWaitingRoomState.Closed;
				case LobbyWaitingRoomState.WaitingForPlayers:
					return nextState == LobbyWaitingRoomState.ReadyCheck || nextState == LobbyWaitingRoomState.Countdown || nextState == LobbyWaitingRoomState.Starting || nextState == LobbyWaitingRoomState.Closed;
				case LobbyWaitingRoomState.ReadyCheck:
					return nextState == LobbyWaitingRoomState.WaitingForPlayers || nextState == LobbyWaitingRoomState.Countdown || nextState == LobbyWaitingRoomState.Starting || nextState == LobbyWaitingRoomState.Closed;
				case LobbyWaitingRoomState.Countdown:
					return nextState == LobbyWaitingRoomState.ReadyCheck || nextState == LobbyWaitingRoomState.Starting || nextState == LobbyWaitingRoomState.Closed;
				case LobbyWaitingRoomState.Starting:
					return nextState == LobbyWaitingRoomState.InGame || nextState == LobbyWaitingRoomState.Closed;
				case LobbyWaitingRoomState.InGame:
					return nextState == LobbyWaitingRoomState.Closed;
				default:
					return false;
			}
		}
	};
};
