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
using System.Text.Json;
using Nomad.Core.FileSystem;
using Nomad.Core.Logger;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Crafting;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Content.Catalogs;

namespace Nomad.Game.Content.Catalogs.Items
{
	/*
	===================================================================================

	CraftingRecipeCatalog

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class CraftingRecipeCatalog : DataDefinitionRegistry<ItemRecipeDefinitionId, ItemRecipe>, ICraftingRecipeCatalog
	{
		protected override Func<string, ItemRecipeDefinitionId> KeyFactory => k => new ItemRecipeDefinitionId( );
		protected override string LoggerCategoryName => nameof( CraftingRecipeCatalog );

		public CraftingRecipeCatalog( IFileSystem fileSystem, ILoggerService logger )
			: base( fileSystem, logger )
		{
		}

		public bool RecipeExists( ItemDefinitionId itemId )
		{
			return true;
		}

		public bool TryGetRecipe( ItemDefinitionId itemId, out ItemRecipe recipe )
		{
			throw new System.NotImplementedException();
		}

		protected override bool TryLoadDefinition( JsonElement json, out ItemRecipe definition )
		{
			string[] ingredients = json.GetRequiredArray<string>( "Ingredients" );

			definition = ingredients.Length switch {
				2 => LoadDuoRecipe( json, ingredients ),
				3 => LoadTrioRecipe( json, ingredients ),
				4 => LoadQuadRecipe( json, ingredients ),
				_ => throw new InvalidOperationException( $"Invalid recipe ingredient quantity '{ingredients.Length}', it must be 2, 3, or 4." )
			};

			return true;
		}

		/*
		===============
		LoadDuoRecipe
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="json"></param>
		/// <param name="ingredients"></param>
		/// <returns></returns>
		private ItemRecipe LoadDuoRecipe( JsonElement json, string[] ingredients )
		{
			DuoRecipe recipe = new DuoRecipe {
				FirstItemId = new ItemDefinitionId( new InternString( ingredients[0] ) ),
				SecondItemId = new ItemDefinitionId( new InternString( ingredients[1] ) )
			};
			return LoadRecipeBase( json, recipe );
		}

		/*
		===============
		LoadTrioRecipe
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="json"></param>
		/// <param name="ingredients"></param>
		/// <returns></returns>
		private ItemRecipe LoadTrioRecipe( JsonElement json, string[] ingredients )
		{
			TrioRecipe recipe = new TrioRecipe {
				FirstItemId = new ItemDefinitionId( new InternString( ingredients[0] ) ),
				SecondItemId = new ItemDefinitionId( new InternString( ingredients[1] ) ),
				ThirdItemId = new ItemDefinitionId( new InternString( ingredients[2] ) )
			};
			return LoadRecipeBase( json, recipe );
		}

		/*
		===============
		LoadQuadRecipe
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="json"></param>
		/// <param name="ingredients"></param>
		/// <returns></returns>
		private ItemRecipe LoadQuadRecipe( JsonElement json, string[] ingredients )
		{
			QuadRecipe recipe = new QuadRecipe {
				FirstItemId = new ItemDefinitionId( new InternString( ingredients[0] ) ),
				SecondItemId = new ItemDefinitionId( new InternString( ingredients[1] ) ),
				ThirdItemId = new ItemDefinitionId( new InternString( ingredients[2] ) ),
				FourthItemId = new ItemDefinitionId( new InternString( ingredients[3] ) )
			};
			return LoadRecipeBase( json, recipe );
		}

		/*
		===============
		LoadRecipeBase
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="json"></param>
		/// <param name="recipe"></param>
		/// <returns></returns>
		private ItemRecipe LoadRecipeBase( JsonElement json, ItemRecipe recipe )
		{
			return recipe with {
				Id = new ItemRecipeDefinitionId( new InternString( json.GetRequired<string>( nameof( ItemRecipe.Id ) ) ) ),
				OutputItemId = new ItemDefinitionId( new InternString( json.GetRequired<string>( nameof( ItemRecipe.OutputItemId ) ) ) )
			};
		}
	};
};
