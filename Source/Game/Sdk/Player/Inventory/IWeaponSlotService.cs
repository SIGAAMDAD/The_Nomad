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
using Nomad.Core.Events;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Player;

namespace Nomad.Game.Sdk.Player.Inventory
{
    /// <summary>
    ///
    /// </summary>
    public interface IWeaponSlotService : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        WeaponSlotIndex Current { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("PreviousSlot", typeof(WeaponSlotIndex), Order = 2)]
        [EventPayload("CurrentSlot", typeof(WeaponSlotIndex), Order = 3)]
        IGameEvent<WeaponSlotChangedEventArgs> WeaponSlotChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("PreviousWeaponId", typeof(ItemInstanceId), Order = 2)]
        [EventPayload("CurrentWeaponId", typeof(ItemInstanceId), Order = 3)]
        IGameEvent<WeaponSlotContentsChangedEventArgs> WeaponSlotContentsChanged { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        bool IsOccupied(WeaponSlotIndex slot);

        /// <summary>
        ///
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        bool TryGetSlot(WeaponSlotIndex slot, out ItemInstanceId weapon);

        /// <summary>
        ///
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="weapon"></param>
        /// <returns></returns>
        bool TrySetSlot(WeaponSlotIndex slot, ItemInstanceId weapon);

        /// <summary>
        ///
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool TrySwapSlots(WeaponSlotIndex a, WeaponSlotIndex b);

        /// <summary>
        /// Attempts to clear all weapon slots.
        /// </summary>
        /// <returns></returns>
        bool TryClearSlots();
    }
}
