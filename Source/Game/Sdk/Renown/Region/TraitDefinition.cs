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

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Renown.Region
{
    public record TraitDefinition
    {
        public InternString Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString Description { get; init; }

        public int OutstandingMargin { get; init; }
        public bool IsRegionBased { get; init; }
        public bool CanBleedToAdjacent { get; init; }
        public bool CanDecay { get; init; }

        /// <summary>
        /// How fast the trait score bleeds into other regions.
        /// </summary>
        public float SpreadRate { get; init; }

        /// <summary>
        /// The number of days until the trait score starts to decay.
        /// </summary>
        public float DecayDelayDays { get; init; }

        /// <summary>
        /// The memory fade rate of a trait when decay starts.
        /// </summary>
        public float DecayRatePerDay { get; init; }

        public int MinScore { get; init; }
        public int MaxScore { get; init; }

        public HashSet<InternString> ConflictingTraits { get; init; }
        public Dictionary<InternString, float> FactionBias { get; init; }

        public static TraitDefinition Load(JsonElement json)
        {
            return new TraitDefinition
            {
                Id = new InternString(JsonLoader.GetRequired<string>(json, nameof(Id))),
                DisplayName = new InternString(JsonLoader.GetRequired<string>(json, nameof(DisplayName))),
                Description = new InternString(JsonLoader.GetRequired<string>(json, nameof(Description))),
                OutstandingMargin = JsonLoader.GetRequired<int>(json, nameof(OutstandingMargin)),
                IsRegionBased = JsonLoader.GetRequired<bool>(json, nameof(IsRegionBased)),
                CanBleedToAdjacent = JsonLoader.GetRequired<bool>(json, nameof(CanBleedToAdjacent)),
                ConflictingTraits = JsonLoader.GetRequiredArray<string>(json, nameof(ConflictingTraits))
                    .Select(s => new InternString(s))
                    .ToHashSet()
            };
        }
    }
}
