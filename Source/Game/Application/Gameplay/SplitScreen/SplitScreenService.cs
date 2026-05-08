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

using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.Scene.GameObjects;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Domain.Events.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nomad.Game.Application.Gameplay.SplitScreen
{
	internal sealed class SplitScreenService : IDisposable
	{
		private readonly IGameObject _worldRoot;
		private readonly ISubscriptionHandle _spawnResultsReady;
		private readonly ISplitScreenService _splitScreenService;

		private bool _isDisposed = false;

		public SplitScreenService( IGameEventRegistryService eventFactory, IServiceLocator serviceLocator, IGameObject worldRoot )
		{
			_worldRoot = worldRoot ?? throw new ArgumentNullException( nameof( worldRoot ) );
			_splitScreenService = serviceLocator.GetService<ISplitScreenService>();

			_spawnResultsReady = eventFactory
				.GetEvent<PlayerSpawnResultEventArgs>( PlayerSpawnResultEventArgs.Name, PlayerSpawnResultEventArgs.NameSpace )
				.Subscribe( OnPlayerSpawned );

			Refresh();
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_spawnResultsReady?.Dispose();
			_splitScreenService.Clear();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnPlayerSpawned( in PlayerSpawnResultEventArgs args )
		{
			if ( !args.Success ) {
				return;
			}

			Refresh();
		}

		private void Refresh()
		{
			var players = GetPlayers();
			_splitScreenService.Apply( _worldRoot, players );
		}

		private IReadOnlyList<IGameObject> GetPlayers()
		{
			return _worldRoot
				.Children
				.Where( child => child.FindChild<IGameObject>( "Camera2D" ) != null )
				.Take( 4 )
				.ToArray();
		}
	}
}
