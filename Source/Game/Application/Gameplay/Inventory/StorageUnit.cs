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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Inventory;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Interfaces.Items;
using Nomad.Game.Domain.Interfaces.Player.Inventory;
using Nomad.Save.Services;

namespace Nomad.Game.Application.Gameplay.Inventory
{
	/*
	===================================================================================

	StorageUnit

	===================================================================================
	*/
	/// <summary>
	/// Represents a in-game item storage unit. This can be the player's inventory, a loot drop,
	/// or even a shop's inventory.
	/// </summary>

	internal sealed class StorageUnit : IStorageUnit
	{
		public InternString Id => _definition.Id;
		public InternString DisplayName => _definition.DisplayName;
		public InventoryContainerType Type => _definition.Type;
		public InventoryRules Rules => _definition.Rules;

		private readonly StorageUnitDefinition _definition;
		private readonly IItemCatalog _catalog;
		private readonly Dictionary<ItemDefinitionId, int> _stacks = new();

		private readonly IDisposable _saveBegin;
		private readonly IDisposable _loadBegin;

		public float CurrentWeight {
			get {
				if ( _definition.Rules.IgnoreWeight ) {
					return 0.0f;
				}

				float weight = 0.0f;

				foreach ( var stack in _stacks ) {
					var item = _catalog.Get<ItemDefinition>( stack.Key );
					weight += item.Weight * stack.Value;
				}

				return weight;
			}
		}

		/*
		===============
		StorageUnit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="displayName"></param>
		/// <param name="rules"></param>
		/// <param name="type"></param>
		/// <param name="catalog"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public StorageUnit( StorageUnitDefinition definition, IItemCatalog catalog, IGameEventRegistryService eventFactory )
		{
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
			_catalog = catalog ?? throw new ArgumentNullException( nameof( catalog ) );

			_saveBegin = eventFactory
				.GetEvent<SaveBeginEventArgs>(
					SaveBeginEventArgs.Name,
					SaveBeginEventArgs.NameSpace
				)
				.Subscribe( OnSaveBegin );

			_loadBegin = eventFactory
				.GetEvent<LoadBeginEventArgs>(
					LoadBeginEventArgs.Name,
					LoadBeginEventArgs.NameSpace
				)
				.Subscribe( OnLoadBegin );
		}

		/*
		===============
		TryAdd
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemType"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool TryAdd( ItemDefinitionId itemType, int amount )
		{
			RangeGuard.ThrowIfNegativeOrZero( amount, nameof( amount ) );

			var item = _catalog.Get<ItemDefinition>( itemType );

			if ( !_definition.Rules.AcceptsItem( item ) ) {
				return false;
			}

			if ( !_definition.Rules.IgnoreWeight ) {
				float newWeight = CurrentWeight + item.Weight * amount;
				if ( newWeight > _definition.Rules.MaxWeight ) {
					return false;
				}
			}

			checked {
				_stacks[itemType] += amount;
			}

			return true;
		}

		/*
		===============
		TryRemove
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemType"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool TryRemove( ItemDefinitionId itemType, int amount )
		{
			if ( amount <= 0 ) {
				return false;
			}

			int stackSize = _stacks[itemType];
			if ( stackSize < amount ) {
				return false;
			}

			stackSize -= amount;
			_stacks[itemType] = stackSize;

			return true;
		}

		private void OnSaveBegin( in SaveBeginEventArgs args )
		{
		}

		private void OnLoadBegin( in LoadBeginEventArgs args )
		{
		}
	};
};
