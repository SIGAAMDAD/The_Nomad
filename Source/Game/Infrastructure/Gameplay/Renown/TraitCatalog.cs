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

using System.Text.Json;
using Godot;
using Nomad.Core.FileSystem;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Renown;

namespace Nomad.Game.Infrastructure.Gameplay.Renown
{
	/*
	===================================================================================
	
	TraitCatalog
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class TraitCatalog : DataLoader<TraitDefinition>
	{
		/*
		===============
		TraitCatalog
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileSystem"></param>
		public TraitCatalog( IFileSystem fileSystem )
			: base( fileSystem )
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
				Id = new InternString( JsonLoader.GetRequired<string>( json, nameof( definition.Id ) ) ),
				DisplayName = new InternString( JsonLoader.GetRequired<string>( json, nameof( definition.DisplayName ) ) ),
				Description = new InternString( JsonLoader.GetRequired<string>( json, nameof( definition.Description ) ) ),
				OutstandingMargin = JsonLoader.GetRequired<int>( json, nameof( definition.OutstandingMargin ) ),
				IsRegionBased = JsonLoader.GetRequired<bool>( json, nameof( definition.IsRegionBased ) ),
				CanBleedToAdjacent = JsonLoader.GetRequired<bool>( json, nameof( definition.CanBleedToAdjacent ) ),
			};

			return true;
		}
	};
};
