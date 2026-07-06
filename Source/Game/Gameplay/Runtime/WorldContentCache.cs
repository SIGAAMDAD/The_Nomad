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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Core.Logger;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Events.Gameplay;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Gameplay.Items;
using Nomad.Game.Content.Catalogs.Items;
using Nomad.Save.Services;
using Nomad.Game.Content.Catalogs.Biomes;
using Nomad.Game.Sdk.Crafting;

namespace Nomad.Game.Gameplay.Runtime
{
	internal sealed class WorldContentCache : IWorldContentCache
	{
		public IItemCatalog Items => _itemCatalog;
		private readonly ItemCatalog _itemCatalog;

		public IItemInstanceRepository ItemInstances => _itemInstances;
		private readonly ItemInstanceRepository _itemInstances;

		public ICraftingRecipeCatalog Recipes => _recipeCatalog;
		private readonly CraftingRecipeCatalog _recipeCatalog;

		private readonly BiomeCatalog _biomeCatalog;

		private readonly IDisposable _worldBootstrapRequested;
		private readonly IDisposable _worldBootstrapSucceeded;

		private readonly ISaveDataProvider _dataProvider;

		private bool _isDisposed = false;

		/*
		===============
		WorldContentCache
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="dataProvider"></param>
		/// <param name="fileSystem"></param>
		/// <param name="logger"></param>
		/// <param name="eventFactory"></param>
		public WorldContentCache( ISaveDataProvider dataProvider, IFileSystem fileSystem, ILoggerService logger, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_dataProvider = dataProvider ?? throw new ArgumentNullException( nameof( dataProvider ) );

			_itemCatalog = new ItemCatalog( fileSystem, logger );
			_itemCatalog.AddLoader( ItemType.Ammunition, AmmoDefinition.Load );
			_itemCatalog.AddLoader( ItemType.FirearmWeapon, FirearmDefinition.Load );
			_itemCatalog.AddLoader( ItemType.Consumable, ConsumableDefinition.Load );

			_itemInstances = new ItemInstanceRepository( logger, eventFactory, _itemCatalog );

			_recipeCatalog = new CraftingRecipeCatalog( fileSystem, logger );

			_worldBootstrapRequested = eventFactory
				.GetEvent<WorldBootstrapRequestEventArgs>(
					WorldBootstrapRequestEventArgs.Name,
					WorldBootstrapRequestEventArgs.NameSpace
				)
				.Subscribe( OnWorldBootstrapRequested );

			_worldBootstrapSucceeded = eventFactory
				.GetEvent<WorldBootstrapSucceededEventArgs>(
					WorldBootstrapSucceededEventArgs.Name,
					WorldBootstrapSucceededEventArgs.NameSpace
				)
				.Subscribe( OnWorldBootstrapSucceeded );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_itemCatalog.Clear();
			_itemInstances.Clear();
			_recipeCatalog.Clear();

			_worldBootstrapSucceeded.Dispose();
			_worldBootstrapRequested.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnWorldBootstrapSucceeded( in WorldBootstrapSucceededEventArgs args )
		{
			if ( args.Mode == WorldBootstrapMode.SinglePlayerLoadGame ) {
				_dataProvider
					.Load( args.SaveName )
					.ConfigureAwait( false );
			}
		}

		private void OnWorldBootstrapRequested( in WorldBootstrapRequestEventArgs args )
		{
			_itemCatalog.ScanAndLoad();
		}
	};
};
