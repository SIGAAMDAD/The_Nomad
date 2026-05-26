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
using System.Linq;
using System.Text.Json;
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Items
{
    public sealed record FirearmDefinition : WeaponDefinition
    {
        public override ItemType BaseType => ItemType.FirearmWeapon;
        public override WeaponType WeaponType => WeaponType.Firearm;
        public FirearmFlags Flags { get; init; }

        public float FireRate { get; init; }
        public int MagazineSize { get; init; }

        public float BaseAimDownSightsSpeed { get; init; } = 1.0f;
        public float BaseHipFireAccuracy { get; init; } = 1.0f;
        public float BaseReloadSpeed { get; init; } = 1.0f;
        public float BaseEquipSpeed { get; init; } = 1.0f;
        public float BaseRecoilRecovery { get; init; } = 1.0f;
        public float BaseJamChance { get; init; } = 0.0f;

        public static FirearmDefinition Load(JsonElement json)
        {
            var definition = new FirearmDefinition
            {
                BaseDurability = JsonLoader.GetRequired<float>(json, nameof(BaseDurability)),
                FireRate = JsonLoader.GetRequired<float>(json, nameof(FireRate)),
                MagazineSize = JsonLoader.GetRequired<int>(json, nameof(MagazineSize)),
                Flags = JsonLoader.GetRequiredArray<string>(json, nameof(Flags))
                    .Select(s => Enum.TryParse(s, out FirearmFlags flag) ? flag : default)
                    .Aggregate((prev, next) => prev | next)
            };
            definition.LoadBase(json);
            return definition;
        }
    }
}
