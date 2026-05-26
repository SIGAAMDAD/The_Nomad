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
using System.Collections.Generic;
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Entities
{
	/*
	===================================================================================

	IEntityBase

	===================================================================================
	*/
	/// <summary>
	/// Base contract for any live runtime entity in the game.
	///
	/// This is intentionally minimal. It is the common denominator for players,
	/// AI, NPCs, items, consumables, world objects, projectiles, doors, containers,
	/// checkpoints, and generic interactables.
	///
	/// IEntityBase should not define health, movement, inventory, AI, animation,
	/// networking implementation, or item-specific behavior. Those belong to
	/// capability interfaces layered on top.
	/// </summary>

	public interface IEntityBase : IDisposable
	{
		/// <summary>
		/// Unique runtime id for this exact live entity.
		/// </summary>
		EntityId Id { get; }

		/// <summary>
		/// Immutable definition/archetype id, if this entity has one.
		///
		/// Examples:
		/// ITEM_WEAPON_HARDBALLER
		/// CHECKPOINT_MELIORA_DURINTAVERN
		/// </summary>
		InternString DefinitionId { get; }

		/// <summary>
		/// Display name or localization key.
		/// </summary>
		InternString DisplayName { get; }

		/// <summary>
		/// Broad entity category.
		/// </summary>
		EntityType Type { get; }

		/// <summary>
		/// Current generic lifecycle state.
		/// </summary>
		EntityLifecycleState LifecycleState { get; }

		/// <summary>
		/// Generic entity flags.
		/// </summary>
		EntityFlags Flags { get; }

		/// <summary>
		/// Optional tags for queries, filtering, interaction routing, and debug tooling.
		/// </summary>
		IReadOnlyCollection<InternString> Tags { get; }

		/// <summary>
		/// Monotonic revision for generic entity state changes.
		///
		/// Incremented when lifecycle, flags, display name, or tags change.
		/// More specialized systems may have their own revisions.
		/// </summary>
		uint Revision { get; }

		bool IsValid { get; }
		bool IsSpawned { get; }
		bool IsActive { get; }
		bool IsHidden { get; }
		bool IsDestroyed { get; }
		bool IsDisposed { get; }

		void Spawn();
		void Activate();
		void Deactivate();
		void Hide();
		void Show();
		void Despawn();
		void Destroy();

		bool HasFlags( EntityFlags flags );
		void AddFlags( EntityFlags flags );
		void RemoveFlags( EntityFlags flags );
		void SetFlags( EntityFlags flags );
	}
}
