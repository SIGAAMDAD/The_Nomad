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
    public enum TraversalEdgeFlags : ushort
    {
        None                = 0,

        RequiresJump        = 1 << 0,
        RequiresClearance   = 1 << 1,
        RequiresLineOfSight = 1 << 2,

        EndsAttached        = 1 << 8,
        EndsGrounded        = 1 << 9,
        EndsFalling         = 1 << 10
    }
}
