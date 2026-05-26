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

using Nomad.Core.Events;
using Nomad.Game.Sdk.Multiplayer.Modes;
using Nomad.Game.Sdk.Multiplayer.Team;
using Nomad.Game.Sdk.Events.Multiplayer;
using Nomad.Game.Sdk.Multiplayer.Objectives;

namespace Nomad.Game.Sdk.Multiplayer.Modes
{
    public interface IKingOfTheHillMode : IGameMode
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("TeamId", typeof(TeamId), Order = 1)]
        [EventPayload("PreviousStatus", typeof(HillStatus), Order = 2)]
        [EventPayload("CurrentStatus", typeof(HillStatus), Order = 3)]
        IGameEvent<HillStatusChangedEventArgs> HillStatusChanged { get; }

        KingOfTheHillInstanceData Snapshot { get; }
    }
}
