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

namespace Nomad.Game.Sdk.Items
{
    public sealed record FirearmResolvedStats
    {
        public FirearmFlags Flags { get; init; }
        public float FireRate { get; init; }
        public int MagazineSize { get; init; }

        public float AimDownSightsSpeed { get; init; }
        public float AimDownSightsAccuracy { get; init; }
        public float HipFireAccuracy { get; init; }
        public float MovementAccuracy { get; init; }

        public float RecoilKick { get; init; }
        public float RecoilRecovery { get; init; }
        public float SpreadBloom { get; init; }
        public float SpreadRecovery { get; init; }

        public float ReloadSpeed { get; init; }
        public float EquipSpeed { get; init; }

        public float JamChance { get; init; }
        public float MisfireChance { get; init; }

        public float HeatPerShot { get; init; }
        public float HeatRecovery { get; init; }
        public float DirtAccumulation { get; init; }

        public float Loudness { get; init; }
        public float MuzzleFlash { get; init; }
        public float ShooterRevealStrength { get; init; }
        public float Weight { get; init; }
        public float Handling { get; init; }
    }
}
