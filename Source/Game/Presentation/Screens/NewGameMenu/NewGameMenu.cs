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

namespace Nomad.Game.Presentation.Screens.NewGameMenu {
	/*
	===================================================================================
	
	NewGameMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class NewGameMenu : EnginePanel {
		private EnginePanel _optionsContainer;

		private EnginePanel _customDifficultyContainer;
		private EnginePanel _customDifficultyButtonContainer;

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
			_buttonGroup = GameEventRegistry.GetGroup( nameof( NewGameMenu ) );

			_optionsContainer = FindChild<EnginePanel>( "PaddingContainer/OptionsContainer" );

			_buttonGroup.Add( _optionsContainer.FindChild<EngineButton>( "EasyDifficultyButton" ).Clicked, OnEasyDifficultySelected );
			_buttonGroup.Add( _optionsContainer.FindChild<EngineButton>( "HardDifficultyButton" ).Clicked, OnHardDifficultySelected );
			_buttonGroup.Add( _optionsContainer.FindChild<EngineButton>( "CustomDifficultyButton" ).Clicked, OnCustomDifficultySelected );
			_buttonGroup.Add( _optionsContainer.FindChild<EngineButton>( "BackButton" ).Clicked, OnBackButtonPressed );

			_customDifficultyContainer = FindChild<EnginePanel>( "PaddingContainer/CustomOptionsContainer" );
			_customDifficultyButtonContainer = FindChild<EnginePanel>( "CustomButtonContainer" );

			_buttonGroup.Add( _customDifficultyButtonContainer.FindChild<EngineButton>( "BackButton" ).Clicked, OnBackButtonCustomPressed );
		}

		/*
		===============
		OnCustomDifficultySelected
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnCustomDifficultySelected( in EmptyEventArgs args ) {
			_optionsContainer.Visible = false;
			_customDifficultyContainer.Visible = true;
			_customDifficultyButtonContainer.Visible = true;
		}

		/*
		===============
		OnEasyDifficultySelected
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnEasyDifficultySelected( in EmptyEventArgs args ) {
		}

		/*
		===============
		OnHardDifficultySelected
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnHardDifficultySelected( in EmptyEventArgs args ) {
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
			GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.NewGame, MenuState.Main ) );
		}

		/*
		===============
		OnBackButtonCustomPressed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnBackButtonCustomPressed( in EmptyEventArgs args ) {
			_optionsContainer.Visible = true;
			_customDifficultyContainer.Visible = false;
			_customDifficultyButtonContainer.Visible = false;
		}
	};
};
