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

namespace Nomad.Game.Gameplay.Runtime.Renown
{
	internal sealed class CompiledRegionalRenownDatabase
	{
		public static readonly CompiledRegionalRenownDatabase Empty = new CompiledRegionalRenownDatabase(
			Array.Empty<CompiledRegionalRenownDefinition>(),
			new Dictionary<AreaDefinitionId, ushort>()
		);

		private readonly CompiledRegionalRenownDefinition[] _regionalRenown;
		private readonly Dictionary<AreaDefinitionId, ushort> _indexByAreaId;

		public int Count => _regionalRenown.Length;
		public ReadOnlySpan<CompiledRegionalRenownDefinition> RegionalRenown => _regionalRenown;

		public CompiledRegionalRenownDatabase(
			CompiledRegionalRenownDefinition[] regionalRenown,
			Dictionary<AreaDefinitionId, ushort> indexByAreaId
		)
		{
			_regionalRenown = regionalRenown ?? Array.Empty<CompiledRegionalRenownDefinition>();
			_indexByAreaId = indexByAreaId ?? new Dictionary<AreaDefinitionId, ushort>();
		}

		public bool TryGetIndex( AreaDefinitionId areaId, out ushort index )
		{
			return _indexByAreaId.TryGetValue( areaId, out index );
		}

		public bool TryGet( AreaDefinitionId areaId, out CompiledRegionalRenownDefinition definition )
		{
			if ( _indexByAreaId.TryGetValue( areaId, out ushort index ) ) {
				definition = _regionalRenown[index];
				return true;
			}

			definition = default;
			return false;
		}

		public ref readonly CompiledRegionalRenownDefinition GetByIndex( ushort index )
		{
			return ref _regionalRenown[index];
		}
	};
};
