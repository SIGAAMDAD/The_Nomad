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
using Nomad.Core.Events;
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
	
	public partial class PauseMenu : EnginePresentationLayer {
		private ISubscriptionGroup _buttonGroup;

		private IGameStateService _gameStateService;

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

			_buttonGroup = GameEventRegistry.GetGroup( nameof( PauseMenu ) );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/ResumeGameButton" ).Clicked, OnResumeGameButtonPressed );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/SettingsButton" ).Clicked, OnSettingsButtonPressed );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/ExitWorldButton" ).Clicked, OnExitWorldButtonPressed );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/QuitGameButton" ).Clicked, OnQuitGameButtonPressed );

			_gameStateService = ServiceLocator.GetService<IGameStateService>();
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
			Enabled = displayState;
			if ( !Enabled ) {
				_gameStateService.SetState( GameState.Level );
			}
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
		private void OnQuitGameButtonPressed( in EmptyEventArgs args ) {
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
		private void OnSettingsButtonPressed( in EmptyEventArgs args ) {
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
	};
};
