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

namespace Nomad.Game.Sdk.Player.State
{
    /// <summary>
    ///
    /// </summary>
    [Flags]
    public enum PlayerFlags : uint
    {
        None = 0,

        /// <summary>
        /// Player is sliding.
        /// </summary>
        Sliding = 1 << 0,

        /// <summary>
        /// Player is in "stealth mode".
        /// </summary>
        Crouching = 1 << 1,

        /// <summary>
        /// Player is in bullet time.
        /// </summary>
        BulletTime = 1 << 2,

        /// <summary>
        /// Player is dashing.
        /// </summary>
        Dashing = 1 << 3,

        /// <summary>
        /// Mana was used this frame.
        /// </summary>
        UsedMana = 1 << 4,

        /// <summary>
        /// Player is currently in their idle animation.
        /// </summary>
        IdleAnimation = 1 << 6,

        /// <summary>
        ///
        /// </summary>
        Checkpoint = 1 << 7,

        /// <summary>
        /// The player's input is currently blocked.
        /// </summary>
        BlockedInput = 1 << 8,
    }
}
