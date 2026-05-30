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

using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Items;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk.Inventory;

namespace Nomad.Game.Sdk.Items
{
    /// <summary>
    ///
    /// </summary>
    public interface IItemInstance : IEntityBase
    {
        ItemInstanceId InstanceId { get; }
        ItemDefinition Definition { get; }

        IStorageUnit? StorageUnit { get; }

        ItemStatus State { get; }

        ItemType BaseType { get; }

        int StackCount { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Items", PayloadName = "ItemStatusChangedEventArgs")]
        [EventPayload("OldStatus", typeof(ItemStatus), Order = 1)]
        [EventPayload("NewStatus", typeof(ItemStatus), Order = 2)]
        IGameEvent<ItemStatusChangedEventArgs> StatusChanged { get; }
    }
}
