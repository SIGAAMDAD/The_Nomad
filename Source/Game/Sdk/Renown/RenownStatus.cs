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
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Renown
{
	/*
	===================================================================================

	RenownStatus
	
	===================================================================================
	*/
	/// <summary>
	/// The player's renown status for a specific region.
	/// </summary>

	public sealed class RenownStatus
	{
		public InternString Region { get; set; }

		/// <summary>
		/// Current known-ness in the region.
		/// </summary>
		public float CurrentRenown { get; set; }

		/// <summary>
		/// Highest renown ever reached. Useful for memory, rumors, achievements, and "old reputation".
		/// </summary>
		public float PeakRenown { get; set; }

		/// <summary>
		/// Derived from <see cref="CurrentRenown"/>.
		/// </summary>
		public RenownTierDefinition CurrentTier { get; set; }

		/// <summary>
		/// Important because some people should remember "you used to be a legend" even if your current renwon has faded.
		/// </summary>
		public RenownTierDefinition HighestTierReached { get; set; }

		/// <summary>
		/// Minimum decay value. Major historical events should not decay below a certain threshold.
		/// </summary>
		public float DecayFloor { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<InternString, float> HistoricalFloorSources { get; } = new();

		/// <summary>
		/// 
		/// </summary>
		public int LastPositiveChangeDay { get; set; }
		public int LastAnyChangeDay { get; set; }
	}
}
