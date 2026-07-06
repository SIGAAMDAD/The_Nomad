/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Areas;

namespace Nomad.Game.Sdk.Renown.Regional
{
    /// <summary>
    /// Authored renown tuning for one area/region scope.
    /// </summary>
    public sealed record RegionalRenownDefinition
    {
        public RegionalRenownDefinitionId Id { get; init; }
        public AreaDefinitionId AreaId { get; init; }
        public float InitialRenownScore { get; init; }
        public float DecayPerDay { get; init; }
        public float SpreadMultiplier { get; init; } = 1.0f;
        public float HistoricalFloor { get; init; }

        public static RegionalRenownDefinition Load(JsonElement json)
        {
            return new RegionalRenownDefinition
            {
                Id = new RegionalRenownDefinitionId(new InternString(ReadString(json, nameof(Id), string.Empty))),
                AreaId = new AreaDefinitionId(new InternString(ReadString(json, nameof(AreaId), string.Empty))),
                InitialRenownScore = ReadSingle(json, nameof(InitialRenownScore), 0.0f),
                DecayPerDay = ReadSingle(json, nameof(DecayPerDay), 0.0f),
                SpreadMultiplier = ReadSingle(json, nameof(SpreadMultiplier), 1.0f),
                HistoricalFloor = ReadSingle(json, nameof(HistoricalFloor), 0.0f)
            };
        }

        private static string ReadString(JsonElement json, string name, string fallback)
        {
            if (json.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String) {
                return value.GetString() ?? fallback;
            }

            return fallback;
        }

        private static float ReadSingle(JsonElement json, string name, float fallback)
        {
            if (json.TryGetProperty(name, out JsonElement value) && value.TryGetSingle(out float result)) {
                return result;
            }

            return fallback;
        }
    }
}
