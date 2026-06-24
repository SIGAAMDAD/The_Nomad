/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Runtime.CompilerServices;

namespace Nomad.Game.Sdk.Player.JumpKit
{
    public readonly struct JumpKitStatus : IEquatable<JumpKitStatus>
    {
        public readonly float BurnoutAmount;
        public readonly float BurnoutCooldownElapsed;
        public readonly float CooldownSeconds;
        public readonly float CurrentDashDuration;
        public readonly float RemainingDashTime;
        public readonly PlayerJumpKitFlags Flags;

        public bool IsDashing => HasAny( PlayerJumpKitFlags.DashInProgress );
        public bool IsBurnedOut => HasAny( PlayerJumpKitFlags.BurnedOut );
        public bool IsCooling => HasAny( PlayerJumpKitFlags.Cooling );
        public bool CanDash => HasAny( PlayerJumpKitFlags.CanDash );

        public JumpKitStatus(
            float burnoutAmount,
            float burnoutCooldownElapsed,
            float cooldownSeconds,
            float currentDashDuration,
            float remainingDashTime,
            PlayerJumpKitFlags flags
        )
        {
            BurnoutAmount = burnoutAmount;
            BurnoutCooldownElapsed = burnoutCooldownElapsed;
            CooldownSeconds = cooldownSeconds;
            CurrentDashDuration = currentDashDuration;
            RemainingDashTime = remainingDashTime;
            Flags = flags;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool HasAny( PlayerJumpKitFlags flags )
        {
            return (Flags & flags) != 0;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool HasAll( PlayerJumpKitFlags flags )
        {
            return (Flags & flags) == flags;
        }

        public bool Equals( JumpKitStatus other )
        {
            return BurnoutAmount.Equals( other.BurnoutAmount )
                && BurnoutCooldownElapsed.Equals( other.BurnoutCooldownElapsed )
                && CooldownSeconds.Equals( other.CooldownSeconds )
                && CurrentDashDuration.Equals( other.CurrentDashDuration )
                && RemainingDashTime.Equals( other.RemainingDashTime )
                && Flags == other.Flags;
        }

        public override bool Equals( object? obj )
        {
            return obj is JumpKitStatus other && Equals( other );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                BurnoutAmount,
                BurnoutCooldownElapsed,
                CooldownSeconds,
                CurrentDashDuration,
                RemainingDashTime,
                Flags
            );
        }
    }
}
