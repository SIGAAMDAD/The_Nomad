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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Player.Inventory;

namespace Nomad.Game.Sdk.Items
{
    /// <summary>
    /// Defines the persistent, catalog-level data for a firearm.
    /// </summary>
    /// <remarks>
    /// Firearm definitions describe handling and mechanical behavior: fire rate,
    /// magazine size, reload speed, aiming behavior, reliability, and mod support.
    /// Ballistic output belongs to <see cref="AmmoDefinition"/>. A firearm can
    /// accept multiple ammo types, but it does not copy damage, range, velocity,
    /// or recoil from those ammo definitions into its own stats.
    /// </remarks>
    public sealed record FirearmDefinition : WeaponDefinition
    {
        public override ItemType BaseType => ItemType.FirearmWeapon;
        public override WeaponType WeaponType => WeaponType.Firearm;

        /// <summary>
        /// Mechanical traits, capabilities, and restrictions for this firearm.
        /// </summary>
        public FirearmFlags Flags { get; init; }

        /// <summary>
        /// Rounds per second the firearm can attempt to fire.
        /// </summary>
        public float FireRate { get; init; }

        /// <summary>
        /// Maximum number of rounds the firearm can hold after handling/mod
        /// modifiers are applied.
        /// </summary>
        public int MagazineSize { get; init; }

        /// <summary>
        /// Data-loaded cartridge or payload families this firearm can chamber.
        /// </summary>
        /// <remarks>
        /// This is intentionally granular and data-driven. A desert rifle can
        /// list values such as <c>7.62</c> and <c>50BMG</c> while rejecting
        /// <c>9mm</c> and <c>45ACP</c>. Broad gameplay categories such as
        /// <see cref="AmmoCategory.Light"/> or
        /// <see cref="AmmoCategory.AntiMaterial"/> come from the selected
        /// <see cref="AmmoDefinition"/> and are not used as the chambering key.
        /// </remarks>
        public IReadOnlySet<AmmoType> AcceptedAmmoTypes { get; init; } =
            new HashSet<AmmoType>();

        /// <summary>
        /// Base handling multiplier for raising the firearm into sights.
        /// </summary>
        public float BaseAimDownSightsSpeed { get; init; } = 1.0f;

        /// <summary>
        /// Base handling multiplier for unaimed fire.
        /// </summary>
        public float BaseHipFireAccuracy { get; init; } = 1.0f;

        /// <summary>
        /// Base handling multiplier for reload actions.
        /// </summary>
        public float BaseReloadSpeed { get; init; } = 1.0f;

        /// <summary>
        /// Base handling multiplier for equipping the firearm.
        /// </summary>
        public float BaseEquipSpeed { get; init; } = 1.0f;

        /// <summary>
        /// Base handling multiplier for recovering from recoil after ammo
        /// supplies the recoil impulse.
        /// </summary>
        public float BaseRecoilRecovery { get; init; } = 1.0f;

        /// <summary>
        /// Base reliability value used when the firearm can jam.
        /// </summary>
        public float BaseJamChance { get; init; } = 0.0f;

        /// <summary>
        /// Returns whether this firearm can load the supplied ammunition.
        /// </summary>
        /// <param name="ammoDefinition">The ammunition definition to test.</param>
        /// <returns>True when the ammo type is accepted by this firearm.</returns>
        public bool AcceptsAmmo(AmmoDefinition ammoDefinition)
        {
            if (ammoDefinition == null)
            {
                return false;
            }

            return ammoDefinition.Type.IsValid && AcceptedAmmoTypes.Contains(ammoDefinition.Type);
        }

        public static FirearmDefinition Load(JsonElement json)
        {
            var definition = new FirearmDefinition
            {
                BaseDurability = JsonLoader.GetRequired<float>(json, nameof(BaseDurability)),
                Slot = JsonLoader.GetRequired<WeaponSlotIndex>(json, nameof(Slot)),
                FireRate = JsonLoader.GetRequired<float>(json, nameof(FireRate)),
                MagazineSize = JsonLoader.GetRequired<int>(json, nameof(MagazineSize)),
                AcceptedAmmoTypes = JsonLoader.GetRequiredArray<string>(json, nameof(AcceptedAmmoTypes))
                    .Select(s => new AmmoType(s))
                    .ToHashSet(),
                BaseAimDownSightsSpeed = JsonLoader.GetOptional(json, nameof(BaseAimDownSightsSpeed), 1.0f),
                BaseHipFireAccuracy = JsonLoader.GetOptional(json, nameof(BaseHipFireAccuracy), 1.0f),
                BaseReloadSpeed = JsonLoader.GetOptional(json, nameof(BaseReloadSpeed), 1.0f),
                BaseEquipSpeed = JsonLoader.GetOptional(json, nameof(BaseEquipSpeed), 1.0f),
                BaseRecoilRecovery = JsonLoader.GetOptional(json, nameof(BaseRecoilRecovery), 1.0f),
                BaseJamChance = JsonLoader.GetOptional(json, nameof(BaseJamChance), 0.0f),
                Flags = JsonLoader.GetRequiredArray<string>(json, nameof(Flags))
                    .Select(s => Enum.TryParse(s, out FirearmFlags flag) ? flag : default)
                    .Aggregate((prev, next) => prev | next)
            };
            return definition;
        }
    }
}
