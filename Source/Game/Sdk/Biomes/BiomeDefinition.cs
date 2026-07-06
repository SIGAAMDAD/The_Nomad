/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Interactables;

namespace Nomad.Game.Sdk.Biomes
{
    /// <summary>
    /// Environmental profile used by areas. Biomes are not gameplay places by themselves.
    /// </summary>
    public sealed record BiomeDefinition
    {
        public BiomeDefinitionId Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString TerrainProfileId { get; init; }
        public InternString WeatherProfileId { get; init; }
        public InternString ResourceProfileId { get; init; }
        public InternString EncounterProfileId { get; init; }
        public InternString AmbientAudioProfileId { get; init; }
        public InternString ColorGradeId { get; init; }

        // Kept for existing JSON compatibility. New content should put wiki/journal entries on AreaDefinition.
        public InternString JournalEntry { get; init; }
        public InternString WikiEntry { get; init; }

        public IReadOnlyList<CheckpointDefinitionId> Meliora { get; init; }
            = Array.Empty<CheckpointDefinitionId>();

        public static BiomeDefinition Load(JsonElement json)
        {
            return new BiomeDefinition
            {
                Id = new BiomeDefinitionId(new InternString(json.GetRequired<string>(nameof(Id)))),
                DisplayName = new InternString(json.GetRequired<string>(nameof(DisplayName))),
                TerrainProfileId = new InternString(json.GetRequired<string>(nameof(TerrainProfileId))),
                WeatherProfileId = new InternString(ReadString(json, nameof(WeatherProfileId), string.Empty)),
                ResourceProfileId = new InternString(ReadString(json, nameof(ResourceProfileId), string.Empty)),
                EncounterProfileId = new InternString(ReadString(json, nameof(EncounterProfileId), string.Empty)),
                AmbientAudioProfileId = new InternString(ReadString(json, nameof(AmbientAudioProfileId), string.Empty)),
                ColorGradeId = new InternString(ReadString(json, nameof(ColorGradeId), string.Empty)),
                JournalEntry = new InternString(ReadString(json, nameof(JournalEntry), string.Empty)),
                WikiEntry = new InternString(ReadString(json, nameof(WikiEntry), string.Empty)),
                Meliora = LoadMeliora(json)
            };
        }

        private static IReadOnlyList<CheckpointDefinitionId> LoadMeliora(JsonElement json)
        {
            if (!json.TryGetProperty(nameof(Meliora), out JsonElement melioraJson) || melioraJson.ValueKind != JsonValueKind.Array) {
                return Array.Empty<CheckpointDefinitionId>();
            }

            var meliora = new List<CheckpointDefinitionId>(melioraJson.GetArrayLength());

            foreach (JsonElement idJson in melioraJson.EnumerateArray()) {
                if (idJson.ValueKind == JsonValueKind.String) {
                    meliora.Add(new CheckpointDefinitionId(new InternString(idJson.GetString() ?? string.Empty)));
                }
            }

            return meliora;
        }

        private static string ReadString(JsonElement json, string name, string fallback)
        {
            if (json.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String) {
                return value.GetString() ?? fallback;
            }

            return fallback;
        }
    }
}
