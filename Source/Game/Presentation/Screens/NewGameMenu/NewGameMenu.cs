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
		private EngineVerticalContainer _optionsContainer;
		private EnginePanel _customDifficultyContainer;

		private ISubscriptionGroup _eventGroup;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			_eventGroup = GameEventRegistry.GetGroup( nameof( NewGameMenu ) );

			_optionsContainer = FindChild<EngineVerticalContainer>( "OptionsContainer" );
			_customDifficultyContainer = FindChild<EnginePanel>( "CustomDifficultyContainer" );

			_eventGroup.Add( _optionsContainer.DisplayStateChanged, OnOptionsContainerDisplayStateChanged );
			_eventGroup.Add( _customDifficultyContainer.DisplayStateChanged, OnCustomContainerDisplayStateChanged );
		}

		/*
		===============
		OnOptionsContainerDisplayStateChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnOptionsContainerDisplayStateChanged( in bool args ) {
			_customDifficultyContainer.Visible = !args;
		}

		/*
		===============
		OnCustomContainerDisplayStateChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnCustomContainerDisplayStateChanged( in bool args ) {
			_optionsContainer.Visible = !args;
		}
	};
};
