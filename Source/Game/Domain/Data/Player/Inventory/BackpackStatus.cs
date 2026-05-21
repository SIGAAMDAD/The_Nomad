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

namespace Nomad.Game.Domain.Data.Player.Inventory
{
	/// <summary>
	/// The status of the player's backpack.
	/// </summary>
	public enum BackpackStatus : byte
	{
		/// <summary>
		/// The backpack is currently equipped by the player.
		/// </summary>
		Equipped,

		/// <summary>
		/// The backpack has been unequipped and stored at a meliora guarunteeing its safety.
		/// </summary>
		UnequippedMeliora,

		/// <summary>
		/// The backpack has been unequipped and placed at a specific point in the world, but its fair game to anyone to steal from.
		/// </summary>
		UnequippedFloor,

		Min = Equipped,
		Max = UnequippedFloor
	};
};
