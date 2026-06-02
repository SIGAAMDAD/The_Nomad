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
using Nomad.Game.Sdk.Interactables;

namespace Nomad.Game.Sdk.World
{
    /// <summary>
    ///
    /// </summary>
    public sealed record BiomeDefinition
    {
        public BiomeDefinitionId Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString JournalEntry { get; init; }
        public InternString WikiEntry { get; init; }

        public IReadOnlyList<CheckpointDefinitionId> Meliora { get; init; }

        public static BiomeDefinition Load(JsonElement json)
        {
            return new BiomeDefinition
            {
                Id = new BiomeDefinitionId(new InternString(JsonLoader.GetRequired<string>(json, nameof(Id)))),
                DisplayName = new InternString(JsonLoader.GetRequired<string>(json, nameof(DisplayName))),
                JournalEntry = new InternString(JsonLoader.GetRequired<string>(json, nameof(JournalEntry))),
                WikiEntry = new InternString(JsonLoader.GetRequired<string>(json, nameof(WikiEntry))),
                Meliora = JsonLoader.GetRequiredArray<string>(json, nameof(Meliora))
                    .Select(s => new CheckpointDefinitionId(new InternString(s)))
                    .ToArray()
            };
        }
    }
}
