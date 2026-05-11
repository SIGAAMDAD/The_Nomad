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
using Nomad.Game.Presentation.Screens.DeveloperCommentaryMenu;
using Nomad.Game.Presentation.Screens.MultiplayerMenu;

namespace Nomad.Game.Presentation.Screens.ExtrasMenu
{
	/*
	===================================================================================

	ExtrasMenuView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class ExtrasMenuView : Control
	{
		private MultiplayerMenuView _multiplayerMenu;
		private DeveloperCommentaryMenuView _developerCommentaryMenu;

		public event Action Back;
		public event Action MultiplayerMenu;
		public event Action DeveloperCommentaryMenu;

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

			_multiplayerMenu = GetNode<MultiplayerMenuView>( "MultiplayerMenu" );
			_developerCommentaryMenu = GetNode<DeveloperCommentaryMenuView>( "DeveloperCommentaryMenu" );

			GetNode<Button>( "ButtonContainer/DeveloperCommentaryButton" ).Pressed += () => DeveloperCommentaryMenu?.Invoke();
			GetNode<Button>( "ButtonContainer/MultiplayerButton" ).Pressed += () => MultiplayerMenu?.Invoke();
			GetNode<Button>( "ButtonContainer/BackButton" ).Pressed += () => Back?.Invoke();
		}
	};
};
