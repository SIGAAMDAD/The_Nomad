/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.
===========================================================================
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Sdk.Areas
{
    /// <summary>
    /// Named gameplay/narrative place of any scale: biome-zone, city, district, village, interior, dungeon, or landmark.
    /// </summary>
    public sealed record AreaDefinition
    {
        public AreaDefinitionId Id { get; init; }
        public InternString Name { get; init; }
        public AreaKind Kind { get; init; }
        public AreaDefinitionId ParentAreaId { get; init; }

        public InternString WikiId { get; init; }
        public InternString JournalEntryDiscoveredId { get; init; }
        public InternString JournalEntryUndiscoveredId { get; init; }

        public IReadOnlyList<AreaBiomeLayerDefinition> Biomes { get; init; }
            = Array.Empty<AreaBiomeLayerDefinition>();

        public AreaProgressDefinition Progress { get; init; }
        public AreaFlags Flags { get; init; }

        public static AreaDefinition Load(JsonElement json)
        {
            return new AreaDefinition
            {
                Id = new AreaDefinitionId(new InternString(ReadString(json, nameof(Id), string.Empty))),
                Name = new InternString(ReadString(json, nameof(Name), string.Empty)),
                Kind = ReadEnum(json, nameof(Kind), AreaKind.Region),
                ParentAreaId = new AreaDefinitionId(new InternString(ReadString(json, nameof(ParentAreaId), string.Empty))),
                WikiId = new InternString(ReadString(json, nameof(WikiId), string.Empty)),
                JournalEntryDiscoveredId = new InternString(ReadString(json, nameof(JournalEntryDiscoveredId), string.Empty)),
                JournalEntryUndiscoveredId = new InternString(ReadString(json, nameof(JournalEntryUndiscoveredId), string.Empty)),
                Biomes = LoadBiomeLayers(json),
                Progress = LoadProgress(json),
                Flags = ReadEnum(json, nameof(Flags), AreaFlags.Discoverable | AreaFlags.TrackCompletion)
            };
        }

        private static IReadOnlyList<AreaBiomeLayerDefinition> LoadBiomeLayers(JsonElement json)
        {
            if (json.TryGetProperty(nameof(Biomes), out JsonElement biomesJson) && biomesJson.ValueKind == JsonValueKind.Array) {
                var biomes = new List<AreaBiomeLayerDefinition>(biomesJson.GetArrayLength());

                foreach (JsonElement biomeJson in biomesJson.EnumerateArray()) {
                    biomes.Add(AreaBiomeLayerDefinition.Load(biomeJson));
                }

                return biomes;
            }

            if (json.TryGetProperty("BiomeId", out JsonElement biomeIdJson) && biomeIdJson.ValueKind == JsonValueKind.String) {
                return new[] {
                    new AreaBiomeLayerDefinition(
                        new BiomeDefinitionId(new InternString(biomeIdJson.GetString() ?? string.Empty)),
                        1.0f
                    )
                };
            }

            return Array.Empty<AreaBiomeLayerDefinition>();
        }

        private static AreaProgressDefinition LoadProgress(JsonElement json)
        {
            if (json.TryGetProperty(nameof(Progress), out JsonElement progressJson) && progressJson.ValueKind == JsonValueKind.Object) {
                return AreaProgressDefinition.Load(progressJson);
            }

            return AreaProgressDefinition.Empty;
        }

        private static string ReadString(JsonElement json, string name, string fallback)
        {
            if (json.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String) {
                return value.GetString() ?? fallback;
            }

            return fallback;
        }

        private static TEnum ReadEnum<TEnum>(JsonElement json, string name, TEnum fallback)
            where TEnum : struct
        {
            if (!json.TryGetProperty(name, out JsonElement value)) {
                return fallback;
            }

            if (value.ValueKind == JsonValueKind.String && Enum.TryParse(value.GetString(), true, out TEnum parsed)) {
                return parsed;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number)) {
                return (TEnum)Enum.ToObject(typeof(TEnum), number);
            }

            return fallback;
        }
    }
}
