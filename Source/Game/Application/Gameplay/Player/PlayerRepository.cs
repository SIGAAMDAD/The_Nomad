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
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Core.ServiceRegistry.Services;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerRepository
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerRepository : IDisposable {
		private readonly ConcurrentDictionary<Guid, PlayerBase> _players = new();
		private readonly string _playerPrefab;

		private readonly ISceneManager _sceneManager;
		private readonly IGameEventRegistryService _eventFactory;
		private readonly IServiceRegistry _registry;
		private readonly ILoggerService _logger;
		
		private readonly ISubscriptionHandle _onGameStateChanged;

		private bool _isDisposed = false;

		/*
		===============
		PlayerRepository
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="registry"></param>
		/// <param name="logger"></param>
		/// <param name="gameStateService"></param>
		/// <param name="sceneManager"></param>
		/// <param name="playerPrefab"></param>
		public PlayerRepository( IGameEventRegistryService eventFactory, IServiceRegistry registry, ILoggerService logger, IGameStateService gameStateService, ISceneManager sceneManager, string playerPrefab ) {
			_onGameStateChanged = gameStateService.StateChanged.Subscribe( OnGameStateChanged );
			_playerPrefab = playerPrefab;
			_sceneManager = sceneManager ?? throw new ArgumentNullException( nameof( sceneManager ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_registry = registry ?? throw new ArgumentNullException( nameof( registry ) );
			_logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			if ( !_isDisposed ) {
				_onGameStateChanged?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		CreatePlayer
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <returns></returns>
		public PlayerBase CreatePlayer( in PlayerSpawnRequestedEventArgs args ) {
			var composite = _sceneManager.LoadPrefab( _playerPrefab );

			_logger.PrintLine( $"Adding player to active scene '{_sceneManager.ActiveScene.Name}'" );
			_sceneManager.ActiveScene.Root.AddChild( composite.Root );

			var guid = Guid.NewGuid();
			var playerBase = new PlayerAggregate( guid, composite.Root.CastAs<PlayerPrefab>(), _registry, _eventFactory, _logger );

			_players[ guid ] = playerBase;
			return playerBase;
		}
		
		/*
		===============
		OnGameStateChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnGameStateChanged( in GameStateChangedEventArgs args ) {
			if ( args.CurrentState == GameState.Menu ) {
				_players.Clear();
			}
		}
	};
};