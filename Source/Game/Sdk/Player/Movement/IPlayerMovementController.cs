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

using System.Numerics;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Sdk.Player.Movement
{
    /// <summary>
    ///
    /// </summary>
    public interface IPlayerMovementController
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player.Movement", PayloadName = "PlayerLocomotionCueEventArgs")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("Cue", typeof(PlayerLocomotionCue), Order = 2)]
        [EventPayload("OldVelocity", typeof(Vector2), Order = 3)]
        [EventPayload("NewVelocity", typeof(Vector2), Order = 4)]
        [EventPayload("IsMoving", typeof(bool), Order = 5)]
        [EventPayload("MoveInput", typeof(Vector2), Order = 6)]
        [EventPayload("Origin", typeof(Vector3), Order = 7)]
        [EventPayload("ServerTick", typeof(uint), Order = 8)]
        IGameEvent<PlayerLocomotionCueEventArgs> LocomotionCue { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Player.Movement", PayloadName = "PlayerDirectionalLocomotionEventArgs")]
        [EventPayload("PlayerId", typeof(PlayerId), Order = 1)]
        [EventPayload("Direction", typeof(PlayerLocomotionDirection), Order = 2)]
        [EventPayload("MoveDirection", typeof(Vector2), Order = 3)]
        [EventPayload("FacingDirection", typeof(Vector2), Order = 4)]
        [EventPayload("ForwardAmount", typeof(float), Order = 5)]
        [EventPayload("RightAmount", typeof(float), Order = 6)]
        [EventPayload("IsMoving", typeof(bool), Order = 7)]
        [EventPayload("ServerTick", typeof(uint), Order = 8)]
        IGameEvent<PlayerDirectionalLocomotionEventArgs> DirectionalLocomotion { get; }
    }
}
