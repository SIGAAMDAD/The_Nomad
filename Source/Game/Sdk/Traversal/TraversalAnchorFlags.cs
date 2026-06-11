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

namespace Nomad.Game.Sdk.Traversal
{
    [Flags]
    public enum TraversalAnchorFlags : ushort
    {
        None            = 0,

        WallHold        = 1 << 0,
        Ledge           = 1 << 1,
        MantleTop       = 1 << 2,
        Vault           = 1 << 3,
        Beam            = 1 << 4,
        Pole            = 1 << 5,
        Corner          = 1 << 6,

        EntryAllowed    = 1 << 7,
        ExitAllowed     = 1 << 8
    }
}
