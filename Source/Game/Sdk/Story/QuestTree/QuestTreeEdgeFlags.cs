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

namespace Nomad.Game.Sdk.Story.QuestTree
{
    [Flags]
    public enum QuestTreeEdgeFlags : byte
    {
        None = 0,
        Optional = 1 << 0,
        Hidden = 1 << 1,
        NonBlocking = 1 << 2,
        Exclusive = 1 << 3
    }
}
