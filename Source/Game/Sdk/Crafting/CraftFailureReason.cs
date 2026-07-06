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

namespace Nomad.Game.Sdk.Crafting
{
    public enum CraftFailureReason : byte
    {
        /// <summary>
        /// No such recipe exists for the item requested.
        /// </summary>
        NoSuchRecipe,

        /// <summary>
        /// There is not enough materials within the storage unit given to the recipe crafter to make the requested item.
        /// </summary>
        NotEnoughMaterials,

        /// <summary>
        /// The recipe for said item hasn't been unlocked yet.
        /// </summary>
        NotUnlocked
    }
}
