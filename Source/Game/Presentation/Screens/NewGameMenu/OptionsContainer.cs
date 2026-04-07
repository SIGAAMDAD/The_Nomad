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
using Nomad.Audio.Interfaces;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Events.Globals;
using Nomad.Game.Application.UI;
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Application.UI.Menus.Events;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.NewGameMenu {
	/*
	===================================================================================
	
	OptionsContainer
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class OptionsContainer : EngineVerticalContainer {
		private IMusicService _musicService;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			_musicService = ServiceLocator.GetService<IMusicService>();

			FindChild<EngineButton>( "StandardModeButton" ).Clicked.Subscribe( OnEasyDifficultySelected );
			FindChild<EngineButton>( "HardModeButton" ).Clicked.Subscribe( OnHardDifficultySelected );
			FindChild<EngineButton>( "CustomModeButton" ).Clicked.Subscribe( OnCustomDifficultySelected );
			FindChild<EngineButton>( "BackButton" ).Clicked.Subscribe( OnBackButtonPressed );
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
		/// <param name="args"></param>
		private void OnEasyDifficultySelected( in EmptyEventArgs args ) {
			_musicService.StopTheme( true );
			
			GameEventRegistry
				.GetEvent<WorldBootstrapRequestEventArgs>( EventNames.WORLD_BOOTSTRAP_REQUESTED, EventNames.NAMESPACE )
				.Publish( new WorldBootstrapRequestEventArgs(
					requestId: Guid.NewGuid(),
					mode: WorldBootstrapMode.SinglePlayer,
					worldId: "world.single.default",
					difficulty: DifficultyPreset.Standard,
					lobbyId: null
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
		/// <param name="args"></param>
		private void OnHardDifficultySelected( in EmptyEventArgs args ) {
			_musicService.StopTheme( true );

			GameEventRegistry
				.GetEvent<WorldBootstrapRequestEventArgs>( EventNames.WORLD_BOOTSTRAP_REQUESTED, EventNames.NAMESPACE )
				.Publish( new WorldBootstrapRequestEventArgs(
					requestId: Guid.NewGuid(),
					mode: WorldBootstrapMode.SinglePlayer,
					worldId: "world.single.default",
					difficulty: DifficultyPreset.Hard,
					lobbyId: null
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
		/// <param name="args"></param>
		private void OnBackButtonPressed( in EmptyEventArgs args ) {
			GameEventRegistry
				.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.NewGame, MenuState.Main ) );
		}
	};
};