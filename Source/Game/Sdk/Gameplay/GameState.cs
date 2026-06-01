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

namespace Nomad.Game.Sdk.Gameplay
{
    /// <summary>
    ///
    /// </summary>
    public enum GameState : byte
    {
        /// <summary>
        /// We are in a menu.
        /// </summary>
        Menu,

        /// <summary>
        /// We are in an active gameplay loop.
        /// </summary>
        Level,

        /// <summary>
        /// The game loop has been paused.
        /// </summary>
        Paused,

        /// <summary>
        /// We are currently in the loading screen caching assets or rehydrating an archived world state.
        /// </summary>
        Loading,

        /// <summary>
        ///
        /// </summary>
        Count
    }
}
