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
	/*
	===================================================================================
	
	EntityInteractionKind
	
	===================================================================================
	*/
	/// <summary>
	/// Broad category of interaction being attempted against an entity.
	///
	/// This should describe the player's/system's gameplay intent, not the exact
	/// implementation detail. Use EntityInteractionContext.Verb for more specific
	/// interaction ids such as "read_note", "drink", "unlock", "loot_all", etc.
	/// </summary>

	public enum EntityInteractionKind : byte
	{
		/// <summary>
		/// No interaction.
		/// </summary>
		None = 0,

		/// <summary>
		/// Generic use interaction.
		/// Suitable fallback for buttons, levers, devices, shrines, machinery, etc.
		/// </summary>
		Use,

		/// <summary>
		/// Inspect the entity without necessarily changing its state.
		/// Used for descriptions, examination text, investigation, or lore preview.
		/// </summary>
		Inspect,

		/// <summary>
		/// Pick the entity up into an inventory.
		/// Used for loose world items, ammo, weapons, consumables, valuables, etc.
		/// </summary>
		Pickup,

		/// <summary>
		/// Talk to the entity.
		/// Used for NPCs, merchants, prisoners, quest givers, companions, etc.
		/// </summary>
		Talk,

		/// <summary>
		/// Open the entity.
		/// Used for doors, containers, gates, books, panels, hatches, etc.
		/// </summary>
		Open,

		/// <summary>
		/// Close the entity.
		/// Used for doors, containers, panels, books, hatches, etc.
		/// </summary>
		Close,

		/// <summary>
		/// Toggle the entity between active/inactive or open/closed-style states.
		/// Useful when the exact direction does not matter to the caller.
		/// </summary>
		Toggle,

		/// <summary>
		/// Rest at the entity.
		/// Used for checkpoints, campfires, shrines, beds, safehouses, etc.
		/// </summary>
		Rest,

		/// <summary>
		/// Loot the entity.
		/// Used for containers, corpses, dropped packs, caches, crates, etc.
		/// </summary>
		Loot,

		/// <summary>
		/// Equip the entity.
		/// Used for weapons, armor, charms, relics, tools, or wearable items.
		/// </summary>
		Equip,

		/// <summary>
		/// Unequip the entity.
		/// Used for removing equipped weapons, armor, charms, relics, or tools.
		/// </summary>
		Unequip,

		/// <summary>
		/// Consume the entity.
		/// Used for food, medicine, drugs, drinks, ammo packs, ritual items, etc.
		/// </summary>
		Consume,

		/// <summary>
		/// Read the entity.
		/// Used for notes, books, signs, inscriptions, terminals, Valden's Book entries, etc.
		/// </summary>
		Read,

		/// <summary>
		/// Activate the entity.
		/// Used for enabling machines, traps, shrines, doors, checkpoints, devices, etc.
		/// </summary>
		Activate,

		/// <summary>
		/// Deactivate the entity.
		/// Used for disabling machines, traps, devices, hazards, alarms, etc.
		/// </summary>
		Deactivate,

		/// <summary>
		/// Unlock the entity.
		/// Used for locked doors, containers, restraints, gates, safes, etc.
		/// </summary>
		Unlock,

		/// <summary>
		/// Lock the entity.
		/// Used for doors, containers, gates, safes, and player-owned storage.
		/// </summary>
		Lock,

		/// <summary>
		/// Repair the entity.
		/// Used for weapons, machinery, vehicles, doors, armor, generators, etc.
		/// </summary>
		Repair,

		/// <summary>
		/// Reload the entity.
		/// Used for firearms, magazines, devices, turrets, machines, or tools.
		/// </summary>
		Reload,

		/// <summary>
		/// Harvest or gather from the entity.
		/// Used for plants, corpses, mineral nodes, monster parts, salvage, etc.
		/// </summary>
		Harvest,

		/// <summary>
		/// Trade with the entity.
		/// Used for vendors, merchants, faction boards, contraband dealers, etc.
		/// </summary>
		Trade,

		/// <summary>
		/// Accept something from the entity.
		/// Used for contracts, quests, rewards, faction board jobs, etc.
		/// </summary>
		Accept,

		/// <summary>
		/// Turn in something to the entity.
		/// Used for contracts, quest items, bounties, faction boards, etc.
		/// </summary>
		TurnIn,

		/// <summary>
		/// Custom/special interaction.
		/// The specific behavior should be identified through EntityInteractionContext.Verb.
		/// </summary>
		Custom
	};
};
