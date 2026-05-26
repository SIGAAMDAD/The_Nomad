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

namespace Nomad.Game.Sdk.Interactables
{
	/// <summary>
	/// The relation of the player to an interactable object.
	/// </summary>
	public enum PlayerInteractionStatus : byte
	{
		/// <summary>
		/// Player is not within processing range.
		/// </summary>
		None,

		/// <summary>
		/// In range to show the "Press [button] to interact" prompt.
		/// </summary>
		InRange,

		/// <summary>
		/// The player is currently interacting with the object.
		/// </summary>
		Interacting,

		/// <summary>
		/// The number of interaction statuses.
		/// </summary>
		Count
	}
}
