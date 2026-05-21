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
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Domain.Interfaces.Items;
using Nomad.Game.Infrastructure.Gameplay.Items;

namespace Nomad.Game.Infrastructure.Gameplay
{
	internal sealed class WorldContentCache : IWorldContentCache
	{
		public IItemCatalog Items => _itemCatalog;
		private readonly ItemCatalog _itemCatalog;

		private readonly IDisposable _worldBootstrapSucceeded;

		private bool _isDisposed = false;

		/*
		===============
		WorldContentCache
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fileSystem"></param>
		/// <param name="logger"></param>
		/// <param name="eventFactory"></param>
		public WorldContentCache( IFileSystem fileSystem, ILoggerService logger, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_itemCatalog = new ItemCatalog( fileSystem, logger );
			_itemCatalog.AddLoader( ItemType.Ammunition, AmmoDefinition.Load );
			_itemCatalog.AddLoader( ItemType.FirearmWeapon, FirearmDefinition.Load );

			_worldBootstrapSucceeded = eventFactory
				.GetEvent<WorldBootstrapRequestEventArgs>(
					WorldBootstrapRequestEventArgs.Name,
					WorldBootstrapRequestEventArgs.NameSpace
				)
				.Subscribe( OnWorldBootstrapFinished );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_itemCatalog.Clear();

			_worldBootstrapSucceeded.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnWorldBootstrapFinished( in WorldBootstrapRequestEventArgs args )
		{
			_itemCatalog.ScanAndLoad();
		}
	};
};
