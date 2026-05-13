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

using Nomad.Game.Application.UI.Menus;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.UI;
using Nomad.Game.Application.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Logger.Globals;
using Nomad.Game.Application.Configuration.Registries;
using Nomad.Game.Infrastructure;
using Nomad.Game.Application.Gameplay.World;
using Nomad.Save.Services;
using Nomad.Game.Application.Gameplay.Persistence;
using Nomad.Core.Logger;
using Nomad.Game.Application.Multiplayer;
using Nomad.Core.Events;
using Nomad.Core.CVars;

namespace Nomad.Game.Application
{
	/*
	===================================================================================

	ApplicationBootstrapper

	===================================================================================
	*/
	/// <summary>
	/// Initializes the Application layer.
	/// </summary>

	public sealed partial class ApplicationBootstrapper : EngineAspectRatioContainer
	{
		private MenuManager _menuManager;
		private IGameFlowCoordinator _gameFlowCoordinator;
		private IWorldLoader _worldLoader;
		private ISceneManager _sceneManager;
		private IGameStateService _gameStateService;
		private SaveGameController _saveController;
		private MultiplayerCoordinator _multiplayerCoordinator;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void OnInit()
		{
			base.OnInit();

			var locator = ServiceLocator.Instance;
			var eventFactory = locator.GetService<IGameEventRegistryService>();
			var cvarSystem = locator.GetService<ICVarSystemService>();
			var logger = locator.GetService<ILoggerService>();

			GameplayCVars.Register( cvarSystem );

			_sceneManager = locator.GetService<ISceneManager>();
			_gameStateService = new GameStateManager( eventFactory );
			_menuManager = new MenuManager( _sceneManager, _gameStateService, eventFactory, logger );
			_worldLoader = new SceneWorldLoader( _sceneManager );
			_saveController = new SaveGameController( ServiceLocator.GetService<ISaveDataProvider>(), eventFactory );

			var worldBootstrapper = new WorldBootstrapper( eventFactory, _worldLoader );
			_gameFlowCoordinator = new GameFlowCoordinator( eventFactory, _gameStateService, cvarSystem, Logging.Instance );

			_multiplayerCoordinator = MultiplayerBootstrapper.Initialize( ServiceRegistry.Instance, ServiceLocator.Instance );

			ServiceRegistry.AddSingleton( _gameStateService );
			ServiceRegistry.AddSingleton( _worldLoader );
			ServiceRegistry.AddSingleton( _gameFlowCoordinator );
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void OnShutdown()
		{
			base.OnShutdown();

			_gameFlowCoordinator?.Dispose();
			_menuManager?.Dispose();
			_saveController?.Dispose();
			_gameStateService?.Dispose();
		}
	};
};
