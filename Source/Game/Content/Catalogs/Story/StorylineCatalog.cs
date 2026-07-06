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
using Nomad.Game.Sdk.Story;
using Nomad.Game.Content.Catalogs;

namespace Nomad.Game.Content.Catalogs.Story
{
	internal sealed class StorylineCatalog : DataDefinitionRegistry<StorylineDefinitionId, StorylineDefinition>
	{
		protected override Func<string, StorylineDefinitionId> KeyFactory => k => new StorylineDefinitionId( new InternString( k ) );
		protected override string LoggerCategoryName => nameof( StorylineCatalog );

		public StorylineCatalog( IFileSystem fileSystem, ILoggerService logger )
			: base( fileSystem, logger )
		{
		}

		public void ScanAndLoad( string dataPath = "Assets/Storylines" )
		{
			ScanDirectory( dataPath, "*.json" );
		}

		public bool TryRegister( StorylineDefinition definition )
		{
			if ( definition == null || !definition.Id.IsValid ) {
				return false;
			}

			dataCache[definition.Id] = definition;
			return true;
		}

		protected override bool TryLoadDefinition( JsonElement json, out StorylineDefinition definition )
		{
			definition = StorylineDefinition.Load( json );
			return definition != null && definition.Id.IsValid;
		}
	};
};
