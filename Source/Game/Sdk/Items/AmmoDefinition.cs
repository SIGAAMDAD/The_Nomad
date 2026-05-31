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

using System.Text.Json;
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Items
{
    /// <summary>
    /// Defines a stackable ammunition item.
    /// </summary>
    /// <remarks>
    /// Ammo owns ballistic output. Damage, range, projectile velocity, recoil,
    /// and special projectile behavior live here so the same firearm can feel
    /// different when loaded with different supported cartridges.
    /// </remarks>
    public sealed record AmmoDefinition : ItemDefinition
    {
        public override ItemType BaseType => ItemType.Ammunition;

        /// <summary>
        /// Data-loaded cartridge or payload family this ammo belongs to.
        /// Firearms match against this value when deciding whether they can
        /// chamber the ammo.
        /// </summary>
        public AmmoType Type { get; init; }

        /// <summary>
        /// Fixed gameplay category supplied by this loaded ammo type.
        /// </summary>
        public AmmoCategory Category { get; init; }

        /// <summary>
        /// Optional projectile modifier, such as armor piercing or incendiary.
        /// The firearm still decides whether that modifier is safe to use.
        /// </summary>
        public AmmoModifier Modifier { get; init; }

        /// <summary>
        /// Effective projectile range supplied by this ammunition.
        /// </summary>
        public float Range { get; init; }

        /// <summary>
        /// Projectile travel speed supplied by this ammunition.
        /// </summary>
        public float Velocity { get; init; }

        /// <summary>
        /// Damage supplied by this ammunition.
        /// </summary>
        public float Damage { get; init; }

        /// <summary>
        /// Recoil impulse supplied by this ammunition.
        /// </summary>
        public float Recoil { get; init; }

        public static AmmoDefinition Load(JsonElement json)
        {
            return new AmmoDefinition
            {
                Type = new AmmoType(JsonLoader.GetRequired<string>(json, nameof(Type))),
                Category = JsonLoader.GetRequired<AmmoCategory>(json, nameof(Category)),
                Modifier = JsonLoader.GetRequired<AmmoModifier>(json, nameof(Modifier)),
                Damage = JsonLoader.GetRequired<float>(json, nameof(Damage)),
                Velocity = JsonLoader.GetRequired<float>(json, nameof(Velocity)),
                Range = JsonLoader.GetRequired<float>(json, nameof(Range)),
                Recoil = JsonLoader.GetRequired<float>(json, nameof(Recoil))
            };
        }
    }
}
