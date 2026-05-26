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
using Nomad.Game.Sdk.Multiplayer.Match;
using Nomad.Game.Sdk.Events.Multiplayer.Match;

namespace Nomad.Game.Sdk.Multiplayer.Match
{
	public interface IMatchFlowService : IDisposable
	{
		MatchPhase Phase { get; }

		uint MatchRevision { get; }
		uint PhaseStartTick { get; }
		uint PhaseEndTick { get; }

		bool IsMatchActive { get; }
		bool IsMatchEnding { get; }

		[Event( nameSpace: "Nomad.Game.Sdk.Events.Multiplayer.Match", PayloadName = "MatchPhaseChangedEventArgs" )]
		[EventPayload( "PreviousPhase", typeof( MatchPhase ), Order = 1 )]
		[EventPayload( "NewPhase", typeof( MatchPhase ), Order = 2 )]
		IGameEvent<MatchPhaseChangedEventArgs> PhaseChanged { get; }

		[Event( nameSpace: "Nomad.Game.Sdk.Events.Multiplayer.Match" )]
		[EventPayload( "MatchRevision", typeof( uint ), Order = 1 )]
		[EventPayload( "ServerTick", typeof( uint ), Order = 2 )]
		[EventPayload( "Rules", typeof( MatchRules ), Order = 3 )]
		IGameEvent<MatchStartedEventArgs> MatchStarted { get; }

		[Event( nameSpace: "Nomad.Game.Sdk.Events.Multiplayer.Match" )]
		[EventPayload( "Reason", typeof( MatchEndReason ), Order = 1 )]
		[EventPayload( "MatchRevision", typeof( uint ), Order = 2 )]
		[EventPayload( "ServerTick", typeof( uint ), Order = 3 )]
		IGameEvent<MatchEndedEventArgs> MatchEnded { get; }

		void BeginCountdown( uint serverTick );
	}
}
