/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;

namespace Nomad.Game.Sdk.Areas
{
    [Flags]
    public enum AreaFlags : ushort
    {
        None = 0,
        Discoverable = 1 << 0,
        TrackCompletion = 1 << 1,
        HasFastTravel = 1 << 2,
        Interior = 1 << 3,
        Urban = 1 << 4,
        Wilderness = 1 << 5,
        Hostile = 1 << 6,
        Safe = 1 << 7
    }
}
