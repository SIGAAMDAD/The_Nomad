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

namespace Nomad.Game.Domain.Data.Player {
	/// <summary>
	/// Player statistic identifiers.
	/// </summary>
	public enum StatType : byte {
		/// <summary>
		/// The player's current health.
		/// </summary>
		Health,

		/// <summary>
		/// The player's current mana.
		/// </summary>
		Rage,

		/// <summary>
		/// The current heat value of the player's dashkit.
		/// </summary>
		JumpKitHeat,

		/// <summary>
		/// The player's current sanity score, this does stat
		/// is not accounted for in Standard Mode.
		/// </summary>
		Sanity,

		/// <summary>
		/// 
		/// </summary>
		Count,

		Min = Health,
		Max = Sanity
	};
};