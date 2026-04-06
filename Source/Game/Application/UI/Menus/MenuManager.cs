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

using Nomad.Game.Application.UI.Menus.Events;
using Nomad.Core.Events;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Engine.Globals;
using Nomad.Core.Engine.Services;
using System;
using System.Collections.Generic;

namespace Nomad.Game.Application.UI.Menus {
	/*
	===================================================================================
	
	MenuManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class MenuManager : IDisposable {
		private MenuState _currentState = MenuState.None;
		private MenuState _previousState = MenuState.None;

		private readonly Dictionary<MenuState, string> _scenePaths = new Dictionary<MenuState, string>() {
			[ MenuState.Main ] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/MainMenu/MainMenu.tscn", StorageScope.Install ),
			[ MenuState.Extras ] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/ExtrasMenu/ExtrasMenu.tscn", StorageScope.Install ),
			[ MenuState.Loading ] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/LoadingScreen/LoadingScreen.tscn", StorageScope.Install ),
			[ MenuState.Settings ] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/SettingsMenu/SettingsMenu.tscn", StorageScope.Install ),
			[ MenuState.NewGame ] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/NewGameMenu/NewGameMenu.tscn", StorageScope.Install )
		};

		private readonly IGameEventRegistryService _eventRegistry;

		private IScene? _currentScene;
		private readonly ISceneManager _sceneManager;
		private readonly ISubscriptionHandle _menuTransitionRequested;

		private bool _isDiposed = false;

		/*
		===============
		MenuManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sceneManager"></param>
		/// <param name="eventFactory"></param>
		public MenuManager( ISceneManager sceneManager, IGameEventRegistryService eventFactory ) {
			_sceneManager = sceneManager;
			_eventRegistry = eventFactory;

			var menuTransitionRequested = eventFactory.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE );
			_menuTransitionRequested = menuTransitionRequested.Subscribe( OnMenuTransitionRequested );

			sceneManager.LoadScene( EngineService.GetStoragePath( "Source/Game/Presentation/Screens/MenuHub/MenuHub.tscn", StorageScope.Install ), LoadSceneMode.Single );
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
			if ( !_isDiposed ) {
				_menuTransitionRequested?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDiposed = true;
		}

		/*
		===============
		TransitionToMenu
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="newState"></param>
		/// <returns></returns>
		public void TransitionToMenu( MenuState newState ) {
			if ( _currentState == newState ) {
				return;
			}

			_previousState = _currentState;

			if ( _currentScene != null ) {
				_sceneManager.UnloadScene( _currentScene );
			}

			_currentScene = _sceneManager.LoadScene( _scenePaths[ newState ], LoadSceneMode.Additive );
			_currentState = newState;

			var menuTransitionCompleted = _eventRegistry.GetEvent<MenuTransitionCompletedEventArgs>( UIConstants.MENU_TRANSITION_COMPLETED_EVENT, UIConstants.NAMESPACE );
			menuTransitionCompleted.Publish( new MenuTransitionCompletedEventArgs( _currentState, _previousState ) );
		}

		/*
		===============
		OnMenuTransitionRequested
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnMenuTransitionRequested( in MenuTransitionRequestedEventArgs args ) {
			TransitionToMenu( args.ToState );
		}
	};
};
