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

namespace Nomad.Game.Sdk.World
{
    /// <summary>
    /// Compatibility wrapper. New content should use Nomad.Game.Sdk.Biomes.BiomeDefinition.
    /// </summary>
    [Obsolete("Use Nomad.Game.Sdk.Biomes.BiomeDefinition. Biomes are environmental profiles, not world-owned places.")]
    public sealed record BiomeDefinition
    {
        public BiomeDefinitionId Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString JournalEntry { get; init; }
        public InternString WikiEntry { get; init; }
        public IReadOnlyList<CheckpointDefinitionId> Meliora { get; init; }
            = Array.Empty<CheckpointDefinitionId>();

        public static BiomeDefinition Load(JsonElement json)
        {
            Nomad.Game.Sdk.Biomes.BiomeDefinition biome = Nomad.Game.Sdk.Biomes.BiomeDefinition.Load(json);

            return new BiomeDefinition
            {
                Id = new BiomeDefinitionId(biome.Id),
                DisplayName = biome.DisplayName,
                JournalEntry = biome.JournalEntry,
                WikiEntry = biome.WikiEntry,
                Meliora = biome.Meliora
            };
        }

        public Nomad.Game.Sdk.Biomes.BiomeDefinition ToBiomeDefinition()
        {
            return new Nomad.Game.Sdk.Biomes.BiomeDefinition
            {
                Id = Id.ToBiomeDefinitionId(),
                DisplayName = DisplayName,
                JournalEntry = JournalEntry,
                WikiEntry = WikiEntry,
                Meliora = Meliora
            };
        }
    }
}
