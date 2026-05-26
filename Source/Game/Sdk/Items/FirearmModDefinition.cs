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

namespace Nomad.Game.Sdk.Items
{
    public sealed record FirearmModDefinition : ItemDefinition
    {
        public override ItemType BaseType => ItemType.FirearmMod;

        public FirearmModSlot Slot { get; init; }

        public FirearmModFlags Flags { get; init; }

        /// <summary>
        /// Firearm flags required for this mod to be installed.
        /// Example: suppressor requires SuppressorCompatible.
        /// </summary>
        public FirearmFlags RequiredFirearmFlags { get; init; }

        /// <summary>
        /// Firearm flags that prevent this mod from being installed.
        /// Example: some optics may block BreakAction or HeavyWeapon if desired.
        /// </summary>
        public FirearmFlags BlockedFirearmFlags { get; init; }

        /// <summary>
        /// Flags added to the firearm while this mod is installed.
        /// Example: suppressor may remove LoudReport through RemovedFirearmFlags
        /// and add Concealable/RevealsShooterPosition changes if desired.
        /// </summary>
        public FirearmFlags AddedFirearmFlags { get; init; }

        /// <summary>
        /// Flags suppressed while this mod is installed.
        /// Example: suppressor can suppress LoudReport or MuzzleFlashHeavy.
        /// </summary>
        public FirearmFlags RemovedFirearmFlags { get; init; }

        /// <summary>
        /// Numeric handling/reliability/signature modifiers.
        /// Damage/range/penetration should generally remain ammo-owned.
        /// </summary>
        public IReadOnlyList<FirearmStatModifier> Modifiers { get; init; } =
            Array.Empty<FirearmStatModifier>();

        public static FirearmModDefinition Load(JsonElement json)
        {
            return new FirearmModDefinition
            {
                Id = new ItemDefinitionId(new InternString(JsonLoader.GetRequired<string>(json, nameof(Id)))),
                Slot = JsonLoader.GetRequired<FirearmModSlot>(json, nameof(Slot)),

                Flags = JsonLoader.GetRequiredArray<string>(json, nameof(Flags))
                    .Select(s => Enum.TryParse(s, out FirearmModFlags flag) ? flag : default)
                    .Aggregate(FirearmModFlags.None, (prev, next) => prev | next),

                RequiredFirearmFlags = LoadFirearmFlagsOrNone(json, nameof(RequiredFirearmFlags)),
                BlockedFirearmFlags = LoadFirearmFlagsOrNone(json, nameof(BlockedFirearmFlags)),
                AddedFirearmFlags = LoadFirearmFlagsOrNone(json, nameof(AddedFirearmFlags)),
                RemovedFirearmFlags = LoadFirearmFlagsOrNone(json, nameof(RemovedFirearmFlags)),

                Modifiers = LoadModifiers(json)
            };
        }

        private static FirearmFlags LoadFirearmFlagsOrNone(JsonElement json, string property)
        {
            if (!json.TryGetProperty(property, out JsonElement element))
            {
                return FirearmFlags.None;
            }

            return JsonLoader.GetRequiredArray<string>(json, property)
                .Select(s => Enum.TryParse(s, out FirearmFlags flag) ? flag : default)
                .Aggregate(FirearmFlags.None, (prev, next) => prev | next);
        }

        private static IReadOnlyList<FirearmStatModifier> LoadModifiers(JsonElement json)
        {
            if (!json.TryGetProperty(nameof(Modifiers), out JsonElement modifiersElement) ||
                modifiersElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<FirearmStatModifier>();
            }

            var modifiers = new List<FirearmStatModifier>();

            foreach (JsonElement modifierJson in modifiersElement.EnumerateArray())
            {
                modifiers.Add(
                    new FirearmStatModifier(
                        JsonLoader.GetRequired<FirearmModStat>(modifierJson, nameof(FirearmStatModifier.Stat)),
                        JsonLoader.GetRequired<FirearmModOperation>(modifierJson, nameof(FirearmStatModifier.Operation)),
                        JsonLoader.GetRequired<float>(modifierJson, nameof(FirearmStatModifier.Value))
                    )
                );
            }

            return modifiers;
        }
    }
}
