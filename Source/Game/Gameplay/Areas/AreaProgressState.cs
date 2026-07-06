/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

namespace Nomad.Game.Gameplay.Areas
{
    internal struct AreaProgressState
    {
        public ushort LandmarksCompleted;
        public ushort ActivitiesCompleted;
        public ushort CollectiblesFound;
        public ushort FastTravelPointsUnlocked;

        public ushort CompletedTotal => (ushort)(LandmarksCompleted + ActivitiesCompleted + CollectiblesFound + FastTravelPointsUnlocked);
    }
}
