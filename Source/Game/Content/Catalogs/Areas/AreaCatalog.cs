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
using Nomad.Game.Sdk.Areas;

namespace Nomad.Game.Content.Catalogs.Areas
{
	internal class AreaCatalog : DataDefinitionRegistry<AreaDefinitionId, AreaDefinition>
	{
		protected override Func<string, AreaDefinitionId> KeyFactory => k => new AreaDefinitionId( new InternString( k ) );
		protected override string LoggerCategoryName => nameof( AreaCatalog );

		public AreaCatalog( IFileSystem fileSystem, ILoggerService logger )
			: base( fileSystem, logger )
		{
		}

		protected override bool TryLoadDefinition( JsonElement json, out AreaDefinition definition )
		{
			definition = AreaDefinition.Load( json );
			return definition.Id.IsValid;
		}
	};
};
