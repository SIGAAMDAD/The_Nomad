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
using Nomad.Game.Sdk.Multiplayer.Match;
using Nomad.Game.Sdk.Multiplayer.Team;
using Nomad.Game.Sdk.Events.Multiplayer;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Multiplayer;

namespace Nomad.Game.Multiplayer.Match
{
	internal sealed class MatchScoreService : IMatchScoreService
	{
		public uint ScoreRevision {
			get {
				throw new System.NotImplementedException();
			}
		}

		public IGameEvent<MatchScoreChangedEventArgs> ScoreChanged => _scoreChanged;
		private readonly IGameEvent<MatchScoreChangedEventArgs> _scoreChanged = null;

		public IGameEvent<MatchScoreLimitReachedEventArgs> ScoreLimitReached => _scoreLimitReached;
		private readonly IGameEvent<MatchScoreLimitReachedEventArgs> _scoreLimitReached = null;

		public MatchScoreService( IGameEventRegistryService eventFactory )
		{
			_scoreChanged = eventFactory.GetEvent<MatchScoreChangedEventArgs>(
				MatchScoreChangedEventArgs.Name,
				MatchScoreChangedEventArgs.NameSpace
			);

			_scoreLimitReached = eventFactory.GetEvent<MatchScoreLimitReachedEventArgs>(
				MatchScoreLimitReachedEventArgs.Name,
				MatchScoreLimitReachedEventArgs.NameSpace
			);
		}

		public void AddPlayerScore( MatchScoreDelta delta )
		{
			throw new System.NotImplementedException();
		}

		public void Initialize( MatchRules rules, IReadOnlyList<PeerId> players )
		{
			throw new System.NotImplementedException();
		}
	};
};
