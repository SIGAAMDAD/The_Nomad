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

namespace Nomad.Game.Domain.Data.Renown {
	public record TraitDefinition {
		public string Id { get; init; }
		public string DisplayName { get; init; }
		public string Description { get; init; }
		public string ShortSummary { get; init; }
		
		public int OutstandingMargin { get; init; }
		public bool IsRegionBased { get; init; }
		public bool CanBleedToAdjacent { get; init; }
		public bool CanDecay { get; init; }

		/// <summary>
		/// How fast the trait score bleeds into other regions.
		/// </summary>
		public float SpreadRate { get; init; }

		/// <summary>
		/// The number of days until the trait score starts to decay.
		/// </summary>
		public float DecayDelayDays { get; init; }

		/// <summary>
		/// The memory fade rate of a trait when decay starts.
		/// </summary>
		public float DecayRatePerDay { get; init; }

		public int MinScore { get; init; }
		public int MaxScore { get; init; }

		public HashSet<string> ConflictingTraits { get; init; }
		public Dictionary<string, float> FactionBias { get; init; }
	};
};