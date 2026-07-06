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
using Nomad.Game.Gameplay.Runtime.Biomes;
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Content.Compilation.Biomes
{
	internal sealed class BiomeCompiler
	{
		public CompiledBiomeDatabase Compile( IEnumerable<BiomeDefinition> definitions )
		{
			if ( definitions == null ) {
				return CompiledBiomeDatabase.Empty;
			}

			var biomes = new List<CompiledBiomeDefinition>();
			var indexById = new Dictionary<BiomeDefinitionId, ushort>();

			foreach ( BiomeDefinition definition in definitions ) {
				if ( !definition.Id.IsValid || indexById.ContainsKey( definition.Id ) ) {
					continue;
				}

				ushort index = checked((ushort)biomes.Count);
				indexById.Add( definition.Id, index );

				biomes.Add(
					new CompiledBiomeDefinition(
						definition.Id,
						index,
						definition.DisplayName,
						definition.TerrainProfileId,
						definition.WeatherProfileId,
						definition.ResourceProfileId,
						definition.EncounterProfileId,
						definition.AmbientAudioProfileId,
						definition.ColorGradeId
					)
				);
			}

			return new CompiledBiomeDatabase( biomes.ToArray(), indexById );
		}
	};
};
