/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using Nomad.Game.Sdk.Areas;
using Nomad.Game.Sdk.Renown.Regional;

namespace Nomad.Game.Sdk.Story.Database
{
    /// <summary>
    /// Compatibility view over regional renown state. Story should prefer IRegionalRenownStateStore.
    /// </summary>
    [Obsolete("Use IRegionalRenownStateStore from Nomad.Game.Sdk.Renown.Regional.")]
    public interface IRenownStateStore : IRegionalRenownStateStore
    {
        bool TrySetRegionRenownScore(AreaDefinitionId areaId, float score)
        {
            return TrySetRegionalRenownScore(areaId, score);
        }

        bool TryGetRegionRenownScore(AreaDefinitionId areaId, out float score)
        {
            return TryGetRegionalRenownScore(areaId, out score);
        }
    }
}
