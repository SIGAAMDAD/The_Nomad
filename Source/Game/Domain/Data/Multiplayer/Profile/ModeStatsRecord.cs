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

namespace Nomad.Game.Domain.Data.Multiplayer.Profile
{
	public sealed record ModeStatsRecord
	{
		public Mode ModeId { get; init; }

		public uint MatchesPlayed { get; init; }
		public uint MatchesWon { get; init; }
		public uint MatchesLost { get; init; }
		public uint MatchesAbandoned { get; init; }
		public uint ScoreEarned { get; init; }
		public uint Kills { get; init; }
		public uint Deaths { get; init; }
		public uint Assists { get; init; }
		public uint ObjectiveScore { get; init; }
		public uint TimePlayedSeconds { get; init; }

		public float WinRate => MatchesPlayed == 0 ? 0.0f : (float)MatchesWon / MatchesPlayed;
	};
};
