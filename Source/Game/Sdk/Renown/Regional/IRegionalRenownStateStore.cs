/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using Nomad.Game.Sdk.Areas;

namespace Nomad.Game.Sdk.Renown.Regional
{
    public interface IRegionalRenownStateStore
    {
        bool TrySetRegionalRenownScore(AreaDefinitionId areaId, float score);
        bool TryGetRegionalRenownScore(AreaDefinitionId areaId, out float score);
    }
}
