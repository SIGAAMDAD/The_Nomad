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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer.Team;
using Nomad.Game.Domain.Events.Multiplayer;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer.Team
{
	/*
	===================================================================================

	TeamService

	===================================================================================
	*/
	/// <summary>
	/// Host-authoritative replicated team state.
	///
	/// Clients request changes with RPCs.
	/// The host validates and publishes replicated team events.
	/// Clients apply replicated team events locally.
	/// </summary>

	internal sealed class TeamService : MultiplayerObject, ITeamService
	{
		public TeamServiceState State => _stateMachine.State;
		public uint StateVersion => _stateMachine.StateVersion;
		public uint TeamVersion => _teamVersion;

		public bool CanRequestTeamChange => IsSessionActive && CanChangeTeamAssignments();

		public IGameEvent<TeamChangedEventArgs> TeamChanged => _teamChanged;
		private readonly IGameEvent<TeamChangedEventArgs> _teamChanged;

		public IGameEvent<TeamServiceStateChangedEventArgs> TeamServiceStateChanged => _teamServiceStateChanged;
		private readonly IGameEvent<TeamServiceStateChangedEventArgs> _teamServiceStateChanged;

		public IGameEvent<TeamsResetEventArgs> TeamsReset => _teamsReset;
		private readonly IGameEvent<TeamsResetEventArgs> _teamsReset;

		private readonly INetworkSessionService _sessionService;
		private readonly MultiplayerStateMachine<TeamServiceState> _stateMachine;

		private readonly List<TeamDefinition> _teams = new List<TeamDefinition>( 4 );
		private readonly Dictionary<TeamId, List<PeerId>> _membersByTeam = new Dictionary<TeamId, List<PeerId>>();
		private readonly Dictionary<PeerId, TeamId> _teamByPeer = new Dictionary<PeerId, TeamId>();

		private uint _teamVersion = 0;

		/*
		===============
		TeamService
		===============
		*/
		public TeamService(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory,
			IReadOnlyList<TeamDefinition>? teams = null
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
			_sessionService = sessionService ?? throw new ArgumentNullException( nameof( sessionService ) );

			_stateMachine = new MultiplayerStateMachine<TeamServiceState>(
				TeamServiceState.Open,
				IsValidStateTransition
			);

			_teamChanged = GetEvent<TeamChangedEventArgs>(
				TeamChangedEventArgs.Name,
				TeamChangedEventArgs.NameSpace
			);

			_teamServiceStateChanged = GetEvent<TeamServiceStateChangedEventArgs>(
				TeamServiceStateChangedEventArgs.Name,
				TeamServiceStateChangedEventArgs.NameSpace
			);

			_teamsReset = GetEvent<TeamsResetEventArgs>(
				TeamsResetEventArgs.Name,
				TeamsResetEventArgs.NameSpace
			);

			RegisterNetworkEvent( MessageIds.TeamChanged, _teamChanged );
			RegisterNetworkEvent( MessageIds.TeamServiceStateChanged, _teamServiceStateChanged );
			RegisterNetworkEvent( MessageIds.TeamsReset, _teamsReset );

			RegisterRpc<TeamJoinRequestRpc>( MessageIds.TeamJoinRequestRpc, OnJoinTeamRequest );
			RegisterRpc<TeamLeaveRequestRpc>( MessageIds.TeamLeaveRequestRpc, OnLeaveTeamRequest );
			RegisterRpc<TeamAutoAssignRequestRpc>( MessageIds.TeamAutoAssignRequestRpc, OnAutoAssignRequest );

			Subscribe( _teamChanged, OnTeamChanged );
			Subscribe( _teamServiceStateChanged, OnTeamServiceStateChanged );
			Subscribe( _teamsReset, OnTeamsReset );

			Subscribe( _sessionService.PeerDisconnected, OnPeerDisconnected );
			Subscribe( _sessionService.SessionChanged, OnSessionChanged );

			if ( teams == null ) {
				AddDefaultTeams();
			} else {
				foreach ( TeamDefinition team in teams ) {
					AddTeamDefinition( team );
				}
			}
		}

		/*
		===============
		RequestJoinTeam
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="teamId"></param>
		/// <returns></returns>
		public bool RequestJoinTeam( TeamId teamId )
		{
			if ( !IsSessionActive || !LocalPeerId.IsValid ) {
				return false;
			}

			if ( IsHost ) {
				return SetPeerTeamInternal(
					LocalPeerId,
					teamId,
					TeamChangeReason.Requested,
					force: false
				);
			}

			return SendRpcToHost(
				new TeamJoinRequestRpc( LocalPeerId, teamId ),
				NetworkSendMode.Reliable
			);
		}

		/*
		===============
		RequestLeaveTeam
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool RequestLeaveTeam()
		{
			if ( !IsSessionActive || !LocalPeerId.IsValid ) {
				return false;
			}

			if ( IsHost ) {
				return RemovePeer( LocalPeerId, TeamChangeReason.Left );
			}

			return SendRpcToHost(
				new TeamLeaveRequestRpc( LocalPeerId ),
				NetworkSendMode.Reliable
			);
		}

		/*
		===============
		RequestAutoAssign
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool RequestAutoAssign()
		{
			if ( !IsSessionActive || !LocalPeerId.IsValid ) {
				return false;
			}

			if ( IsHost ) {
				return AutoAssignPeer( LocalPeerId );
			}

			return SendRpcToHost(
				new TeamAutoAssignRequestRpc( LocalPeerId ),
				NetworkSendMode.Reliable
			);
		}

		/*
		===============
		SetPeerTeam
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <param name="teamId"></param>
		/// <returns></returns>
		public bool SetPeerTeam( PeerId peerId, TeamId teamId )
		{
			if ( !IsHost ) {
				return false;
			}
			return SetPeerTeamInternal(
				peerId,
				teamId,
				TeamChangeReason.Assigned,
				force: false
			);
		}

		/*
		===============
		AutoAssignPeer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <returns></returns>
		public bool AutoAssignPeer( PeerId peerId )
		{
			if ( !IsHost || !CanChangeTeamAssignments() || !IsPeerInSession( peerId ) ) {
				return false;
			}

			if ( !TryFindBestTeam( out TeamId teamId ) ) {
				return false;
			}

			return SetPeerTeamInternal(
				peerId,
				teamId,
				TeamChangeReason.AutoAssigned,
				force: false
			);
		}

		/*
		===============
		RemovePeer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public bool RemovePeer( PeerId peerId, TeamChangeReason reason = TeamChangeReason.Left )
		{
			if ( !IsHost || !peerId.IsValid ) {
				return false;
			}

			if ( !_teamByPeer.TryGetValue( peerId, out TeamId previousTeam ) ) {
				return false;
			}

			AdvanceTeamVersion();

			ApplyTeamChange(
				peerId,
				TeamId.None,
				_teamVersion
			);

			return PublishHostEvent(
				_teamChanged,
				new TeamChangedEventArgs(
					peerId,
					previousTeam,
					TeamId.None,
					reason,
					_teamVersion
				),
				NetworkSendMode.Reliable
			);
		}

		/*
		===============
		IsPeerAssigned
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <returns></returns>
		public bool IsPeerAssigned( PeerId peerId )
		{
			return TryGetTeam( peerId, out var teamId ) && teamId != TeamId.None;
		}

		/*
		===============
		OpenTeams
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool OpenTeams()
		{
			if ( !IsHost ) {
				return false;
			}
			return TransitionHostState( TeamServiceState.Open );
		}

		/*
		===============
		LockTeams
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool LockTeams()
		{
			if ( !IsHost ) {
				return false;
			}
			return TransitionHostState( TeamServiceState.Locked );
		}

		/*
		===============
		BeginMatch
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool BeginMatch()
		{
			if ( !IsHost ) {
				return false;
			}
			return TransitionHostState( TeamServiceState.MatchActive );
		}

		/*
		===============
		CloseTeams
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool CloseTeams()
		{
			if ( !IsHost ) {
				return false;
			}
			return TransitionHostState( TeamServiceState.Closed );
		}

		/*
		===============
		ResetTeams
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool ResetTeams()
		{
			if ( !IsHost ) {
				return false;
			}

			ClearTeamsLocal();
			AdvanceTeamVersion();

			return PublishHostEvent(
				_teamsReset,
				new TeamsResetEventArgs( _teamVersion ),
				NetworkSendMode.Reliable
			);
		}

		/*
		===============
		TryGetTeam
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <param name="teamId"></param>
		/// <returns></returns>
		public bool TryGetTeam( PeerId peerId, out TeamId teamId )
		{
			return _teamByPeer.TryGetValue( peerId, out teamId );
		}

		/*
		===============
		TryGetTeamInfo
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="teamId"></param>
		/// <param name="info"></param>
		/// <returns></returns>
		public bool TryGetTeamInfo( TeamId teamId, out TeamInfo info )
		{
			for ( int i = 0; i < _teams.Count; i++ ) {
				TeamDefinition team = _teams[i];
				if ( team.Id != teamId ) {
					continue;
				}

				info = new TeamInfo(
					team.Id,
					team.Name,
					team.MaxMembers,
					GetTeamMemberCount( team.Id ),
					team.IsPlayable
				);

				return true;
			}

			info = default;
			return false;
		}

		/*
		===============
		GetTeamMemberCount
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="teamId"></param>
		/// <returns></returns>
		public int GetTeamMemberCount( TeamId teamId )
		{
			return _membersByTeam.TryGetValue( teamId, out List<PeerId>? members )
				? members.Count
				: 0;
		}

		/*
		===============
		CopyTeamMembers
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="teamId"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public int CopyTeamMembers( TeamId teamId, PeerId[] destination )
		{
			ArgumentGuard.ThrowIfNull( destination, nameof( destination ) );

			if ( !_membersByTeam.TryGetValue( teamId, out List<PeerId>? members ) ) {
				return 0;
			}

			int count = Math.Min( destination.Length, members.Count );
			for ( int i = 0; i < count; i++ ) {
				destination[i] = members[i];
			}

			return count;
		}

		/*
		===============
		OnJoinTeamRequest
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="context"></param>
		/// <param name="rpc"></param>
		[RpcMethod( "TeamJoinRequestRpc" )]
		[RpcMethodPayload( "PeerId", typeof( PeerId ), Order = 1 )]
		[RpcMethodPayload( "TeamId", typeof( TeamId ), Order = 2 )]
		private void OnJoinTeamRequest( in NetworkRpcContext context, in TeamJoinRequestRpc rpc )
		{
			if ( !IsHost ) {
				return;
			}

			SetPeerTeamInternal(
				rpc.PeerId,
				rpc.TeamId,
				TeamChangeReason.Requested,
				force: false
			);
		}

		/*
		===============
		OnLeaveTeamRequest
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="context"></param>
		/// <param name="rpc"></param>
		[RpcMethod( "TeamLeaveRequestRpc" )]
		[RpcMethodPayload( "PeerId", typeof( PeerId ) )]
		private void OnLeaveTeamRequest( in NetworkRpcContext context, in TeamLeaveRequestRpc rpc )
		{
			if ( !IsHost ) {
				return;
			}

			RemovePeer( rpc.PeerId, TeamChangeReason.Left );
		}

		/*
		===============
		OnAutoAssignRequest
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="context"></param>
		/// <param name="rpc"></param>
		[RpcMethod( "TeamAutoAssignRequestRpc" )]
		[RpcMethodPayload( "PeerId", typeof( PeerId ) )]
		private void OnAutoAssignRequest( in NetworkRpcContext context, in TeamAutoAssignRequestRpc rpc )
		{
			if ( !IsHost ) {
				return;
			}

			AutoAssignPeer( rpc.PeerId );
		}

		/*
		===============
		OnPeerDisconnected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnPeerDisconnected( in PeerDisconnectedEventArgs args )
		{
			if ( !IsHost ) {
				return;
			}

			RemovePeer( args.PeerId, TeamChangeReason.PeerDisconnected );
		}

		/*
		===============
		OnSessionChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnSessionChanged( in NetworkSessionChangedEventArgs args )
		{
			if ( IsSessionActive ) {
				return;
			}

			ClearTeamsLocal();
			_teamVersion = 0;
		}

		/*
		===============
		OnTeamChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnTeamChanged( in TeamChangedEventArgs args )
		{
			if ( IsHost ) {
				return;
			}

			if ( IsStaleTeamVersion( args.Version ) ) {
				return;
			}

			ApplyTeamChange(
				args.PeerId,
				args.CurrentTeam,
				args.Version
			);
		}

		/*
		===============
		OnTeamServiceStateChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnTeamServiceStateChanged( in TeamServiceStateChangedEventArgs args )
		{
			if ( IsHost ) {
				return;
			}

			_stateMachine.ApplyReplicated(
				args.CurrentState,
				args.Version
			);
		}

		/*
		===============
		OnTeamsReset
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnTeamsReset( in TeamsResetEventArgs args )
		{
			if ( IsHost ) {
				return;
			}

			if ( IsStaleTeamVersion( args.Version ) ) {
				return;
			}

			ClearTeamsLocal();
			_teamVersion = args.Version;
		}

		/*
		===============
		OnStateTransitioned
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="previousState"></param>
		/// <param name="currentState"></param>
		/// <param name="stateVersion"></param>
		private void OnStateTransitioned(
			TeamServiceState previousState,
			TeamServiceState currentState,
			uint stateVersion
		)
		{
			if ( !IsHost ) {
				return;
			}

			PublishHostEvent(
				_teamServiceStateChanged,
				new TeamServiceStateChangedEventArgs(
					previousState,
					currentState,
					stateVersion
				),
				NetworkSendMode.Reliable
			);
		}

		/*
		===============
		SetPeerTeamInternal
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <param name="teamId"></param>
		/// <param name="reason"></param>
		/// <param name="force"></param>
		/// <returns></returns>
		private bool SetPeerTeamInternal(
			PeerId peerId,
			TeamId teamId,
			TeamChangeReason reason,
			bool force
		)
		{
			if ( !IsHost || !peerId.IsValid ) {
				return false;
			}

			if ( !force && !CanChangeTeamAssignments() ) {
				return false;
			}

			if ( !IsPeerInSession( peerId ) ) {
				return false;
			}

			if ( teamId != TeamId.None && !IsKnownTeam( teamId ) ) {
				return false;
			}

			if ( teamId != TeamId.None && !force && IsTeamFull( teamId ) ) {
				return false;
			}

			_teamByPeer.TryGetValue( peerId, out TeamId previousTeam );
			if ( previousTeam == teamId ) {
				return true;
			}

			AdvanceTeamVersion();

			ApplyTeamChange(
				peerId,
				teamId,
				_teamVersion
			);

			return PublishHostEvent(
				_teamChanged,
				new TeamChangedEventArgs(
					peerId,
					previousTeam,
					teamId,
					reason,
					_teamVersion
				),
				NetworkSendMode.Reliable
			);
		}

		/*
		===============
		ApplyTeamChange
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <param name="teamId"></param>
		/// <param name="version"></param>
		private void ApplyTeamChange( PeerId peerId, TeamId teamId, uint version )
		{
			if ( _teamByPeer.TryGetValue( peerId, out TeamId previousTeam ) ) {
				RemoveFromTeamList( previousTeam, peerId );
			}

			if ( teamId == TeamId.None ) {
				_teamByPeer.Remove( peerId );
				_teamVersion = version;
				return;
			}

			if ( !_membersByTeam.TryGetValue( teamId, out List<PeerId>? members ) ) {
				members = new List<PeerId>( 8 );
				_membersByTeam[teamId] = members;
			}

			if ( !members.Contains( peerId ) ) {
				members.Add( peerId );
			}

			_teamByPeer[peerId] = teamId;
			_teamVersion = version;
		}

		/*
		===============
		RemoveFromTeamList
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="teamId"></param>
		/// <param name="peerId"></param>
		private void RemoveFromTeamList( TeamId teamId, PeerId peerId )
		{
			if ( !_membersByTeam.TryGetValue( teamId, out List<PeerId>? members ) ) {
				return;
			}

			for ( int i = 0; i < members.Count; i++ ) {
				if ( members[i] != peerId ) {
					continue;
				}

				members.RemoveAt( i );
				return;
			}
		}

		/*
		===============
		TryFindBestTeam
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="teamId"></param>
		/// <returns></returns>
		private bool TryFindBestTeam( out TeamId teamId )
		{
			teamId = TeamId.None;

			int bestCount = int.MaxValue;

			for ( int i = 0; i < _teams.Count; i++ ) {
				TeamDefinition team = _teams[i];

				if ( !team.IsPlayable || team.Id == TeamId.None || team.Id == TeamId.Spectator ) {
					continue;
				}

				int count = GetTeamMemberCount( team.Id );

				if ( team.HasLimit && count >= team.MaxMembers ) {
					continue;
				}

				if ( count >= bestCount ) {
					continue;
				}

				bestCount = count;
				teamId = team.Id;
			}

			return teamId != TeamId.None;
		}

		/*
		===============
		IsPeerInSession
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <returns></returns>
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

		/*
		===============
		IsKnownTeam
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="teamId"></param>
		/// <returns></returns>
		private bool IsKnownTeam( TeamId teamId )
		{
			for ( int i = 0; i < _teams.Count; i++ ) {
				if ( _teams[i].Id == teamId ) {
					return true;
				}
			}

			return false;
		}

		/*
		===============
		IsTeamFull
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="teamId"></param>
		/// <returns></returns>
		private bool IsTeamFull( TeamId teamId )
		{
			for ( int i = 0; i < _teams.Count; i++ ) {
				TeamDefinition team = _teams[i];

				if ( team.Id != teamId ) {
					continue;
				}

				return team.HasLimit && GetTeamMemberCount( teamId ) >= team.MaxMembers;
			}

			return true;
		}

		/*
		===============
		CanChangeTeamAssignments
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private bool CanChangeTeamAssignments()
		{
			return _stateMachine.Is( TeamServiceState.Open );
		}

		/*
		===============
		TransitionHostState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="nextState"></param>
		/// <returns></returns>
		private bool TransitionHostState( TeamServiceState nextState )
		{
			if ( !IsHost ) {
				return false;
			}

			return _stateMachine.TransitionTo( nextState );
		}

		/*
		===============
		AddDefaultTeams
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void AddDefaultTeams()
		{
			AddTeamDefinition( new TeamDefinition( TeamId.Spectator, "Spectator", 0, false ) );
			AddTeamDefinition( new TeamDefinition( TeamId.Red, "Red", 0, true ) );
			AddTeamDefinition( new TeamDefinition( TeamId.Blue, "Blue", 0, true ) );
		}

		/*
		===============
		AddTeamDefinition
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="team"></param>
		private void AddTeamDefinition( TeamDefinition team )
		{
			if ( team.Id == TeamId.None || team.Id == TeamId.Count ) {
				return;
			}

			for ( int i = 0; i < _teams.Count; i++ ) {
				if ( _teams[i].Id == team.Id ) {
					_teams[i] = team;
					return;
				}
			}

			_teams.Add( team );

			if ( !_membersByTeam.ContainsKey( team.Id ) ) {
				_membersByTeam.Add( team.Id, new List<PeerId>( 8 ) );
			}
		}

		/*
		===============
		ClearTeamsLocal
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void ClearTeamsLocal()
		{
			_teamByPeer.Clear();

			foreach ( List<PeerId> members in _membersByTeam.Values ) {
				members.Clear();
			}
		}

		/*
		===============
		AdvanceTeamVersion
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private uint AdvanceTeamVersion()
		{
			unchecked {
				_teamVersion++;
			}

			return _teamVersion;
		}

		/*
		===============
		IsStaleTeamVersion
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		private bool IsStaleTeamVersion( uint version )
		{
			if ( version == 0 || _teamVersion == 0 ) {
				return false;
			}

			return version <= _teamVersion;
		}

		/*
		===============
		TransitionTeamState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		private bool TransitionTeamState( TeamServiceState state )
		{
			return _stateMachine.TransitionHost(
				this,
				state,
				ReplicateTeamServiceState
			);
		}

		/*
		===============
		ReplicateTeamServiceState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="previousState"></param>
		/// <param name="currentState"></param>
		/// <param name="version"></param>
		private void ReplicateTeamServiceState( TeamServiceState previousState, TeamServiceState currentState, uint version )
		{
			PublishHostEvent(
				_teamServiceStateChanged,
				new TeamServiceStateChangedEventArgs(
					previousState,
					currentState,
					version
				),
				NetworkSendMode.Reliable
			);
		}

		/*
		===============
		IsValidStateTransition
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="currentState"></param>
		/// <param name="nextState"></param>
		/// <returns></returns>
		private static bool IsValidStateTransition( TeamServiceState currentState, TeamServiceState nextState )
		{
			if ( nextState == TeamServiceState.Count ) {
				return false;
			}

			if ( currentState == nextState ) {
				return true;
			}

			switch ( currentState ) {
				case TeamServiceState.Disabled:
					return nextState == TeamServiceState.Open
						|| nextState == TeamServiceState.Closed;

				case TeamServiceState.Open:
					return nextState == TeamServiceState.Locked
						|| nextState == TeamServiceState.MatchActive
						|| nextState == TeamServiceState.Closed
						|| nextState == TeamServiceState.Disabled;

				case TeamServiceState.Locked:
					return nextState == TeamServiceState.Open
						|| nextState == TeamServiceState.MatchActive
						|| nextState == TeamServiceState.Closed;

				case TeamServiceState.MatchActive:
					return nextState == TeamServiceState.Closed;

				case TeamServiceState.Closed:
					return nextState == TeamServiceState.Open
						|| nextState == TeamServiceState.Disabled;

				default:
					return false;
			}
		}
	};
};
