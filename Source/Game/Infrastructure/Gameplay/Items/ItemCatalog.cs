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
using System.Text.Json;
using Nomad.Core.FileSystem;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Interfaces.Items;

namespace Nomad.Game.Infrastructure.Gameplay.Items {
	/*
	===================================================================================
	
	ItemCatalog
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class ItemCatalog : DataLoader<ItemDefinition>, IItemCatalog {
		private readonly Dictionary<ItemType, Func<JsonElement, ItemDefinition>> _loaders = new();

		/*
		===============
		ItemCatalog
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileSystem"></param>
		public ItemCatalog( IFileSystem fileSystem )
			: base( fileSystem )
		{
		}

		public TItemDefinition? Get<TItemDefinition>( Guid itemId )
			where TItemDefinition : ItemDefinition
		{
			return (TItemDefinition?)Get( itemId );
		}

		public bool TryGet<TItemDefinition>( Guid itemId, out TItemDefinition? item )
			where TItemDefinition : ItemDefinition
		{
			if ( !TryGet( itemId, out var data ) ) {
				item = null;
				return false;
			}
			item = (TItemDefinition?)data;
			return true;
		}

		/*
		===============
		AddLoader
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="callback"></param>
		public void AddLoader( ItemType type, Func<JsonElement, ItemDefinition> callback ) {
			_loaders[type] = callback;
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
		/// <exception cref="InvalidOperationException"></exception>
		protected override bool TryLoadDefinition( JsonElement json, out ItemDefinition definition ) {
			definition = null;
			if ( !JsonLoader.TryGet<ItemType>( json, nameof( definition.BaseType ), out var baseType ) ) {
				return false;
			}
			if ( !_loaders.TryGetValue( baseType, out var callback ) ) {
				throw new InvalidOperationException();
			}
			definition = callback.Invoke( json );
			definition = LoadItemBase( json, definition );
			return true;
		}

		/*
		===============
		LoadItemBase
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="json"></param>
		/// <param name="definition"></param>
		private static ItemDefinition LoadItemBase( JsonElement json, ItemDefinition definition ) {
			return definition with {
				Weight = JsonLoader.TryGet( json, nameof( definition.Weight ), out float weight ) ? weight : 0.0f,
				BaseCost = JsonLoader.TryGet( json, nameof( definition.BaseCost ), out float baseCost ) ? baseCost : 0.0f,
				Name = new InternString( JsonLoader.TryGet( json, nameof( definition.Name ), out string name ) ? name : $"Item#{definition.GetHashCode()}" ),
				JournalEntry = new InternString( JsonLoader.TryGet( json, nameof( definition.JournalEntry ), out string journalEntry ) ? journalEntry : string.Empty )
			};
		}
	};
};
