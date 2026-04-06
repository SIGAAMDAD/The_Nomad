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
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.MultiplayerMenu {
	/*
	===================================================================================
	
	MultiplayerMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class MultiplayerMenu : EnginePanel {
		private ISubscriptionHandle _backButtonClicked;

		protected override void OnInit() {
			base.OnInit();

			var backButton = FindChild<EngineButton>( "OptionsContainer/BackButton" );
			_backButtonClicked = backButton.Clicked.Subscribe( OnBackButtonClicked );
		}

		protected override void OnShutdown() {
			base.OnShutdown();

			_backButtonClicked?.Dispose();
		}

		private void OnBackButtonClicked( in EmptyEventArgs args ) {
			Visible = false;
		}
	};
};
