/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Sdk.Areas
{
    /// <summary>
    /// Authoring link from a named area to one environmental biome profile.
    /// </summary>
    public readonly struct AreaBiomeLayerDefinition
    {
        public readonly BiomeDefinitionId BiomeId;
        public readonly float Weight;

        public AreaBiomeLayerDefinition(BiomeDefinitionId biomeId, float weight)
        {
            BiomeId = biomeId;
            Weight = weight;
        }

        public static AreaBiomeLayerDefinition Load(JsonElement json)
        {
            string biomeId = json.GetRequired<string>(nameof(BiomeId));
            float weight = json.TryGet<float>(nameof(Weight), out var value) ? value : 1.0f;

            return new AreaBiomeLayerDefinition(
                new BiomeDefinitionId(new InternString(biomeId)),
                weight
            );
        }
    }
}
