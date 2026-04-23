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

using System;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenuView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed partial class SettingsMenuView : EnginePanel {
		public AudioSettingsContainerView AudioView { get; private set; }
		public DisplaySettingsContainerView DisplayView { get; private set; }

		private SettingsMenuPresenter _presenter;

		public event Action SaveRequested;
		public event Action BackRequested;
		public event Action ResetRequested;

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

			AudioView = FindChild<AudioSettingsContainerView>( "TabContainer/Audio" );
			DisplayView = FindChild<DisplaySettingsContainerView>( "TabContainer/Display" );

			_presenter = SettingsMenuFactory.Create( this );

			FindChild<EngineButton>( "BottomContainer/ButtonContainer/BackButton" ).Clicked.Subscribe( ( in args ) => BackRequested?.Invoke() );
			FindChild<EngineButton>( "BottomContainer/ButtonContainer/SaveButton" ).Clicked.Subscribe( ( in args ) => SaveRequested?.Invoke() );
			FindChild<EngineButton>( "BottomContainer/ButtonContainer/ResetButton" ).Clicked.Subscribe( ( in args ) => ResetRequested?.Invoke() );
		}
	};
};