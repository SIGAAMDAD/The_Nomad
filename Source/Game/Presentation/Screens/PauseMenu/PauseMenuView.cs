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

namespace Nomad.Game.Presentation.Screens.PauseMenu
{
	/*
	===================================================================================

	PauseMenuView

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed partial class PauseMenuView : CanvasLayer
	{
		public event Action Resume;
		public event Action LoadGame;
		public event Action SettingsMenu;
		public event Action QuitToMainMenu;
		public event Action QuitGame;

		private PauseMenuPresenter _presenter;

		/*
		===============
		SetVisibility
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="visibility"></param>
		public void SetVisibility( bool visibility )
		{
			Visible = visibility;
		}

		/*
		===============
		OnResumeGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnResumeGame()
		{
			Resume?.Invoke();
		}

		/*
		===============
		OnLoadGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnLoadGame()
		{
			LoadGame?.Invoke();
		}

		/*
		===============
		OnSettingsMenu
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnSettingsMenu()
		{
			SettingsMenu?.Invoke();
		}

		/*
		===============
		OnQuitToMainMenu
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnQuitToMainMenu()
		{
			QuitToMainMenu?.Invoke();
		}

		/*
		===============
		OnQuitGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnQuitGame()
		{
			QuitGame?.Invoke();
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

			_presenter = ScreenPresenterFactory.CreatePauseMenuPresenter( this );

			GetNode<Button>( "OptionsContainer/ResumeGameButton" ).Pressed += OnResumeGame;
			GetNode<Button>( "OptionsContainer/LoadGameButton" ).Pressed += OnLoadGame;
			GetNode<Button>( "OptionsContainer/SettingsButton" ).Pressed += OnSettingsMenu;
			GetNode<Button>( "OptionsContainer/ExitWorldButton" ).Pressed += OnQuitToMainMenu;
			GetNode<Button>( "OptionsContainer/QuitGameButton" ).Pressed += OnQuitGame;
		}

		/*
		===============
		OnShutdown
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
