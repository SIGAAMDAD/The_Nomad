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
	public sealed record CombatStatsRecord
	{
		public uint Kills { get; init; }
		public uint Deaths { get; init; }
		public uint Assists { get; init; }
		public uint Executions { get; init; }
		public uint Headshots { get; init; }

		public uint ParryKills { get; init; }
		public uint PerfectParries { get; init; }
		public uint DashOverheatKills { get; init; }

		public uint DamageDealt { get; init; }
		public uint DamageTaken { get; init; }

		public uint ShotsFired { get; init; }
		public uint ShotsHit { get; init; }

		public float KillDeathRatio => Deaths == 0 ? Kills : (float)Kills / Deaths;
		public float Accuracy => ShotsFired == 0 ? 0f : (float)ShotsHit / ShotsFired;
	};
};
