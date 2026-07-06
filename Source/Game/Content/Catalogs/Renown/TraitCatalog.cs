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
using Nomad.Game.Sdk.Renown.Regional;
using Nomad.Game.Content.Catalogs;

namespace Nomad.Game.Content.Catalogs.Renown
{
	/*
	===================================================================================

	TraitCatalog

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class TraitCatalog : DataDefinitionRegistry<TraitDefinitionId, TraitDefinition>
	{
		protected override Func<string, TraitDefinitionId> KeyFactory => k => new TraitDefinitionId( new InternString( k ) );
		protected override string LoggerCategoryName => nameof( TraitCatalog );

		/*
		===============
		TraitCatalog
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fileSystem"></param>
		public TraitCatalog( IFileSystem fileSystem, ILoggerService logger )
			: base( fileSystem, logger )
		{
		}

		/*
		===============
		TryLoadDefinition
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="json"></param>
		/// <param name="definition"></param>
		/// <returns></returns>
		protected override bool TryLoadDefinition( JsonElement json, out TraitDefinition definition )
		{
			definition = new TraitDefinition {

			};

			return true;
		}
	};
};
