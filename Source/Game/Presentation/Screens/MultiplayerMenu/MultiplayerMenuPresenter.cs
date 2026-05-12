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
using Nomad.Game.Application.UI.Menus;

namespace Nomad.Game.Presentation.Screens.MultiplayerMenu
{
	internal sealed class MultiplayerMenuPresenter
	{
		private readonly MultiplayerMenuView _view;
		private readonly MultiplayerMenuModel _model;

		private readonly IGameEventRegistryService _eventFactory;

		public MultiplayerMenuPresenter( MultiplayerMenuView view, MultiplayerMenuModel model, IGameEventRegistryService eventFactory )
		{
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_view = view;
			_model = model;

			view.Back += OnBack;
			view.CreateLobby += OnCreateLobby;
		}

		private void OnCreateLobby()
		{
			_model.SetState( MultiplayerMenuState.LobbyCreation );
		}

		private void OnBack()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>(
					MenuTransitionRequestedEventArgs.Name,
					MenuTransitionRequestedEventArgs.NameSpace
				)
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Multiplayer, MenuState.Extras ) );
		}
	};
};
