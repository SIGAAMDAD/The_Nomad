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
using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Crafting;
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Infrastructure.Gameplay.Items
{
	internal sealed class CraftingRecipeCatalog : ICraftingRecipeCatalog
	{
		private readonly Dictionary<ItemDefinitionId, ItemRecipe> _recipes = new();

		public bool TryLoadRecipe( ItemDefinitionId itemId, JsonElement recipe )
		{
			string[] ingredients = recipe.GetOptionalArray<string>( "Recipe" );
			if ( ingredients == null ) {
				return false;
			}

			_recipes[itemId] = ingredients.Length switch {
				2 => new DuoRecipe {
					FirstItemId = new ItemDefinitionId( new InternString( ingredients[0] ) ),
					SecondItemId = new ItemDefinitionId( new InternString( ingredients[1] ) )
				},
				3 => new TrioRecipe {
					FirstItemId = new ItemDefinitionId( new InternString( ingredients[0] ) ),
					SecondItemId = new ItemDefinitionId( new InternString( ingredients[1] ) ),
					ThirdItemId = new ItemDefinitionId( new InternString( ingredients[2] ) )
				},
				4 => new QuadRecipe {
					FirstItemId = new ItemDefinitionId( new InternString( ingredients[0] ) ),
					SecondItemId = new ItemDefinitionId( new InternString( ingredients[1] ) ),
					ThirdItemId = new ItemDefinitionId( new InternString( ingredients[2] ) ),
					FourthItemId = new ItemDefinitionId( new InternString( ingredients[3] ) )
				},
				_ => throw new InvalidOperationException( "" )
			};

			return true;
		}

		public bool RecipeExists( ItemDefinitionId itemId )
		{
			return true;
		}

		public bool TryGetRecipe( ItemDefinitionId itemId, out ItemRecipe recipe )
		{
			throw new System.NotImplementedException();
		}
	};
};
