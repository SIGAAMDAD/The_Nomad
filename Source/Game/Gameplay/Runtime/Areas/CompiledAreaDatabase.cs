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
using Nomad.Game.Sdk.Areas;

namespace Nomad.Game.Gameplay.Runtime.Areas
{
	internal sealed class CompiledAreaDatabase
	{
		public static readonly CompiledAreaDatabase Empty = new CompiledAreaDatabase(
			Array.Empty<CompiledAreaDefinition>(),
			Array.Empty<CompiledAreaBiomeLayer>(),
			new Dictionary<AreaDefinitionId, ushort>()
		);

		private readonly CompiledAreaDefinition[] _areas;
		private readonly CompiledAreaBiomeLayer[] _biomeLayers;
		private readonly Dictionary<AreaDefinitionId, ushort> _indexById;

		public int Count => _areas.Length;
		public ReadOnlySpan<CompiledAreaDefinition> Areas => _areas;
		public ReadOnlySpan<CompiledAreaBiomeLayer> BiomeLayers => _biomeLayers;

		public CompiledAreaDatabase(
			CompiledAreaDefinition[] areas,
			CompiledAreaBiomeLayer[] biomeLayers,
			Dictionary<AreaDefinitionId, ushort> indexById
		)
		{
			_areas = areas ?? Array.Empty<CompiledAreaDefinition>();
			_biomeLayers = biomeLayers ?? Array.Empty<CompiledAreaBiomeLayer>();
			_indexById = indexById ?? new Dictionary<AreaDefinitionId, ushort>();
		}

		public bool TryGetIndex( AreaDefinitionId id, out ushort index )
		{
			return _indexById.TryGetValue( id, out index );
		}

		public bool TryGet( AreaDefinitionId id, out CompiledAreaDefinition area )
		{
			if ( _indexById.TryGetValue( id, out ushort index ) ) {
				area = _areas[index];
				return true;
			}

			area = default;
			return false;
		}

		public ref readonly CompiledAreaDefinition GetByIndex( ushort index )
		{
			return ref _areas[index];
		}

		public ReadOnlySpan<CompiledAreaBiomeLayer> GetBiomeLayers( in CompiledAreaDefinition area )
		{
			return _biomeLayers.AsSpan( area.FirstBiomeLayer, area.BiomeLayerCount );
		}
	};
};
