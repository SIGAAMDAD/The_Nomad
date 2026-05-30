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
using Nomad.Game.Sdk.Combat;
using Nomad.Game.Sdk.Events.Combat;
using Nomad.Game.Sdk.Inventory;

namespace Nomad.Game.Sdk.Items
{
    /// <summary>
    ///
    /// </summary>
    public interface IFirearmInstance : IWeaponInstance
    {
        FirearmMagazine AmmoSnapshot { get; }

        FirearmResolvedStats Stats { get; }
        FirearmDefinition FirearmDefinition { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Combat")]
        [EventPayload("FirearmId", typeof(ItemInstanceId), Order = 1)]
        IGameEvent<FirearmJammedEventArgs> FirearmJammed { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Combat")]
        [EventPayload("FirearmId", typeof(ItemInstanceId), Order = 1)]
        [EventPayload("AmmoCount", typeof(int), Order = 2)]
        IGameEvent<FirearmReloadedEventArgs> FirearmReloaded { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Combat")]
        [EventPayload("FirearmId", typeof(ItemInstanceId), Order = 1)]
        IGameEvent<FirearmUsedEventArgs> FirearmUsed { get; }

        bool TryReload(IStorageUnit inventory);

        bool TryStartUse();
        bool TryEndUse();

        bool TryAddMod(FirearmModSlot slot, FirearmModDefinition mod);
        bool TryRemoveMod(FirearmModSlot slot);
    }
}
