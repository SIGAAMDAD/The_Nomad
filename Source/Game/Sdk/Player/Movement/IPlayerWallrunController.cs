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
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Events.Player.Movement;
using System.Numerics;

namespace Nomad.Game.Sdk.Player.Movement
{
    public interface IPlayerWallrunController
    {
        PlayerWallrunState State { get; }
        WallMountedMoveMode Mode { get; }
        bool IsActive { get; }
        Vector3 Velocity { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player.Movement", PayloadName = "PlayerWallrunStatusChangedEventArgs")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("OldState", typeof(PlayerWallrunState), Order = 2)]
        [EventPayload("NewState", typeof(PlayerWallrunState), Order = 3)]
        [EventPayload("Mode", typeof(WallMountedMoveMode), Order = 4)]
        [EventPayload("SurfaceNormal", typeof(Vector3), Order = 5)]
        IGameEvent<PlayerWallrunStatusChangedEventArgs> StatusChanged { get; }
    }
}
