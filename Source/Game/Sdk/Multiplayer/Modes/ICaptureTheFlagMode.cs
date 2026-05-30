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
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Multiplayer.Modes;
using Nomad.Game.Sdk.Multiplayer.Objectives;
using Nomad.Game.Sdk.Multiplayer.Team;
using Nomad.Game.Sdk.Events.Multiplayer;

namespace Nomad.Game.Sdk.Multiplayer.Modes
{
    /// <summary>
    ///
    /// </summary>
    public interface ICaptureTheFlagMode : IDisposable
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("PeerId", typeof(PeerId), Order = 1)]
        [EventPayload("PreviousStatus", typeof(FlagStatus), Order = 2)]
        [EventPayload("CurrentStatus", typeof(FlagStatus), Order = 3)]
        IGameEvent<FlagStatusChangedEventArgs> FlagStatusChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        IGameEvent<CTFRoundBeginEventArgs> CTFRoundBegin { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        IGameEvent<CTFRoundEndEventArgs> CTFRoundEnd { get; }

        CaptureTheFlagSnapshot Snapshot { get; }

        bool TryBeginRound();
        bool TryEndRound(CaptureTheFlagRoundEndReason reason);

        bool TryGetTeamScore(TeamId teamId, out int score);
    }
}
