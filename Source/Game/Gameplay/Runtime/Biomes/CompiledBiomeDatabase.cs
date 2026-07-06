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
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Gameplay.Runtime.Biomes
{
	internal sealed class CompiledBiomeDatabase
	{
		public static readonly CompiledBiomeDatabase Empty = new CompiledBiomeDatabase(
			Array.Empty<CompiledBiomeDefinition>(),
			new Dictionary<BiomeDefinitionId, ushort>()
		);

		private readonly CompiledBiomeDefinition[] _biomes;
		private readonly Dictionary<BiomeDefinitionId, ushort> _indexById;

		public int Count => _biomes.Length;
		public ReadOnlySpan<CompiledBiomeDefinition> Biomes => _biomes;

		public CompiledBiomeDatabase(
			CompiledBiomeDefinition[] biomes,
			Dictionary<BiomeDefinitionId, ushort> indexById
		)
		{
			_biomes = biomes ?? Array.Empty<CompiledBiomeDefinition>();
			_indexById = indexById ?? new Dictionary<BiomeDefinitionId, ushort>();
		}

		public bool TryGetIndex( BiomeDefinitionId id, out ushort index )
		{
			return _indexById.TryGetValue( id, out index );
		}

		public bool TryGet( BiomeDefinitionId id, out CompiledBiomeDefinition definition )
		{
			if ( _indexById.TryGetValue( id, out ushort index ) ) {
				definition = _biomes[index];
				return true;
			}

			definition = default;
			return false;
		}

		public ref readonly CompiledBiomeDefinition GetByIndex( ushort index )
		{
			return ref _biomes[index];
		}
	};
};
