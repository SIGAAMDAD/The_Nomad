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
using Nomad.Core.Events;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Player;

namespace Nomad.Game.Sdk.Player.State
{
    /// <summary>
    ///
    /// </summary>
    public interface IPlayerFlagService : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        IReadOnlyList<string> CurrentFlags { get; }

        /// <summary>
        ///
        /// </summary>
        PlayerFlags Bits { get; }

        /// <summary>
        ///
        /// </summary>
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player", PayloadName = "PlayerFlagsChangedEventArgs")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("OldFlags", typeof(PlayerFlags), Order = 2)]
        [EventPayload("NewFlags", typeof(PlayerFlags), Order = 3)]
        IGameEvent<PlayerFlagsChangedEventArgs> FlagsChanged { get; }

        /// <summary>
        ///
        /// </summary>
        void ClearFlags();

        /// <summary>
        ///
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        bool GetFlags(PlayerFlags flags);

        /// <summary>
        ///
        /// </summary>
        /// <param name="flags"></param>
        void AddFlags(PlayerFlags flags);

        /// <summary>
        ///
        /// </summary>
        /// <param name="flags"></param>
        void RemoveFlags(PlayerFlags flags);

        /// <summary>
        ///
        /// </summary>
        /// <param name="flagName"></param>
        /// <param name="state"></param>
        void SetFlag(string flagName, bool state);

        /// <summary>
        ///
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="clearFlags"></param>
        void ApplyFlags(IReadOnlyList<string> flags, bool clearFlags = false);
    }
}
