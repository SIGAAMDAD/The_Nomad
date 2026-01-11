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
using Game.Infrastructure;
using Game.Infrastructure.Caching;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Application.UI.Menus {
	/*
	===================================================================================
	
	MenuManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class MenuManager( Node worldNode ) : Node {
		private MenuState _currentState = MenuState.None;
		private MenuState _previousState = MenuState.None;

		private readonly Dictionary<MenuState, FilePath> _scenePaths = new Dictionary<MenuState, FilePath>() {
			[MenuState.Main] = new( "res://Source/Game/Presentation/Screens/MainMenu/MainMenu.tscn", PathType.Resource ),
			[MenuState.Loading] = new( "res://Source/Game/Presentation/Screens/LoadingScreen/LoadingScreen.tscn", PathType.Resource ),
			[MenuState.Settings] = new( "res://Source/Game/Presentation/Screens/SettingsMenu/SettingsMenu.tscn", PathType.Resource )
		};

		private Control _currentMenuInstance;

		private IGameEventRegistryService _eventRegistry;

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
		public async Task TransitionToMenu( MenuState newState ) {
			if ( _currentState == newState ) {
				return;
			}

			_previousState = _currentState;

			if ( _currentMenuInstance != null ) {
				SceneCache.Instance.ReleaseReference( _scenePaths[ _previousState ] );
				worldNode.CallDeferred( Node.MethodName.RemoveChild, _currentMenuInstance );
				_currentMenuInstance.CallDeferred( Control.MethodName.QueueFree );
			}

			var entry = await SceneCache.Instance.GetCachedAsync( _scenePaths[ newState ] );
			entry.Get( out var menu );

			_currentMenuInstance = menu.Instantiate<Control>();
			_currentState = newState;
			worldNode.CallDeferred( Node.MethodName.AddChild, _currentMenuInstance );

			UIEventHelper.PublishUIEvent( _eventRegistry, UIConstants.MENU_TRANSITION_COMPLETED_EVENT, new MenuTransitionCompletedEventArgs( _currentState, _previousState ) );
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

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			_eventRegistry = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.SubscribeToUIEvent<MenuTransitionRequestedEventArgs>( _eventRegistry, this, UIConstants.MENU_TRANSITION_REQUESTED_EVENT, OnMenuTransitionRequested );

			var logger = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<ILoggerService>();
		}
	};
};