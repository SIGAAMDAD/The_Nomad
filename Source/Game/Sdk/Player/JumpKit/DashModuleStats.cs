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

namespace Nomad.Game.Sdk.Player.JumpKit
{
    public readonly struct DashModuleStats
    {
        public static DashModuleStats Default => new DashModuleStats(
            burnoutPerDash: 0.33f,
            burnoutOvercharge: 1.0f,
            dashSpeed: 1.0f,
            dashDistance: 1.0f,
            maxIFrameSeconds: 0.30f,
            coolRate: 1.0f,
            consecutiveDashPenalty: 1.0f,
            overheatLockoutSeconds: 1.0f
        );

        public readonly float BurnoutPerDash;
        public readonly float BurnoutOvercharge;
        public readonly float DashSpeed;
        public readonly float DashDistance;
        public readonly float MaxIFrameSeconds;
        public readonly float CoolRate;
        public readonly float ConsecutiveDashPenalty;
        public readonly float OverheatLockoutSeconds;

        public readonly float AirControlMultiplier;
        public readonly float WallrunBurnoutMultiplier;
        public readonly float DashNoiseMultiplier;
        public readonly float WeaponSpreadMultiplier;
        public readonly float RecoilMultiplier;

        public DashModuleStats(
            float burnoutPerDash,
            float burnoutOvercharge,
            float dashSpeed,
            float dashDistance,
            float maxIFrameSeconds,
            float coolRate,
            float consecutiveDashPenalty,
            float overheatLockoutSeconds,
            float airControlMultiplier = 1.0f,
            float wallrunBurnoutMultiplier = 1.0f,
            float dashNoiseMultiplier = 1.0f,
            float weaponSpreadMultiplier = 1.0f,
            float recoilMultiplier = 1.0f
        )
        {
            BurnoutPerDash = burnoutPerDash;
            BurnoutOvercharge = burnoutOvercharge;
            DashSpeed = dashSpeed;
            DashDistance = dashDistance;
            MaxIFrameSeconds = maxIFrameSeconds;
            CoolRate = coolRate;
            ConsecutiveDashPenalty = consecutiveDashPenalty;
            OverheatLockoutSeconds = overheatLockoutSeconds;
            AirControlMultiplier = airControlMultiplier;
            WallrunBurnoutMultiplier = wallrunBurnoutMultiplier;
            DashNoiseMultiplier = dashNoiseMultiplier;
            WeaponSpreadMultiplier = weaponSpreadMultiplier;
            RecoilMultiplier = recoilMultiplier;
        }
    }
}
