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

using Godot;

namespace Nomad.Game.Presentation.Screens.NewGameMenu
{
	/*
	===================================================================================

	NewGameMenuView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class NewGameMenuView : Control
	{
		private VBoxContainer _optionsContainer;
		private Control _customDifficultyContainer;

		public void SetOptionsContainerVisibility( bool visible )
		{
			_optionsContainer.Visible = visible;
		}

		public void SetCustomDifficultyContainerVisibility( bool visible )
		{
			_customDifficultyContainer.Visible = visible;
		}

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

			_optionsContainer = GetNode<VBoxContainer>( "OptionsContainer" );
			_customDifficultyContainer = GetNode<Control>( "CustomDifficultyContainer" );
		}
	};
};
