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
using Godot;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer.Match;
using Nomad.Game.Domain.Events.Multiplayer.Match;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer.Match
{
	/*
	===================================================================================

	MatchFlowService

	===================================================================================
	*/
	/// <summary>
	/// Owns the match lifecycle.
	/// </summary>

	internal sealed class MatchFlowService : MultiplayerObject, IMatchFlowService
	{
		public MatchPhase Phase => _phase;
		private MatchPhase _phase = MatchPhase.None;

		public uint MatchRevision => _matchRevision;
		private uint _matchRevision = 0;

		public uint PhaseStartTick => _phaseStartTick;
		private uint _phaseStartTick = 0;

		public uint PhaseEndTick => _phaseEndTick;
		private uint _phaseEndTick = 0;

		public bool IsMatchActive {
			get {
				throw new NotImplementedException();
			}
		}

		public bool IsMatchEnding {
			get {
				throw new NotImplementedException();
			}
		}

		public IGameEvent<MatchPhaseChangedEventArgs> PhaseChanged => _phaseChanged;
		private readonly IGameEvent<MatchPhaseChangedEventArgs> _phaseChanged = null;

		public IGameEvent<MatchStartedEventArgs> MatchStarted => _matchStarted;
		private readonly IGameEvent<MatchStartedEventArgs> _matchStarted = null;

		public IGameEvent<MatchEndedEventArgs> MatchEnded => _matchEnded;
		private readonly IGameEvent<MatchEndedEventArgs> _matchEnded = null;

		public MatchFlowService(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
			RegisterRpc<MatchPhaseChangedRpc>( MessageIds.MatchPhaseChangedRpc, OnMatchPhaseChanged );
		}

		public void BeginCountdown( uint countdown )
		{
		}

		[RpcMethod( "MatchPhaseChangedRpc" )]
		[RpcMethodPayload( "MatchRevision", typeof( uint ), Order = 1 )]
		[RpcMethodPayload( "Phase", typeof( MatchPhase ), Order = 2 )]
		[RpcMethodPayload( "PhaseStartTick", typeof( uint ), Order = 3 )]
		[RpcMethodPayload( "PhaseEndTick", typeof( uint ), Order = 4 )]
		private void OnMatchPhaseChanged( in NetworkRpcContext context, in MatchPhaseChangedRpc rpc )
		{
			if ( !context.FromHost ) {
				return;
			}
			if ( rpc.MatchRevision <= _matchRevision ) {
				return;
			}

			_matchRevision = rpc.MatchRevision;

			var previousPhase = _phase;
			_phase = rpc.Phase;

			_phaseStartTick = rpc.PhaseStartTick;
			_phaseEndTick = rpc.PhaseEndTick;

			_phaseChanged.Publish( new MatchPhaseChangedEventArgs(
				previousPhase,
				rpc.Phase
			) );
		}
	};
};
