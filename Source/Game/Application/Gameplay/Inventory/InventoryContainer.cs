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
using Nomad.Game.Domain.Data.Inventory;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Interfaces.Items;

namespace Nomad.Game.Application.Gameplay.Inventory
{
	/*
	===================================================================================
	
	InventoryContainer
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class InventoryContainer
	{
		public IReadOnlyList<ItemStack> Stacks => _stacks;
		private readonly List<ItemStack> _stacks = new();

		public InventoryContainerType Type { get; }
		public InventoryRules Rules { get; }

		private readonly IItemCatalog _catalog;

		public float CurrentWeight {
			get {
				float weight = 0.0f;

				foreach ( var stack in _stacks ) {
					var def = _catalog.Get<ItemDefinition>( stack.ItemId );
					weight += def.Weight * stack.Amount;
				}

				return weight;
			}
		}

		/*
		===============
		InventoryContainer
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="catalog"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public InventoryContainer( InventoryContainerType type, InventoryRules rules, IItemCatalog catalog )
		{
			Type = type;
			Rules = rules ?? throw new ArgumentNullException( nameof( rules ) );
			_catalog = catalog ?? throw new ArgumentNullException( nameof( catalog ) );
		}

		public void AddStack( Guid itemId, int amount )
		{
			_stacks.Add( new ItemStack( itemId, amount ) );
		}

		public void RemoveStack( ItemStack stack )
		{
			_stacks.Remove( stack );
		}
	};
};
