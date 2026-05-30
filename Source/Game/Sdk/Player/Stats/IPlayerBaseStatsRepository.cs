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
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Player;

namespace Nomad.Game.Sdk.Player.Stats
{
    /// <summary>
    /// The base abstraction contract for managing player related numbers.
    /// </summary>
    public interface IPlayerBaseStatsRepository
    {
        /// <summary>
        ///
        /// </summary>
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player", PayloadName = "PlayerBaseStatChangedEventArgs")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("OldValue", typeof(float), Order = 2)]
        [EventPayload("NewValue", typeof(float), Order = 3)]
        [EventPayload("StatId", typeof(BaseStatType), Order = 4)]
        IGameEvent<PlayerBaseStatChangedEventArgs> BaseStatChanged { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        void SetBaseStatValue(BaseStatType type, float value);

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        float GetBaseStatValue(BaseStatType type);
    }
}
