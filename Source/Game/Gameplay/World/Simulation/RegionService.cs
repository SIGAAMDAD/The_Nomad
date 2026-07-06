/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using Nomad.Core.Events;
using Nomad.Game.Gameplay.Runtime.Areas;
using Nomad.Game.Gameplay.Areas;

namespace Nomad.Game.Gameplay.World.Simulation
{
    /// <summary>
    /// Compatibility wrapper. Gameplay place mutation now lives in AreaService.
    /// </summary>
    [Obsolete("Use Nomad.Game.Gameplay.Areas.AreaService.")]
    internal sealed class RegionService : AreaService
    {
        public RegionService(
            IGameEventRegistryService eventFactory,
            CompiledAreaDatabase? areaDatabase = null)
            : base(eventFactory, areaDatabase)
        {
        }
    }
}
