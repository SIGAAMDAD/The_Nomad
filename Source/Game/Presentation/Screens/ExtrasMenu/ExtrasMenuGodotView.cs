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

namespace Nomad.Game.Presentation.Screens.ExtrasMenu
{
	/*
	===================================================================================

	ExtrasMenuGodotView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class ExtrasMenuGodotView : Control, IExtrasMenuView
	{
		public event Action Back;
		public event Action MultiplayerMenu;
		public event Action DeveloperCommentaryMenu;
		public event Action ChallengeModeMenu;

		private ExtrasMenuPresenter _presenter;

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

			GetNode<Button>( "OptionsContainer/DeveloperCommentaryButton" ).Pressed += () => DeveloperCommentaryMenu?.Invoke();
			GetNode<Button>( "OptionsContainer/MultiplayerButton" ).Pressed += () => MultiplayerMenu?.Invoke();
			GetNode<Button>( "OptionsContainer/BackButton" ).Pressed += () => Back?.Invoke();

			_presenter = ScreenPresenterFactory.CreateExtrasMenuPresenter( this );
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

			_presenter.Dispose();
		}
	};
};
