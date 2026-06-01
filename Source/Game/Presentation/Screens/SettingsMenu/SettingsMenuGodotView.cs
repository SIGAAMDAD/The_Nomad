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
using Godot;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================

	SettingsMenuGodotView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class SettingsMenuGodotView : Control, ISettingsMenuView
	{
		public AudioSettingsContainerView AudioView { get; private set; }
		public DisplaySettingsContainerView DisplayView { get; private set; }

		private SettingsMenuPresenter _presenter;

		public event Action SaveRequested;
		public event Action BackRequested;
		public event Action ResetRequested;

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready()
		{
			base._Ready();

			AudioView = GetNode<AudioSettingsContainerView>( "TabContainer/Audio" );
			DisplayView = GetNode<DisplaySettingsContainerView>( "TabContainer/Display" );

			_presenter = ScreenPresenterFactory.CreateSettingsMenuPresenter( this );

			GetNode<Button>( "BottomContainer/ButtonContainer/BackButton" ).Pressed += () => BackRequested?.Invoke();
			GetNode<Button>( "BottomContainer/ButtonContainer/SaveButton" ).Pressed += () => SaveRequested?.Invoke();
			GetNode<Button>( "BottomContainer/ButtonContainer/ResetButton" ).Pressed += () => ResetRequested?.Invoke();
		}

		/*
		===============
		_ExitTree
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _ExitTree()
		{
			base._ExitTree();

			_presenter?.Dispose();
			_presenter = null;
		}
	};
};
