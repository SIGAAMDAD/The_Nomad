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
using Nomad.Game.Gameplay.Runtime.Areas;
using Nomad.Game.Gameplay.Runtime.Biomes;
using Nomad.Game.Sdk.Areas;
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Content.Compilation.Areas
{
	/// <summary>
	/// Converts authored AreaDefinitions into indexed, immutable runtime area data.
	/// </summary>
	internal sealed class AreaCompiler
	{
		private const ushort INVALID_INDEX = ushort.MaxValue;

		public CompiledAreaDatabase Compile( IEnumerable<AreaDefinition> definitions, CompiledBiomeDatabase? biomeDatabase = null )
		{
			if ( definitions == null ) {
				return CompiledAreaDatabase.Empty;
			}

			biomeDatabase ??= CompiledBiomeDatabase.Empty;

			var source = new List<AreaDefinition>();
			var indexById = new Dictionary<AreaDefinitionId, ushort>();

			foreach ( AreaDefinition definition in definitions ) {
				if ( !definition.Id.IsValid || indexById.ContainsKey( definition.Id ) ) {
					continue;
				}

				ushort index = checked((ushort)source.Count);
				source.Add( definition );
				indexById.Add( definition.Id, index );
			}

			var areas = new CompiledAreaDefinition[source.Count];
			var biomeLayers = new List<CompiledAreaBiomeLayer>();

			for ( int i = 0; i < source.Count; i++ ) {
				AreaDefinition definition = source[i];
				ushort areaIndex = checked((ushort)i);
				ushort parentIndex = ResolveParentIndex( definition.ParentAreaId, indexById );
				ushort firstBiomeLayer = checked((ushort)biomeLayers.Count);

				for ( int layerIndex = 0; layerIndex < definition.Biomes.Count; layerIndex++ ) {
					AreaBiomeLayerDefinition layer = definition.Biomes[layerIndex];
					ushort biomeIndex = ResolveBiomeIndex( layer.BiomeId, biomeDatabase );

					biomeLayers.Add(
						new CompiledAreaBiomeLayer(
							layer.BiomeId,
							biomeIndex,
							layer.Weight
						)
					);
				}

				ushort biomeLayerCount = checked((ushort)(biomeLayers.Count - firstBiomeLayer));

				areas[i] = new CompiledAreaDefinition(
					definition.Id,
					areaIndex,
					definition.Kind,
					definition.ParentAreaId,
					parentIndex,
					firstBiomeLayer,
					biomeLayerCount,
					definition.Progress.LandmarkCount,
					definition.Progress.ActivityCount,
					definition.Progress.CollectibleCount,
					definition.Progress.FastTravelPointCount,
					definition.Flags,
					definition.Name,
					definition.WikiId,
					definition.JournalEntryDiscoveredId,
					definition.JournalEntryUndiscoveredId
				);
			}

			return new CompiledAreaDatabase( areas, biomeLayers.ToArray(), indexById );
		}

		private static ushort ResolveParentIndex( AreaDefinitionId parentAreaId, Dictionary<AreaDefinitionId, ushort> indexById )
		{
			if ( !parentAreaId.IsValid ) {
				return INVALID_INDEX;
			}

			return indexById.TryGetValue( parentAreaId, out ushort index ) ? index : INVALID_INDEX;
		}

		private static ushort ResolveBiomeIndex( BiomeDefinitionId biomeId, CompiledBiomeDatabase biomeDatabase )
		{
			if ( !biomeId.IsValid ) {
				return INVALID_INDEX;
			}

			return biomeDatabase.TryGetIndex( biomeId, out ushort index ) ? index : INVALID_INDEX;
		}
	};
};
