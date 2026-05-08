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
using System.Linq;
using Nomad.Game.Application.Gameplay.Inventory;
using Nomad.Game.Domain.Data.Inventory;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Interfaces.Items;

namespace Nomad.Game.Application.Gameplay.GameServices
{
	internal sealed class InventoryService
	{
		private readonly IItemCatalog _catalog;

		public InventoryService( IItemCatalog catalog )
		{
			_catalog = catalog ?? throw new ArgumentNullException( nameof( catalog ) );
		}

		public InventoryMoveResult MoveStack( InventoryContainer source, InventoryContainer destination, Guid itemId, int amount )
		{
			if ( amount <= 0 ) {
				return InventoryMoveResult.InvalidAmount;
			}

			var sourceStack = source.Stacks.FirstOrDefault( item => item.ItemId == itemId );
			if ( sourceStack == null || sourceStack.Amount < amount ) {
				return InventoryMoveResult.SourceMissing;
			}

			var definition = _catalog.Get<ItemDefinition>( itemId );
			if ( definition == null ) {
				return InventoryMoveResult.SourceMissing;
			}

			ItemStack? existingDestStack = null;
			if ( destination.Rules.AcceptsItem( definition ) ) {
				existingDestStack = destination.Stacks.FirstOrDefault( item => item.ItemId == itemId );
			}
			if ( !sourceStack.TryRemove( amount ) ) {
				return InventoryMoveResult.SourceMissing;
			}
			if ( sourceStack.IsEmpty ) {
				source.RemoveStack( sourceStack );
			}
			if ( existingDestStack != null ) {
				existingDestStack.Add( amount );
			} else {
				destination.AddStack( itemId, amount );
			}

			return InventoryMoveResult.Success;
		}
	};
};
