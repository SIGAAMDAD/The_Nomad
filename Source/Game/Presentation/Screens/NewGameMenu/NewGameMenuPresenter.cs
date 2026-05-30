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
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Events.Gameplay;
using Nomad.Core.Events;

namespace Nomad.Game.Presentation.Screens.NewGameMenu
{
	/*
	===================================================================================

	NewGameMenuPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class NewGameMenuPresenter : IDisposable
	{
		private readonly NewGameMenuView _view;
		private readonly IGameEventRegistryService _eventFactory;

		private bool _isDisposed = false;

		public NewGameMenuPresenter( NewGameMenuView view, IGameEventRegistryService eventFactory )
		{
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_view.StartStandardMode += OnStandardModeRequested;
			_view.StartHardMode += OnHardModeRequested;
			_view.StartCustomDifficulty += OnCustomModeRequested;
			_view.OptionsBack += OnOptionsBackRequested;
			_view.CustomDifficultyBack += OnCustomDifficultyBackRequested;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_view.StartStandardMode -= OnStandardModeRequested;
			_view.StartHardMode -= OnHardModeRequested;
			_view.StartCustomDifficulty -= OnCustomModeRequested;
			_view.OptionsBack -= OnOptionsBackRequested;
			_view.CustomDifficultyBack -= OnCustomDifficultyBackRequested;

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnStandardModeRequested()
		{
			BeginGame();
		}

		private void OnHardModeRequested()
		{
			BeginGame();
		}

		private void OnCustomModeRequested()
		{
			_view.SetOptionsContainerVisibility( false );
			_view.SetCustomDifficultyContainerVisibility( true );
		}

		private void OnOptionsBackRequested()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>(
					MenuTransitionRequestedEventArgs.Name,
					MenuTransitionRequestedEventArgs.NameSpace
				)
				.Publish(
					new MenuTransitionRequestedEventArgs(
						MenuState.NewGame,
						MenuState.Main
					)
				);
		}

		private void OnCustomDifficultyBackRequested()
		{
			_view.SetCustomDifficultyContainerVisibility( false );
			_view.SetOptionsContainerVisibility( true );
		}

		private void BeginGame()
		{
			_eventFactory
				.GetEvent<WorldBootstrapRequestEventArgs>(
					WorldBootstrapRequestEventArgs.Name,
					WorldBootstrapRequestEventArgs.NameSpace
				)
				.Publish(
					new WorldBootstrapRequestEventArgs(
						requestId: Guid.NewGuid(),
						mode: WorldBootstrapMode.SinglePlayerNewGame,
						worldId: "world.single.default",
						difficulty: DifficultyPreset.Standard,
						lobbyid: null
					)
				);
		}
	};
};
