/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

namespace Nomad.Game.Sdk.Player
{
    /// <summary>
    /// Defines the tunable dash/jump-kit behavior exposed by an equipped module.
    /// </summary>
    public interface IDashModule
    {
        /// <summary>
        /// The name of this dash module/upgrade.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The upgrade's description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Heat value at or above which the kit enters burnout after the current dash.
        /// </summary>
        float BurnoutMax { get; }

        /// <summary>
        /// Heat added by a successful dash.
        /// </summary>
        float DashBurnoutIncrease { get; }

        /// <summary>
        /// Base dash duration before consecutive-use penalties are applied.
        /// </summary>
        float DashDuration { get; }

        /// <summary>
        /// Delay after a completed dash before passive heat cooling starts.
        /// </summary>
        float BurnoutCooldown { get; }

        /// <summary>
        /// Lockout duration after a full burnout. A value less than or equal to zero uses the kit default.
        /// </summary>
        float BurnoutResetDuration { get; }

        /// <summary>
        /// Optional module-provided dash velocity. Movement may use a derived-stat fallback instead.
        /// </summary>
        float DashVelocity { get; }

        /// <summary>
        /// Base invulnerability frame count available to gameplay systems that consume jump-kit status.
        /// </summary>
        int BaseIFrames { get; }

        /// <summary>
        /// Pitch scaling applied by audio feedback for chained dashes.
        /// </summary>
        float EnginePitchPerChain { get; }
    }
}
