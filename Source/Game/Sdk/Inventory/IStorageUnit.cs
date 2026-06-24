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
using Nomad.Core.Util;
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Sdk.Inventory
{
    /// <summary>
    ///
    /// </summary>
    public interface IStorageUnit : IDisposable
    {
        InternString StorageId { get; }
        InternString DisplayName { get; }

        InventoryContainerType ContainerType { get; }
        InventoryRules Rules { get; }
        float CurrentWeight { get; }

        IReadOnlyList<ItemStack> Stacks { get; }

        bool TryAdd(ItemDefinitionId itemType, int amount);
        bool TryRemove(ItemDefinitionId itemType, int amount, out int removed);

        bool TryAddInstance(ItemInstanceId instance);
        bool TryRemoveInstance(ItemInstanceId instance);
        bool ContainsInstance(ItemInstanceId instance);

        bool MoveStacksTo(IStorageUnit storageUnit);
        int GetStackAmount(ItemDefinitionId itemType);
        bool ContainsStack(ItemDefinitionId itemType);
    }
}
