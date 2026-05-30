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
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Events.Gameplay;
using Nomad.Game.Sdk.Interactables;
using Nomad.Core.Logger;
using Nomad.Logger.Extensions;

namespace Nomad.Game.Application.Gameplay
{
	internal sealed class GameplayApplicationCoordinator : IDisposable
	{
		private readonly IGameEventRegistryService _eventFactory;
		private readonly IServiceLocator _services;
		private readonly ILoggerCategory _category;

		private InteractableApplicationCoordinator? _interactableCoordinator;
		private InventoryApplicationCoordinator? _inventoryCoordinator;
		private ItemSpawner? _itemSpawner;

		private readonly IDisposable _gameStateChanged;

		private bool _isDisposed = false;

		public GameplayApplicationCoordinator( IGameEventRegistryService eventFactory, IServiceLocator services )
		{
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_services = services ?? throw new ArgumentNullException( nameof( services ) );

			var logger = _services.GetService<ILoggerService>();
			_category = logger.For<GameplayApplicationCoordinator>();

			_gameStateChanged = eventFactory
				.GetEvent<GameStateChangedEventArgs>(
					GameStateChangedEventArgs.Name,
					GameStateChangedEventArgs.NameSpace
				)
				.Subscribe( OnGameStateChanged );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_gameStateChanged.Dispose();
			_category.Dispose();
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

		private void CreateLevelServices()
		{
			if ( _interactableCoordinator != null || _inventoryCoordinator != null || _itemSpawner != null ) {
				return;
			}

			if ( !_services.TryGetService<IPlayerRuntimeRegistry>( out IPlayerRuntimeRegistry? players ) ) {
				return;
			}

			_category.PrintLine( $"Initializing level services...." );

			var sceneManager = _services.GetService<ISceneManager>();
			var worldContent = _services.GetService<IWorldContentCache>();
			var behaviorRegistry = _services.GetService<INomadBehaviorRegistry>();

			_interactableCoordinator = new InteractableApplicationCoordinator( players, _eventFactory );
			_services.Collection.AddSingleton<ICheckpointService>( _interactableCoordinator.CheckpointService );

			_inventoryCoordinator = new InventoryApplicationCoordinator( players, _eventFactory );
			_itemSpawner = new ItemSpawner( sceneManager, worldContent.Items, behaviorRegistry, _eventFactory );
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
