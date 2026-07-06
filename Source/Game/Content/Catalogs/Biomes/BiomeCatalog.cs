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
using System.Text.Json;
using Nomad.Core.FileSystem;
using Nomad.Core.Logger;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Content.Catalogs.Biomes
{
	internal class BiomeCatalog : DataDefinitionRegistry<BiomeDefinitionId, BiomeDefinition>
	{
		protected override Func<string, BiomeDefinitionId> KeyFactory => k => new BiomeDefinitionId( new InternString( k ) );
		protected override string LoggerCategoryName => nameof( BiomeCatalog );

		public BiomeCatalog( IFileSystem fileSystem, ILoggerService logger )
			: base( fileSystem, logger )
		{
		}

		protected override bool TryLoadDefinition( JsonElement json, out BiomeDefinition definition )
		{
			definition = BiomeDefinition.Load( json );
			return definition.Id.IsValid;
		}
	};
};
