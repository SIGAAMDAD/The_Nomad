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

using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.Game.Application.UI;
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Application.UI.Menus.Events;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.ExtrasMenu {
	/*
	===================================================================================
	
	ExtrasMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class ExtrasMenu : EnginePanel {
		private EnginePanel _multiplayerMenu;
		private EnginePanel _developerCommentaryMenu;

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

			_multiplayerMenu = FindChild<EnginePanel>( "MultiplayerMenu" );
			_developerCommentaryMenu = FindChild<EnginePanel>( "DeveloperCommentaryMenu" );

			FindChild<EngineButton>( "ButtonContainer/DeveloperCommentaryButton" ).Clicked.Subscribe( OnDeveloperCommentaryButtonPressed );
			_developerCommentaryMenu.DisplayStateChanged.Subscribe( OnDeveloperMenuDisplayStateChanged );
			
			FindChild<EngineButton>( "ButtonContainer/MultiplayerButton" ).Clicked.Subscribe( OnMultiplayerButtonPressed );
			_multiplayerMenu.DisplayStateChanged.Subscribe( OnMultiplayerMenuDisplayStateChanged );

			FindChild<EngineButton>( "ButtonContainer/BackButton" ).Clicked.Subscribe( OnBackButtonPressed );
		}

		/*
		===============
		OnDeveloperMenuDisplayStateChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnDeveloperMenuDisplayStateChanged( in bool args ) {
			_multiplayerMenu.Visible = !args;
		}

		/*
		===============
		OnMultiplayerMenuDisplayStateChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnMultiplayerMenuDisplayStateChanged( in bool args ) {
			_developerCommentaryMenu.Visible = !args;
		}

		/*
		===============
		OnDeveloperCommentaryButtonPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnDeveloperCommentaryButtonPressed( in EmptyEventArgs args ) {
			_developerCommentaryMenu.Visible = true;
		}
		
		/*
		===============
		OnMultiplayerButtonPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnMultiplayerButtonPressed( in EmptyEventArgs args ) {
			_multiplayerMenu.Visible = true;
		}
		
		/*
		===============
		OnBackButtonPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnBackButtonPressed( in EmptyEventArgs args ) {
			GameEventRegistry
				.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Extras, MenuState.Main ) );
		}
	};
};
