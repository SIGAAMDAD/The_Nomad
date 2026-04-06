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
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.UI;
using Nomad.Game.Application.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Events.Globals;
using Nomad.CVars.Global;
using Nomad.Logger.Globals;
using Nomad.Game.Application.Configuration.Registries;

namespace Nomad.Game.Application {
	/*
	===================================================================================
	
	ApplicationBootstrapper
	
	===================================================================================
	*/
	/// <summary>
	/// Initializes the Application layer.
	/// </summary>
	
	public sealed partial class ApplicationBootstrapper : EngineAspectRatioContainer {
		private MenuManager _menuManager;
		private IGameFlowCoordinator _gameFlowCoordinator;
		private ISubscriptionHandle _onGameStateChanged;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			base.OnInit();

			var eventFactory = GameEventRegistry.Instance;
			var gameStateManager = new GameStateManager( eventFactory );
			var cvarSystem = CVarSystem.Instance;
			var sceneManager = ServiceLocator.GetService<ISceneManager>();
			
			GameplayCVars.Register( cvarSystem );

			_menuManager = new MenuManager( sceneManager, eventFactory );
			_gameFlowCoordinator = new GameFlowCoordinator( eventFactory, gameStateManager, cvarSystem, Logging.Instance, sceneManager );
			_onGameStateChanged = gameStateManager.StateChanged.Subscribe( OnGameStateChanged );
			
			ServiceRegistry.AddSingleton<IGameStateService>( gameStateManager );
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
		protected override void OnShutdown() {
			base.OnShutdown();

			_onGameStateChanged?.Dispose();
			_menuManager?.Dispose();
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
				_menuManager = new MenuManager( ServiceLocator.GetService<ISceneManager>(), ServiceLocator.GetService<IGameEventRegistryService>() );
			} else {
				_menuManager?.Dispose();
				_menuManager = null;
			}
		}
	};
};
