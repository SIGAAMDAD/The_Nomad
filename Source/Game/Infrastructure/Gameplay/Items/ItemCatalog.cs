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
using Nomad.Core.Logger;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Infrastructure.Gameplay.Items
{
	/*
	===================================================================================

	ItemCatalog

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class ItemCatalog : DataDefinitionRegistry<ItemDefinitionId, ItemDefinition>, IItemCatalog
	{
		protected override Func<string, ItemDefinitionId> KeyFactory => k => new ItemDefinitionId( new InternString( k ) );
		protected override string LoggerCategoryName => nameof( ItemCatalog );

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
		public ItemCatalog( IFileSystem fileSystem, ILoggerService logger )
			: base( fileSystem, logger )
		{
		}

		public void ScanAndLoad()
		{
			ScanDirectory( "Assets/Items", "*.json" );
		}

		/*
		===============
		Get
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TItemDefinition"></typeparam>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public TItemDefinition? Get<TItemDefinition>( ItemDefinitionId itemId )
			where TItemDefinition : ItemDefinition
		{
			if ( TryGet( itemId, out ItemDefinition? definition ) ) {
				return (TItemDefinition?)definition;
			}

			foreach ( ItemDefinition item in dataCache.Values ) {
				if ( item.Id.Equals( itemId ) ) {
					return (TItemDefinition?)item;
				}
			}

			return null;
		}

		/*
		===============
		TryGet
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TItemDefinition"></typeparam>
		/// <param name="itemId"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool TryGet<TItemDefinition>( ItemDefinitionId itemId, out TItemDefinition? item )
			where TItemDefinition : ItemDefinition
		{
			if ( TryGet( itemId, out var data ) ) {
				item = (TItemDefinition?)data;
				return true;
			}

			foreach ( ItemDefinition definition in dataCache.Values ) {
				if ( definition.Id.Equals( itemId ) ) {
					item = (TItemDefinition?)definition;
					return true;
				}
			}

			item = null;
			return false;
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
		public void AddLoader( ItemType type, Func<JsonElement, ItemDefinition> callback )
		{
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
		protected override bool TryLoadDefinition( JsonElement json, out ItemDefinition definition )
		{
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
		private static ItemDefinition LoadItemBase( JsonElement json, ItemDefinition definition )
		{
			return definition with {
				Id = new ItemDefinitionId( new InternString( JsonLoader.TryGet( json, nameof( definition.Id ), out string id ) ? id : string.Empty ) ),
				Weight = JsonLoader.TryGet( json, nameof( definition.Weight ), out float weight ) ? weight : 0.0f,
				BaseCost = JsonLoader.TryGet( json, nameof( definition.BaseCost ), out float baseCost ) ? baseCost : 0.0f,
				Name = new InternString( JsonLoader.TryGet( json, nameof( definition.Name ), out string name ) ? name : $"Item#{definition.GetHashCode()}" ),
				JournalEntry = new InternString( JsonLoader.TryGet( json, nameof( definition.JournalEntry ), out string journalEntry ) ? journalEntry : string.Empty )
			};
		}
	};
};
