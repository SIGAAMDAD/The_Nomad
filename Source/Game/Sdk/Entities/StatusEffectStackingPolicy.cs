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

namespace Nomad.Game.Sdk.Entities
{
    public enum StatusEffectStackingPolicy
    {
        None = 0,

        /// <summary>
        /// If the effect already exists, ignore the new application.
        /// </summary>
        Ignore,

        /// <summary>
        /// Reset the duration, but do not increase stacks.
        /// </summary>
        RefreshDuration,

        /// <summary>
        /// Add stack count up to MaxStacks.
        /// </summary>
        AddStack,

        /// <summary>
        /// Add stack and refresh duration.
        /// </summary>
        AddStackAndRefresh,

        /// <summary>
        /// Replace old instance entirely.
        /// </summary>
        Replace,

        /// <summary>
        /// Keep the instance with the strongest magnitude
        /// </summary>
        StrongestOnly
    }
}
