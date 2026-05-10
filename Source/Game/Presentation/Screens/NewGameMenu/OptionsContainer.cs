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
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Events.Globals;
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.NewGameMenu
{
	/*
	===================================================================================

	OptionsContainer

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class OptionsContainer : EngineVerticalContainer
	{
		private IMusicService _musicService;
		private ShaderMaterial _material;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void OnInit()
		{
			_musicService = ServiceLocator.GetService<IMusicService>();

			GetNode<Button>( "StandardModeButton" ).Pressed += OnEasyDifficultySelected;
			GetNode<Button>( "HardModeButton" ).Pressed += OnHardDifficultySelected;
			GetNode<Button>( "CustomModeButton" ).Pressed += OnCustomDifficultySelected;
			GetNode<Button>( "BackButton" ).Pressed += OnBackButtonPressed;

			_material = Material as ShaderMaterial;
		}

		/*
		===============
		OnCustomDifficultySelected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnCustomDifficultySelected()
		{
			Visible = false;
		}

		/*
		===============
		OnEasyDifficultySelected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnEasyDifficultySelected()
		{
			_musicService.StopTheme( true );

			GameEventRegistry
				.GetEvent<WorldBootstrapRequestEventArgs>(
					WorldBootstrapRequestEventArgs.Name,
					WorldBootstrapRequestEventArgs.NameSpace
				)
				.Publish( new WorldBootstrapRequestEventArgs(
					requestId: Guid.NewGuid(),
					mode: WorldBootstrapMode.SinglePlayerNewGame,
					worldId: "world.single.default",
					difficulty: DifficultyPreset.Standard,
					lobbyid: null
				) );
		}

		/*
		===============
		OnHardDifficultySelected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnHardDifficultySelected()
		{
			_musicService.StopTheme( true );

			GameEventRegistry
				.GetEvent<WorldBootstrapRequestEventArgs>(
					WorldBootstrapRequestEventArgs.Name,
					WorldBootstrapRequestEventArgs.NameSpace
				)
				.Publish( new WorldBootstrapRequestEventArgs(
					requestId: Guid.NewGuid(),
					mode: WorldBootstrapMode.SinglePlayerNewGame,
					worldId: "world.single.default",
					difficulty: DifficultyPreset.Hard,
					lobbyid: null
				) );
		}

		/*
		===============
		OnBackButtonPressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnBackButtonPressed()
		{
			GameEventRegistry
				.GetEvent<MenuTransitionRequestedEventArgs>(
					MenuTransitionRequestedEventArgs.Name,
					MenuTransitionRequestedEventArgs.NameSpace
				)
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.NewGame, MenuState.Main ) );
		}
	};
};
