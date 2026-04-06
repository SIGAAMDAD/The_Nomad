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

using Nomad.Game.Application.UI;
using Nomad.Game.Application.UI.Menus.Events;
using Nomad.Audio.Interfaces;
using Nomad.Core.Engine.Globals;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Events.Globals;
using Nomad.UI;
using Nomad.Game.Application.UI.Menus;

namespace Nomad.Game.Presentation.Screens.MainMenu {
	/*
	===================================================================================
	
	MainMenu
	
	===================================================================================
	*/
	/// <summary>
	/// Handles the main menu's creation.
	/// </summary>
	
	public sealed partial class MainMenu : EnginePanel {
		private ISubscriptionGroup _buttonGroup;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			var audioDevice = ServiceLocator.GetService<IAudioDevice>();
			audioDevice.LoadBank( EngineService.GetStoragePath( "Audio/Banks/Desktop/ui.bank", StorageScope.StreamingAssets ) );
			audioDevice.LoadBank( EngineService.GetStoragePath( "Audio/Banks/Desktop/music.bank", StorageScope.StreamingAssets ) );

			var musicService = ServiceLocator.GetService<IMusicService>();
			musicService.PlayTheme( "event:/Music/UserInterface/MainMenuTheme" );

			_buttonGroup = GameEventRegistry.GetGroup( "MainMenu" );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/QuitGameButton" ).Clicked, OnQuitGameClicked );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/NewGameButton" ).Clicked, OnNewGameButtonClicked );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/ExtrasButton" ).Clicked, OnExtrasButtonClicked );
			_buttonGroup.Add( FindChild<EngineButton>( "OptionsContainer/SettingsButton" ).Clicked, OnSettingsMenuButtonClicked );
		}

		protected override void OnShutdown() {
			base.OnShutdown();

			_buttonGroup?.Dispose();
		}

		/*
		===============
		OnExtrasButtonClicked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnExtrasButtonClicked( in EmptyEventArgs args ) {
			GameEventRegistry
				.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Extras ) );
		}

		/*
		===============
		OnNewGameButtonClicked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnNewGameButtonClicked( in EmptyEventArgs args ) {
			GameEventRegistry
				.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.NewGame ) );
		}

		/*
		===============
		OnSettingsMenuButtonClicked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnSettingsMenuButtonClicked( in EmptyEventArgs args ) {
			GameEventRegistry
				.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Main, MenuState.Settings ) );
		}

		/*
		===============
		OnQuitGameClicked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnQuitGameClicked( in EmptyEventArgs args ) {
			EngineService.Quit();
		}
	};
};
