/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

namespace Nomad.Game.Sdk.Areas
{
    /// <summary>
    /// Authoring-scale classification for named gameplay places.
    /// </summary>
    public enum AreaKind : byte
    {
        Unknown = 0,
        BiomeZone = 1,
        Region = 2,
        City = 3,
        District = 4,
        Village = 5,
        Settlement = 6,
        Landmark = 7,
        Dungeon = 8,
        Interior = 9,
        EncounterZone = 10
    }
}
