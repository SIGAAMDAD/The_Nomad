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

namespace Nomad.Game.Sdk.Player.JumpKit
{
    public readonly struct DashModuleContext
    {
        public readonly float Delta;
        public readonly Vector3 Velocity;
        public readonly Vector3 WishDirection;
        public readonly Vector3 Forward;
        public readonly Vector3 Up;
        public readonly bool IsGrounded;
        public readonly bool IsAirborne;
        public readonly bool IsWallRunning;
        public readonly bool IsAiming;
        public readonly bool IsStealthed;

        public DashModuleContext(
            float delta,
            Vector3 velocity,
            Vector3 wishDirection,
            Vector3 forward,
            Vector3 up,
            bool isGrounded,
            bool isAirborne,
            bool isWallRunning,
            bool isAiming,
            bool isStealthed
        )
        {
            Delta = delta;
            Velocity = velocity;
            WishDirection = wishDirection;
            Forward = forward;
            Up = up;
            IsGrounded = isGrounded;
            IsAirborne = isAirborne;
            IsWallRunning = isWallRunning;
            IsAiming = isAiming;
            IsStealthed = isStealthed;
        }
    }
}
