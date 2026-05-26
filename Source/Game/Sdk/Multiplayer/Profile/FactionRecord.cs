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

namespace Nomad.Game.Sdk.Multiplayer.Profile
{
	public sealed record FactionRecord
	{
		public FactionId Id { get; init; }
		public string Name { get; init; }
		public string ShortName { get; init; }
		public string Description { get; init; }
		public string FlavorLine { get; init; }

		/// <summary>
		/// Faction ranks in ascending order.
		/// </summary>
		public IReadOnlyList<FactionRankRecord> Ranks { get; init; }

		/// <summary>
		/// Weekly contract pool for this faction.
		/// The live board chooses from this list each weekly refresh.
		/// </summary>
		public IReadOnlyList<WeeklyContractDefinitionRecord> WeeklyContractPool { get; init; }

		/// <summary>
		/// Rewards granted by faction standing/rank advancement.
		/// </summary>
		public IReadOnlyList<FactionRewardRecord> RewardTrack { get; init; }
	}
}
