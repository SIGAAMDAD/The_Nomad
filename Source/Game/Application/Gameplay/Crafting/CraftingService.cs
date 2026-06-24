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

using System.Collections.Generic;
using Nomad.Game.Sdk.Crafting;
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Application.Gameplay.Crafting
{
	internal sealed class CraftingService : ICraftingService
	{
		private readonly Dictionary<ItemDefinitionId, ItemRecipe> _recipes = new Dictionary<ItemDefinitionId, ItemRecipe>();

		public CraftingService()
		{
		}

		public bool RecipeExists( ItemDefinitionId itemId )
		{
			return _recipes.ContainsKey( itemId );
		}

		public CraftingResult TryCraft( IStorageUnit itemCache, ItemDefinitionId itemId, int amount )
		{
			if ( !_recipes.TryGetValue( itemId, out var recipe ) ) {
				return CraftingResult.Failed(
					amount: 0,
					failureReason: CraftFailureReason.NoSuchRecipe
				);
			}

			switch ( recipe.Type ) {
				case RecipeType.Duo:
				case RecipeType.Trio:
				case RecipeType.Quad:
					break;
			}

			return CraftingResult.Crafted(
				success: true,
				amount: 0
			);
		}
	};
};
