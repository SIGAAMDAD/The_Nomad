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
using Nomad.Game.Infrastructure.StateManagement;
using Nomad.Game.Domain.Events.StateManagement;
using Nomad.Game.Domain.Data.StateManagement;
using Nomad.Scene.GameObjects;
using Nomad.UI;

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
			
			_menuManager = new MenuManager( ServiceLocator.GetService<ISceneManager>(), ServiceLocator.GetService<IGameEventRegistryService>() );
			_onGameStateChanged = GameStateManager.StateChanged.Subscribe( OnGameStateChanged );
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
