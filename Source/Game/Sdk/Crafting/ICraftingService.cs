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

namespace Nomad.Game.Sdk.Crafting
{
    public interface ICraftingService
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="itemCache"></param>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [ResultObject("CraftingResult", Namespace = "Nomad.Game.Sdk.Crafting")]
        [ResultObjectPayload("Success", typeof(bool), order: 1)]
        [ResultObjectPayload("Amount", typeof(int), order: 2)]
        [ResultObjectPayload("FailureReason", typeof(CraftFailureReason), order: 3)]
        [ResultObjectSuccess("Success", "Amount", MethodName = "Crafted")]
        [ResultObjectFailure("Amount", "FailureReason", MethodName = "Failed")]
        CraftingResult TryCraft(IStorageUnit itemCache, ItemDefinitionId itemId, int amount);

        /// <summary>
        /// Checks whether a recipe exists for the specified item.
        /// </summary>
        /// <param name="itemId">The item id to check.</param>
        /// <returns><c>true</c> if a recipe for the item exists, <c>false</c> if not.</returns>
        bool RecipeExists(ItemDefinitionId itemId);
    }
}
