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

using Nomad.Game.Domain.Data.Multiplayer.Objectives;

namespace Nomad.Game.Domain.Data.Multiplayer.Modes
{
	public readonly struct CaptureTheFlagSnapshot
	{
		public uint ScoreVersion { get; init; }
		public uint RedTeamScore { get; init; }
		public uint BlueTeamScore { get; init; }
		public uint RoundIndex { get; init; }

		public FlagObjectiveSnapshot RedFlagState { get; init; }
		public FlagObjectiveSnapshot BlueFlagState { get; init; }
	};
};
