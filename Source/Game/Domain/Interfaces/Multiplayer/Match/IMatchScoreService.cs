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

using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Multiplayer.Match;
using Nomad.Game.Domain.Data.Multiplayer.Team;
using Nomad.Game.Domain.Events.Multiplayer;

namespace Nomad.Game.Domain.Interfaces.Multiplayer
{
	public interface IMatchScoreService
	{
		uint ScoreRevision { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Multiplayer", PayloadName = "MatchScoreChangedEventArgs" )]
		[EventPayload( "ScoreRevision", typeof( uint ), Order = 1 )]
		[EventPayload( "Delta", typeof( MatchScoreDelta ), Order = 2 )]
		[EventPayload( "NewPlayerScore", typeof( uint ), Order = 3 )]
		[EventPayload( "NewTeamScore", typeof( uint ), Order = 4 )]
		IGameEvent<MatchScoreChangedEventArgs> ScoreChanged { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Multiplayer", PayloadName = "MatchScoreLimitReachedEventArgs" )]
		[EventPayload( "ScoreRevision", typeof( uint ), Order = 1 )]
		[EventPayload( "PlayerId", typeof( PeerId ), Order = 2 )]
		[EventPayload( "Team", typeof( TeamId ), Order = 3 )]
		[EventPayload( "Score", typeof( int ), Order = 4 )]
		[EventPayload( "ServerTick", typeof( uint ), Order = 5 )]
		IGameEvent<MatchScoreLimitReachedEventArgs> ScoreLimitReached { get; }

		void Initialize( MatchRules rules, IReadOnlyList<PeerId> players );
		void AddPlayerScore( MatchScoreDelta delta );
	};
};
