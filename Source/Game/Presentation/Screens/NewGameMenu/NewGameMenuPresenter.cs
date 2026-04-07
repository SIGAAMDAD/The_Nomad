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
using Nomad.Events.Globals;
using Nomad.Game.Application.UI;
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Application.UI.Menus.Events;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;

namespace Nomad.Game.Presentation.Screens.NewGameMenu {
	/*
	===================================================================================
	
	NewGameMenuPresenter
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class NewGameMenuPresenter : IDisposable {
		private readonly INewGameMenuView _view;
		private readonly NewGameMenuModel _model;

		public NewGameMenuPresenter( INewGameMenuView view, NewGameMenuModel model ) {
			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_model = model ?? throw new ArgumentNullException( nameof( model ) );

			_view.OptionsView.StandardModeRequested += OnStandardModeRequested;
			_view.OptionsView.HardModeRequested += OnHardModeRequested;
			_view.OptionsView.CustomModeRequested += OnCustomModeRequested;
			_view.OptionsView.BackRequested += OnOptionsBackRequested;
			_view.CustomDifficultyView.BackRequested += OnCustomDifficultyBackRequested;

			SyncView();
		}

		public void Dispose() {
			_view.OptionsView.StandardModeRequested -= OnStandardModeRequested;
			_view.OptionsView.HardModeRequested -= OnHardModeRequested;
			_view.OptionsView.CustomModeRequested -= OnCustomModeRequested;
			_view.OptionsView.BackRequested -= OnOptionsBackRequested;
			_view.CustomDifficultyView.BackRequested -= OnCustomDifficultyBackRequested;
		}

		private void OnStandardModeRequested() {
			BeginGame();
		}

		private void OnHardModeRequested() {
			// TODO:
			// When BeginGameEventArgs carries difficulty/preset information,
			// this is where Hard mode should be passed through.
			BeginGame();
		}

		private void OnCustomModeRequested() {
			_model.ShowCustomDifficulty();
			SyncView();
		}

		private void OnOptionsBackRequested() {
			GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>(
				UIConstants.MENU_TRANSITION_REQUESTED_EVENT,
				UIConstants.NAMESPACE
			).Publish( new MenuTransitionRequestedEventArgs( MenuState.NewGame, MenuState.Main ) );
		}

		private void OnCustomDifficultyBackRequested() {
			_model.ShowOptions();
			SyncView();
		}

		private void SyncView() {
			_view.OptionsView.SetVisibility( _model.IsOptionsVisible );
			_view.CustomDifficultyView.SetVisibility( _model.IsCustomDifficultyVisible );
		}

		private static void BeginGame() {
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
	};
};