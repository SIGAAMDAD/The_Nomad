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
using Nomad.Core.Events;
using Nomad.Game.Application.Gameplay.Inventory;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;

namespace Nomad.Game.Application.Gameplay
{
	internal sealed class GameplayApplicationCoordinator : IDisposable
	{
		private InteractableApplicationCoordinator _interactableCoordinator;

		private readonly IDisposable _gameStateChanged;

		private bool _isDisposed = false;

		public GameplayApplicationCoordinator( IGameEventRegistryService eventFactory )
		{
			_gameStateChanged = eventFactory
				.GetEvent<GameStateChangedEventArgs>(
					GameStateChangedEventArgs.Name,
					GameStateChangedEventArgs.NameSpace
				)
				.Subscribe( OnGameStateChanged );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_gameStateChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnGameStateChanged( in GameStateChangedEventArgs args )
		{
			if ( args.CurrentState == GameState.Level ) {

			}
		}
	};
};
