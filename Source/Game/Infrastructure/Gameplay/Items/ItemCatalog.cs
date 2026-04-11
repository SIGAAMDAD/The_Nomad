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
using System.Collections.Concurrent;
using Nomad.Core.FileSystem;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Items;

namespace Nomad.Game.Infrastructure.Gameplay.Items {
	internal sealed class ItemCatalog : DataLoader {
		protected override string dataPath => "Assets/Items/";
		protected override string extensionPattern => "*.item";

		private readonly ConcurrentDictionary<Guid, ItemDefinition> _dataCache = new();

		public ItemCatalog( IFileSystem fileSystem )
			: base( fileSystem )
		{
		}

		/*
		===============
		LoadDefinition
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		protected override bool LoadDefinition( string filePath ) {
			using var fileBuffer = fileSystem.LoadFile( filePath );
			if ( fileBuffer == null ) {
				return false;
			}

			using var stream = fileBuffer.AsStream( 0, fileBuffer.Length );
			using var document = JsonLoader.Parse( stream );

			var definition = new ItemDefinition {
				Weight = JsonLoader.GetOptional<float>( document.RootElement, "Weight", 0.0f ),
				Name = JsonLoader.GetOptional<string>( document.RootElement, "Name", "UNKNOWN" ),
				Type = JsonLoader.GetOptional<ItemType>( document.RootElement, "Type", ItemType.Consumable )
			};
			_dataCache.AddOrUpdate( Guid.NewGuid(), definition, ( id, old ) => {
				old = old with {
					Name = definition.Name,
					Weight = definition.Weight,
					Type = definition.Type
				};
				return old;
			} );

			return true;
		}

		public ItemDefinition? Get( Guid itemId ) {
			return _dataCache.TryGetValue( itemId, out var item ) ? item : null;
		}
	};
};
