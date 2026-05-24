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
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Application.Gameplay.Inventory;
using Nomad.Game.Application.Gameplay.Interactables;
using Nomad.Game.Application.Gameplay.Items;
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Domain.Interfaces.Interactables;
using Nomad.Game.Domain.Interfaces.Items;

namespace Nomad.Game.Application.Gameplay
{
	internal sealed class GameplayApplicationCoordinator : IDisposable
	{
		private readonly IGameEventRegistryService _eventFactory;
		private readonly IServiceLocator _services;

		private InteractableApplicationCoordinator? _interactableCoordinator;
		private InventoryApplicationCoordinator? _inventoryCoordinator;
		private IItemSpawnService? _itemSpawner;

		private readonly IDisposable _gameStateChanged;
		private readonly IDisposable _playerSpawned;

		private bool _isDisposed = false;

		public GameplayApplicationCoordinator( IGameEventRegistryService eventFactory, IServiceLocator services )
		{
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_services = services ?? throw new ArgumentNullException( nameof( services ) );

			_gameStateChanged = eventFactory
				.GetEvent<GameStateChangedEventArgs>(
					GameStateChangedEventArgs.Name,
					GameStateChangedEventArgs.NameSpace
				)
				.Subscribe( OnGameStateChanged );

			_playerSpawned = eventFactory
				.GetEvent<PlayerSpawnResultEventArgs>(
					PlayerSpawnResultEventArgs.Name,
					PlayerSpawnResultEventArgs.NameSpace
				)
				.Subscribe( OnPlayerSpawned );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_gameStateChanged.Dispose();
			_playerSpawned.Dispose();
			DisposeLevelServices();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnGameStateChanged( in GameStateChangedEventArgs args )
		{
			if ( args.CurrentState == GameState.Level ) {
				CreateLevelServices();
				return;
			}

			if ( args.PrevState == GameState.Level ) {
				DisposeLevelServices();
			}
		}

		private void OnPlayerSpawned( in PlayerSpawnResultEventArgs args )
		{
			if ( args.Success ) {
				CreateLevelServices();
			}
		}

		private void CreateLevelServices()
		{
			if ( _interactableCoordinator != null || _inventoryCoordinator != null || _itemSpawner != null ) {
				return;
			}

			if ( !_services.TryGetService<IPlayerRuntimeRegistry>( out IPlayerRuntimeRegistry? players ) ) {
				return;
			}

			var sceneManager = _services.GetService<ISceneManager>();
			var worldContent = _services.GetService<IWorldContentCache>();

			_interactableCoordinator = new InteractableApplicationCoordinator( players, _eventFactory );
			_services.Collection.AddSingleton<ICheckpointService>( _interactableCoordinator.CheckpointService );
			_inventoryCoordinator = new InventoryApplicationCoordinator( players, _eventFactory );
			_itemSpawner = new ItemSpawner( sceneManager, worldContent.Items, _eventFactory );
		}

		private void DisposeLevelServices()
		{
			_interactableCoordinator?.Dispose();
			_inventoryCoordinator?.Dispose();
			_itemSpawner?.Dispose();

			_interactableCoordinator = null;
			_inventoryCoordinator = null;
			_itemSpawner = null;
		}
	};
};
