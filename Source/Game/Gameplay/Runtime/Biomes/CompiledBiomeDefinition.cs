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

using Nomad.Core.Util;
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Gameplay.Runtime.Biomes
{
	internal readonly struct CompiledBiomeDefinition
	{
		public readonly BiomeDefinitionId Id;
		public readonly ushort BiomeIndex;
		public readonly InternString DisplayName;
		public readonly InternString TerrainProfileId;
		public readonly InternString WeatherProfileId;
		public readonly InternString ResourceProfileId;
		public readonly InternString EncounterProfileId;
		public readonly InternString AmbientAudioProfileId;
		public readonly InternString ColorGradeId;

		public CompiledBiomeDefinition(
			BiomeDefinitionId id,
			ushort biomeIndex,
			InternString displayName,
			InternString terrainProfileId,
			InternString weatherProfileId,
			InternString resourceProfileId,
			InternString encounterProfileId,
			InternString ambientAudioProfileId,
			InternString colorGradeId
		)
		{
			Id = id;
			BiomeIndex = biomeIndex;
			DisplayName = displayName;
			TerrainProfileId = terrainProfileId;
			WeatherProfileId = weatherProfileId;
			ResourceProfileId = resourceProfileId;
			EncounterProfileId = encounterProfileId;
			AmbientAudioProfileId = ambientAudioProfileId;
			ColorGradeId = colorGradeId;
		}
	};
};
