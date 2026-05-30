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
    /// Defines the different modes for bootstrapping a game world.
    /// This enum specifies how the world should be initialized, whether for single-player or multiplayer scenarios.
    /// </summary>
    public enum WorldBootstrapMode : byte
    {
        /// <summary>
        /// Starts a new single-player game, creating a fresh world instance.
        /// </summary>
        SinglePlayerNewGame = 0,
        /// <summary>
        /// Loads an existing single-player game from a saved state.
        /// </summary>
        SinglePlayerLoadGame,
        /// <summary>
        /// Hosts a multiplayer game session, initializing the world as the server.
        /// </summary>
        MultiplayerHost,
        /// <summary>
        /// Joins an existing multiplayer game session as a client.
        /// </summary>
        MultiplayerClient,

        /// <summary>
        /// Represents the total count of valid bootstrap modes. Used for iteration and validation.
        /// </summary>
        Count
    }
}
