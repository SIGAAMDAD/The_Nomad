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
using Nomad.Core.FileSystem;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Items;

namespace Nomad.Game.Infrastructure.Gameplay.Items {
	/*
	===================================================================================
	
	AmmoCatalog
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class AmmoCatalog : DataLoader<AmmoDefinition> {
		protected override string dataPath => "Assets/Items/Ammo";
		protected override string extensionPattern => "*.ammodef";

		/*
		===============
		AmmoCatalog
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileSystem"></param>
		public AmmoCatalog( IFileSystem fileSystem )
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
		protected override bool TryLoadDefinition( JsonElement json, out AmmoDefinition definition ) {
			if ( JsonLoader.TryGet<ItemType>( json, nameof( definition.BaseType ), out var baseType ) || baseType != ItemType.Ammunition ) {
				definition = null;
				return false;
			}
			
			definition = new AmmoDefinition {
				Damage = JsonLoader.TryGet( json, nameof( definition.Damage ), out float damage ) ? damage : 0.0f,
				Velocity = JsonLoader.TryGet( json, nameof( definition.Velocity ), out float velocity ) ? velocity : 0.0f,
				Range = JsonLoader.TryGet( json, nameof( definition.Range ), out float range ) ? range : 0.0f,
			};
			ItemCatalog.LoadItemBase( json, definition );

			return true;
		}
	};
};
