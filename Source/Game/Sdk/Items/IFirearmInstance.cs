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
    /// Runtime state and behavior for a firearm item instance.
    /// </summary>
    public interface IFirearmInstance : IWeaponInstance
    {
        /// <summary>
        /// Current magazine capacity snapshot. This intentionally contains
        /// magazine state only; ballistic stats come from <see cref="LoadedAmmo"/>.
        /// </summary>
        FirearmMagazine AmmoSnapshot { get; }

        /// <summary>
        /// Resolved firearm handling and mechanical stats after installed mods.
        /// Ammo damage, range, velocity, and recoil are not copied into this value.
        /// </summary>
        FirearmResolvedStats Stats { get; }

        /// <summary>
        /// Catalog definition for this firearm instance.
        /// </summary>
        FirearmDefinition FirearmDefinition { get; }

        /// <summary>
        /// Ammunition currently selected for reload/use, or null when no ammo
        /// has been selected. The ammo's granular <see cref="AmmoDefinition.Type"/>
        /// drives chamber compatibility; its <see cref="AmmoDefinition.Category"/>
        /// drives broad balance/behavior buckets.
        /// </summary>
        AmmoDefinition? LoadedAmmo { get; }

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

        /// <summary>
        /// Selects the ammunition definition this firearm should load next.
        /// </summary>
        /// <param name="ammoDefinition">Ammunition to chamber.</param>
        /// <returns>
        /// True when the ammo type and ammo modifier are both compatible with
        /// this firearm.
        /// </returns>
        bool TrySetAmmo(AmmoDefinition ammoDefinition);

        /// <summary>
        /// Refills the firearm from the selected ammunition stack in inventory.
        /// </summary>
        /// <param name="inventory">Inventory containing the selected ammo stack.</param>
        /// <returns>True when at least one round was loaded.</returns>
        bool TryReload(IStorageUnit inventory);

        /// <summary>
        /// Attempts to begin firearm use and consume one round.
        /// </summary>
        bool TryStartUse();

        /// <summary>
        /// Ends an active firearm-use interaction.
        /// </summary>
        bool TryEndUse();

        /// <summary>
        /// Installs a firearm modification into the requested slot.
        /// </summary>
        bool TryAddMod(FirearmModSlot slot, FirearmModDefinition mod);

        /// <summary>
        /// Removes a firearm modification from the requested slot.
        /// </summary>
        bool TryRemoveMod(FirearmModSlot slot);
    }
}
