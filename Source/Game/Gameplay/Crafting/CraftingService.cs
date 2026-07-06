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
using Nomad.Game.Gameplay.Items;
using Nomad.Game.Content.Catalogs.Items;
using Nomad.Game.Sdk.Crafting;
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Gameplay.Crafting
{
	internal sealed class CraftingService : ICraftingService
	{
		private readonly Dictionary<ItemDefinitionId, ItemRecipeDefinitionId[]> _itemToRecipe = new();
		private readonly CraftingRecipeCatalog _catalog;

		public CraftingService( CraftingRecipeCatalog catalog )
		{
			_catalog = catalog ?? throw new ArgumentNullException( nameof( catalog ) );
		}

		public bool CanCraft( IStorageUnit storageUnit, ItemDefinitionId requestedItemId )
		{
			return false;
		}

		public IReadOnlyList<ItemRecipeDefinitionId> GetAvailableRecipes( ItemDefinitionId itemId )
		{
			throw new NotImplementedException();
		}

		public bool IsRecipeAvailable( ItemDefinitionId itemId )
		{
			throw new NotImplementedException();
		}

		public bool RecipeExists( ItemDefinitionId itemId )
		{
			return true;
		}

		public CraftingResult TryCraft( IStorageUnit itemCache, ItemDefinitionId itemId )
		{

/*
			Span<ItemDefinitionId> ingredients = stackalloc ItemDefinitionId[4];

			switch ( recipe.Type ) {
				case RecipeType.Duo:
					if ( recipe is DuoRecipe duo ) {
						ingredients[0] = duo.FirstItemId;
						ingredients[1] = duo.SecondItemId;
						ingredients = ingredients.Slice( 0, 2 );
					}
					break;

				case RecipeType.Trio:
					if ( recipe is TrioRecipe trio ) {
						ingredients[0] = trio.FirstItemId;
						ingredients[1] = trio.SecondItemId;
						ingredients[2] = trio.ThirdItemId;
						ingredients = ingredients.Slice( 0, 3 );
					}
					break;

				case RecipeType.Quad:
					if ( recipe is QuadRecipe quad ) {
						ingredients[0] = quad.FirstItemId;
						ingredients[1] = quad.SecondItemId;
						ingredients[2] = quad.ThirdItemId;
						ingredients[3] = quad.FourthItemId;
					}
					break;

				default:
					throw new InvalidOperationException(
						$"Invalid recipe type {recipe.Type}"
					);
			}

			if ( !ContainerHasItems( itemCache, ingredients ) ) {
				return CraftingResult.Failed(
					failureReason: CraftFailureReason.NotEnoughMaterials
				);
			}
*/

			return CraftingResult.Crafted(
				success: true
			);
		}

		public CraftingResult TryCraft( IStorageUnit storageUnit, ItemDefinitionId requestedItemId, ItemRecipeDefinitionId recipeId )
		{
			throw new NotImplementedException();
		}

		private bool ContainerHasItems( IStorageUnit storageUnit, ReadOnlySpan<ItemDefinitionId> ingredients )
		{
			for ( int i = 0; i < ingredients.Length; i++ ) {
				if ( !storageUnit.ContainsStack( ingredients[i] ) ) {
					return false;
				}
			}

			return true;
		}
	};
};
