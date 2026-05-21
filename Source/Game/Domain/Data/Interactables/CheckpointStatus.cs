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

namespace Nomad.Game.Domain.Data.Interactables
{
	/// <summary>
	/// A checkpoint's live status according to how the player has interacted with it thus far.
	/// </summary>
	public enum CheckpointStatus : byte
	{
		/// <summary>
		/// The checkpoint has never been activated before, currently undiscovered.
		/// </summary>
		Inactive,

		/// <summary>
		/// The checkpoint has been activated but it is not the one the player is currently
		/// using.
		/// </summary>
		Activated,

		/// <summary>
		/// This the player's current checkpoint that they are resting at.
		/// </summary>
		Current,

		Min = Inactive,
		Max = Current
	};
};
