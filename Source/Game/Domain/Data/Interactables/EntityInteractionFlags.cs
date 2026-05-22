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

using System;

namespace Nomad.Game.Domain.Data.Interactables
{
	/*
	===================================================================================
	
	EntityInteractionFlags
	
	===================================================================================
	*/
	/// <summary>
	/// Additional modifiers for an interaction attempt.
	/// </summary>

	[Flags]
	public enum EntityInteractionFlags : ushort
	{
		None = 0,

		/// <summary>
		/// Interaction was requested by a local player input action.
		/// </summary>
		LocalInput = 1 << 0,

		/// <summary>
		/// Interaction was requested by AI, script, or simulation logic.
		/// </summary>
		Scripted = 1 << 1,

		/// <summary>
		/// Interaction was received from the network.
		/// </summary>
		Networked = 1 << 2,

		/// <summary>
		/// Interaction should skip normal range checks.
		/// Useful for scripted scenes, admin/debug actions, or special abilities.
		/// </summary>
		IgnoreRange = 1 << 3,

		/// <summary>
		/// Interaction should skip line-of-sight checks.
		/// </summary>
		IgnoreLineOfSight = 1 << 4,

		/// <summary>
		/// Interaction should not play presentation feedback.
		/// Useful for silent simulation/application.
		/// </summary>
		Silent = 1 << 5,

		/// <summary>
		/// Interaction should only validate whether it could happen.
		/// It should not mutate entity state.
		/// </summary>
		PreviewOnly = 1 << 6,

		/// <summary>
		/// Interaction should be treated as forced/authoritative.
		/// Useful for host corrections, loading, scripted locks, or rollback.
		/// </summary>
		Forced = 1 << 7,

		/// <summary>
		/// Interaction should consume or use the held/equipped item if applicable.
		/// </summary>
		ConsumesHeldItem = 1 << 8,

		/// <summary>
		/// Interaction came from a repeated/held input rather than a fresh press.
		/// </summary>
		Held = 1 << 9
	};
};
