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

using Nomad.Core.Engine.Globals;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.Input;
using Nomad.Core.Input.ValueObjects;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Events.Globals;
using Nomad.Game.Application.UI;
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Application.UI.Menus.Events;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.PauseMenu {
	/*
	===================================================================================
	
	PauseMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class PauseMenu : EnginePresentationLayer {
		private IGameStateService _gameStateService;
		private IGamePauseService _pauseService;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			DisplayStateChanged.Subscribe( OnDisplayStateChanged );

			FindChild<EngineButton>( "OptionsContainer/ResumeGameButton" ).Clicked.Subscribe( OnResumeGameButtonPressed );
			FindChild<EngineButton>( "OptionsContainer/SettingsButton" ).Clicked.Subscribe( OnSettingsButtonPressed );
			FindChild<EngineButton>( "OptionsContainer/ExitWorldButton" ).Clicked.Subscribe( OnExitWorldButtonPressed );
			FindChild<EngineButton>( "OptionsContainer/QuitGameButton" ).Clicked.Subscribe( OnQuitGameButtonPressed );

			var serviceLocator = ServiceLocator.Instance;
			_gameStateService = serviceLocator.GetService<IGameStateService>();
			_pauseService = serviceLocator.GetService<IGamePauseService>();

			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();
			eventFactory
				.GetEvent<KeyboardEventArgs>( Core.Constants.Events.Input.KEYBOARD_EVENT, Core.Constants.Events.Input.NAMESPACE )
				.Subscribe( OnKeyboardEvent );
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

			var eventFactory = ServiceLocator.GetService<IGameEventRegistryService>();
			eventFactory
				.GetEvent<KeyboardEventArgs>( Core.Constants.Events.Input.KEYBOARD_EVENT, Core.Constants.Events.Input.NAMESPACE )
				.Unsubscribe( OnKeyboardEvent );
		}

		/*
		===============
		OnDisplayStateChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="displayState"></param>
		private void OnDisplayStateChanged( in bool displayState ) {
			if ( !displayState ) {
				_gameStateService.SetState( GameState.Level );
			} else {
				_gameStateService.SetState( GameState.Paused );
			}
			_pauseService.SetPaused( displayState );
		}
		
		/*
		===============
		OnQuitGameButtonPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private static void OnQuitGameButtonPressed( in EmptyEventArgs args ) {
			EngineService.Quit();
		}

		/*
		===============
		OnExitWorldButtonPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnExitWorldButtonPressed( in EmptyEventArgs args ) {
			_pauseService.SetPaused( false );
			SceneManager.LoadScene( EngineService.GetStoragePath( "Source/Game/Presentation/Screens/MenuHub/MenuHub.tscn", Core.Engine.Services.StorageScope.Install ) );
		}

		/*
		===============
		OnSettingsButtonPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private static void OnSettingsButtonPressed( in EmptyEventArgs args ) {
			GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.Pause, MenuState.Settings ) );
		}

		/*
		===============
		OnResumeGameButtonPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnResumeGameButtonPressed( in EmptyEventArgs args ) {
			Visible = false;
		}

		/*
		===============
		OnKeyboardEvent
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnKeyboardEvent( in KeyboardEventArgs args ) {
			if ( args.KeyNum == KeyNum.Escape && args.Pressed ) {
				Visible = !Visible;
			}
		}
	};
};
