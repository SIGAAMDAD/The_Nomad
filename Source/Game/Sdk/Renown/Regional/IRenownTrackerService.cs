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

using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Renown;
using Nomad.Game.Sdk.Events.Renown.Region;
using Nomad.Game.Sdk.World;
using Nomad.Game.Sdk.Areas;

namespace Nomad.Game.Sdk.Renown.Regional
{
    /// <summary>
    ///
    /// </summary>
    public interface IRenownTrackerService
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Renown.Region")]
        [EventPayload("RegionId", typeof(AreaDefinitionId), Order = 1)]
        [EventPayload("OldValue", typeof(float), Order = 2)]
        [EventPayload("NewValue", typeof(float), Order = 3)]
        IGameEvent<RenownScoreChangedEventArgs> RenownScoreChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Renown.Region")]
        [EventPayload("RegionId", typeof(AreaDefinitionId), Order = 1)]
        [EventPayload("OldTier", typeof(RenownTier), Order = 2)]
        [EventPayload("NewTier", typeof(RenownTier), Order = 3)]
        IGameEvent<RenownTierChangedEventArgs> RenownTierChanged { get; }

        RenownStatus GetStatus(AreaDefinitionId regionId);

        void ApplyDelta(RenownDelta delta);
        void ApplyDecay(AreaDefinitionId regionId, float amount, int worldDay);
        void SetHistoricalFloor(AreaDefinitionId regionId, InternString sourceId, float floorValue);

        RenownTier GetTier(AreaDefinitionId regionId);
        float GetRenownRatio(AreaDefinitionId regionId);
    }
}
