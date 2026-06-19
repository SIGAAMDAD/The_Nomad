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
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using System.Numerics;

namespace Nomad.Game.Sdk.Player.Movement
{
    public interface IPlayerCameraController
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player.Movement", PayloadName = "PlayerCameraStatusChangedEventArgs")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("Origin", typeof(Vector3), Order = 2)]
        [EventPayload("Forward", typeof(Vector3), Order = 3)]
        [EventPayload("Right", typeof(Vector3), Order = 4)]
        [EventPayload("Pitch", typeof(float), Order = 5)]
        [EventPayload("Yaw", typeof(float), Order = 6)]
        [EventPayload("Roll", typeof(float), Order = 7)]
        IGameEvent<PlayerCameraStatusChangedEventArgs> CameraStatusChanged { get; }
    }
}
