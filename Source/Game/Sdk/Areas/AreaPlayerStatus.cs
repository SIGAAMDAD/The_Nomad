/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

namespace Nomad.Game.Sdk.Areas
{
    /// <summary>
    /// Mutable player relationship to an authored area.
    /// </summary>
    public enum AreaPlayerStatus : byte
    {
        Undiscovered = 0,
        Discovered = 1,
        Visited = 2,
        Completed = 3,
        Locked = 4
    }
}
