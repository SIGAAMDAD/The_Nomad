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

using System.Collections.Generic;
using Nomad.Game.Infrastructure.Multiplayer;

namespace Nomad.Game.Presentation.Screens.LobbyCreationMenu
{
	internal sealed class LobbyCreationMenuPresenter
	{
		private readonly LobbyCreationMenuModel _model;
		private readonly LobbyCreationMenuView _view;

		public LobbyCreationMenuPresenter( LobbyCreationMenuView view, LobbyCreationMenuModel model )
		{
			_model = model;
			_view = view;

			view.CreateLobby += OnCreateLobby;
			view.Back += OnBack;
		}

		public void SyncView()
		{
			var gameModes = new List<string>();
			foreach ( var gameMode in GameModeCache.Modes ) {
				gameModes.Add( gameMode.Value.DisplayName );
			}

			_view.SetGameModeOptions( gameModes );
			_view.SetGameModeValue( gameModes.IndexOf( _model.Info.GameMode ) );

			var maps = new List<string>();
			foreach ( var gameMode in GameModeCache.Modes ) {
				gameModes.Add( gameMode.Value.DisplayName );
			}

			_view.SetGameModeOptions( gameModes );
			_view.SetGameModeValue( gameModes.IndexOf( _model.Info.GameMode ) );
		}

		private void OnCreateLobby()
		{
		}

		private void OnBack()
		{
		}
	};
};
