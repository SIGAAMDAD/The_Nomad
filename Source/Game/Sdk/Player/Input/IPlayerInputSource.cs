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
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;

namespace Nomad.Game.Sdk.Player.Input
{
    /*
	===================================================================================

	IPlayerInputSource

	===================================================================================
	*/
    /// <summary>
    /// Supplies normalized player input to gameplay code.
    ///
    /// Local input sources read keyboard/controller events.
    /// Remote input sources are fed by network RPCs.
    /// Player gameplay code should not care which one is used.
    /// </summary>

    public interface IPlayerInputSource : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        PlayerId PlayerId { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// The most recently produced input frame.
        /// </summary>
        PlayerInputFrame Current { get; }

        /// <summary>
        /// Gets the input frame for a game/network tick.
        /// Repeated calls with the same tick should return the same frame.
        /// </summary>
        PlayerInputFrame ReadFrame(uint tick);

        void Reset();
    }
}
