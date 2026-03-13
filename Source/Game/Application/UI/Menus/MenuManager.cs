/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Game.Application.UI.Menus.Events;
using Nomad.Core.EngineUtils;
using Nomad.Core.Events;
using Nomad.Core.EngineUtils.Globals;
using System;
using System.Collections.Generic;

namespace Game.Application.UI.Menus {
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
			[ MenuState.Loading ] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/LoadingScreen/LoadingScreen.tscn", StorageScope.Install ),
			[ MenuState.Settings ] = EngineService.GetStoragePath( "Source/Game/Presentation/Screens/SettingsMenu/SettingsMenu.tscn", StorageScope.Install )
		};

		private IGameEventRegistryService _eventRegistry;

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

			_currentScene = _sceneManager.LoadScene( _scenePaths[ newState ], LoadSceneMode.Single );
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
