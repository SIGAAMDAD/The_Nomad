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
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Application.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer.Modes;
using Nomad.Game.Domain.Events.Multiplayer;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Networking.Events;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer.Modes
{
	/*
	===================================================================================

	DuelMode

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class DuelMode : ModeBase, IDuelMode
	{
		private enum DuelRoundState : byte
		{
			Waiting,
			Active,
			Ended
		};

		private struct HostState
		{
			public Guid SessionId { get; set; }
			public PeerId Player1Id { get; set; }
			public PeerId Player2Id { get; set; }
			public byte Player1Score { get; set; }
			public byte Player2Score { get; set; }
			public byte RoundIndex { get; set; }
		};

		public override string ModeName => "Duel";
		public override Mode Mode => Mode.Duel;

		public IGameEvent<DuelRoundEndEventArgs> DuelRoundEnd => _duelRoundEnd;
		private readonly IGameEvent<DuelRoundEndEventArgs> _duelRoundEnd = default;

		public IGameEvent<DuelRoundBeginEventArgs> DuelRoundBegin => _duelRoundBegin;
		private readonly IGameEvent<DuelRoundBeginEventArgs> _duelRoundBegin = default;

		public DuelInstanceData Snapshot => new DuelInstanceData { Player1Score = _hostState.Player1Score, Player2Score = _hostState.Player2Score, RoundIndex = _hostState.RoundIndex };

		private HostState _hostState;
		private readonly MultiplayerStateMachine<DuelRoundState> _roundFlow;

		/*
		===============
		DuelMode
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="sessionService"></param>
		/// <param name="rpcBus"></param>
		/// <param name="eventBus"></param>
		/// <param name="eventFactory"></param>
		public DuelMode( INetworkSessionService sessionService, INetworkRpcBus rpcBus, INetworkEventBus eventBus, IGameEventRegistryService eventFactory )
			: base( sessionService, rpcBus, eventBus, eventFactory )
		{
			_roundFlow = CreateStateMachine<DuelRoundState>();

			_duelRoundBegin = GetEvent<DuelRoundBeginEventArgs>(
				DuelRoundBeginEventArgs.Name,
				DuelRoundBeginEventArgs.NameSpace
			);

			_duelRoundEnd = GetEvent<DuelRoundEndEventArgs>(
				DuelRoundEndEventArgs.Name,
				DuelRoundEndEventArgs.NameSpace
			);

			RegisterNetworkEvent( _duelRoundBegin );
			RegisterNetworkEvent( _duelRoundEnd );

			Subscribe( _duelRoundBegin, OnDuelRoundBegin );
			Subscribe( _duelRoundEnd, OnDuelRoundEnd );

			RefreshContenders( resetStateForNewSession: true );
		}

		/*
		===============
		TryBeginRound
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool TryBeginRound()
		{
			return PublishHostEvent( _duelRoundBegin, default );
		}

		/*
		===============
		TryEndRound
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="winnerId"></param>
		/// <param name="loserId"></param>
		/// <param name="wasTie"></param>
		/// <returns></returns>
		public bool TryEndRound( PeerId winnerId, PeerId loserId, bool wasTie )
		{
			return PublishHostEvent( _duelRoundEnd, new DuelRoundEndEventArgs( winnerId, loserId, wasTie ) );
		}

		/*
		===============
		OnDuelRoundBegin
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnDuelRoundBegin( in DuelRoundBeginEventArgs args )
		{
			RefreshContenders( resetStateForNewSession: true );
			_roundFlow.TransitionTo( DuelRoundState.Active, allowSameState: true );
			_hostState.RoundIndex++;
			AdvanceStateVersion();
		}

		/*
		===============
		OnDuelRoundEnd
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnDuelRoundEnd( in DuelRoundEndEventArgs args )
		{
			RefreshContenders( resetStateForNewSession: true );

			if ( !args.WasTie ) {
				ApplyScore( args.WinnerId );
			}

			_roundFlow.TransitionTo( DuelRoundState.Ended, allowSameState: true );
			AdvanceStateVersion();
		}

		/*
		===============
		ApplyScore
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="winnerId"></param>
		private void ApplyScore( PeerId winnerId )
		{
			if ( winnerId.Equals( _hostState.Player1Id ) ) {
				if ( _hostState.Player1Score < byte.MaxValue ) {
					_hostState.Player1Score++;
				}
				return;
			}

			if ( winnerId.Equals( _hostState.Player2Id ) && _hostState.Player2Score < byte.MaxValue ) {
				_hostState.Player2Score++;
			}
		}

		/*
		===============
		RefreshContenders
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="resetStateForNewSession"></param>
		private void RefreshContenders( bool resetStateForNewSession )
		{
			NetworkSessionInfo? session = CurrentSession;
			if ( session == null ) {
				_hostState = default;
				return;
			}

			if ( resetStateForNewSession && _hostState.SessionId != session.SessionId ) {
				_hostState = new HostState {
					SessionId = session.SessionId
				};
			} else {
				_hostState.SessionId = session.SessionId;
			}

			_hostState.Player1Id = session.HostPeerId;
			_hostState.Player2Id = peerRoster.GetFirstNonHostPeer()?.PeerId ?? default;
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
		protected override void OnSessionChanged( in NetworkSessionChangedEventArgs args )
		{
			RefreshContenders( resetStateForNewSession: true );
		}

		/*
		===============
		OnPeerConnected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		protected override void OnPeerConnected( in PeerConnectedEventArgs args )
		{
			RefreshContenders( resetStateForNewSession: false );
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
		protected override void OnPeerDisconnected( in PeerDisconnectedEventArgs args )
		{
			RefreshContenders( resetStateForNewSession: false );
		}
	};
};
