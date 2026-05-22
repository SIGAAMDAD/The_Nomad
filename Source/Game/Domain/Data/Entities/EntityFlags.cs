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

namespace Nomad.Game.Domain.Data.Entities
{
	/*
	===================================================================================

	EntityFlags

	===================================================================================
	*/
	/// <summary>
	/// Generic runtime traits shared by all entities.
	///
	/// These are not replacement flags for item/firearm/player-specific flags.
	/// They are only broad entity-level traits.
	/// </summary>

	[Flags]
	public enum EntityFlags : ulong
	{
		None = 0,

		/// <summary>
		/// Entity can be targeted by generic targeting/query systems.
		/// </summary>
		Targetable = 1UL << 0,

		/// <summary>
		/// Entity can be interacted with by a player, NPC, script, or gameplay system.
		/// </summary>
		Interactable = 1UL << 1,

		/// <summary>
		/// Entity should appear in save data.
		/// </summary>
		Persistent = 1UL << 2,

		/// <summary>
		/// Entity exists only for the current active runtime/session.
		/// </summary>
		Transient = 1UL << 3,

		/// <summary>
		/// Entity participates in multiplayer/network replication.
		/// </summary>
		Networked = 1UL << 4,

		/// <summary>
		/// Entity is owned by a player, container, NPC, or other entity.
		/// </summary>
		Owned = 1UL << 5,

		/// <summary>
		/// Entity can be picked up into an inventory.
		/// </summary>
		Pickupable = 1UL << 6,

		/// <summary>
		/// Entity blocks movement or line tests.
		/// </summary>
		Solid = 1UL << 7,

		/// <summary>
		/// Entity blocks visibility or line-of-sight checks.
		/// </summary>
		Occluder = 1UL << 8,

		/// <summary>
		/// Entity should be ignored by normal interaction scans.
		/// </summary>
		InteractionDisabled = 1UL << 9,

		/// <summary>
		/// Entity is hidden from normal gameplay but may still exist in memory.
		/// </summary>
		Hidden = 1UL << 10,

		/// <summary>
		/// Entity is currently locked by a script, cutscene, menu, or transaction.
		/// </summary>
		Locked = 1UL << 11,

		/// <summary>
		/// Entity should be considered destroyed and unavailable for normal gameplay.
		/// </summary>
		Destroyed = 1UL << 12
	};
};
