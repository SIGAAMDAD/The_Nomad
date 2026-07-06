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

using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Inventory;
using Nomad.Core.Util;
using System.Collections.Generic;

namespace Nomad.Game.Sdk.Crafting
{
    /// <summary>
    ///
    /// </summary>
    public interface ICraftingService
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="storageUnit">The <see cref="IStorageUnit"/> to request resources from.</param>
        /// <param name="requestedItemId">The item we are attempting to craft.</param>
        /// <param name="recipeId">The specific recipe we want to use to craft the item.</param>
        /// <returns><c>true</c> if the requested item was crafted successfully</returns>
        [ResultObject("CraftingResult", Namespace = "Nomad.Game.Sdk.Crafting")]
        [ResultObjectPayload("Success", typeof(bool), order: 1)]
        [ResultObjectPayload("FailureReason", typeof(CraftFailureReason), order: 2)]
        [ResultObjectSuccess("Success", MethodName = "Crafted")]
        [ResultObjectFailure("FailureReason", MethodName = "Failed")]
        CraftingResult TryCraft(IStorageUnit storageUnit, ItemDefinitionId requestedItemId, ItemRecipeDefinitionId recipeId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="storageUnit"></param>
        /// <param name="requestedItemId"></param>
        /// <returns></returns>
        bool CanCraft(IStorageUnit storageUnit, ItemDefinitionId requestedItemId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        IReadOnlyList<ItemRecipeDefinitionId> GetAvailableRecipes(ItemDefinitionId itemId);

        /// <summary>
        /// Checks whether a recipe exists for the specified item. There can be multiple different recipes for the same item.
        /// </summary>
        /// <param name="itemId">The item id to check.</param>
        /// <returns><c>true</c> if a recipe for the item exists, <c>false</c> if not.</returns>
        bool RecipeExists(ItemDefinitionId itemId);

        /// <summary>
        /// Checks whether any sort of recipe is available for the given item id.
        /// </summary>
        /// <remarks>
        /// There are cases where a recipe will not exist for an item, such as a backpack, a note, etc.
        /// </remarks>
        /// <param name="itemId"></param>
        /// <returns></returns>
        bool IsRecipeAvailable(ItemDefinitionId itemId);
    }
}
