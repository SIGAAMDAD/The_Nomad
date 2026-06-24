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

namespace Nomad.Game.Sdk.Player.JumpKit
{
    /// <summary>
    /// Bitfield for persistent jump-kit state and one-shot transition signals.
    /// Persistent state bits may be observed through <see cref="JumpKitStatus" />;
    /// transition bits are only valid for the event payload that carries them.
    /// </summary>
    [Flags]
    public enum PlayerJumpKitFlags : ushort
    {
        None = 0,

        DashInProgress = 1 << 0,
        BurnedOut = 1 << 1,
        Cooling = 1 << 2,
        CanDash = 1 << 3,
        BurnoutPending = 1 << 4,

        DashStarted = 1 << 5,
        DashEnded = 1 << 6,
        DashRejected = 1 << 7,
        BurnoutEntered = 1 << 8,
        BurnoutChanged = 1 << 9,
        CoolingStarted = 1 << 10,
        Recharged = 1 << 11,
        ModuleChanged = 1 << 12,

        // Compatibility aliases for older call sites.
        IsDashing = DashInProgress,
        IsBurnedOut = BurnedOut,
        BurnedOutThisFrame = BurnoutEntered,
        RechargedThisFrame = Recharged
    }
}
