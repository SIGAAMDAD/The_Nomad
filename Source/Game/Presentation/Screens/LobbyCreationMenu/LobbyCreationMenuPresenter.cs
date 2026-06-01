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
using System.Collections.Generic;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Infrastructure.Multiplayer;
using Nomad.Networking.Session;

namespace Nomad.Game.Presentation.Screens.LobbyCreationMenu
{
	/*
	===================================================================================

	LobbyCreationMenuPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class LobbyCreationMenuPresenter : IDisposable
	{
		private readonly LobbyCreationMenuModel _model;
		private readonly ILobbyCreationMenuView _view;
		private readonly INetworkSessionService _sessionService;

		private readonly IGameEventRegistryService _eventFactory;

		private bool _isDisposed = false;

		public LobbyCreationMenuPresenter( ILobbyCreationMenuView view, LobbyCreationMenuModel model, INetworkSessionService sessionService, IGameEventRegistryService eventFactory )
		{
			_model = model;
			_view = view;
			_sessionService = sessionService ?? throw new ArgumentNullException( nameof( sessionService ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			view.CreateLobby += OnCreateLobby;
			view.Back += OnBack;

			SyncView();

			_eventFactory
				.GetEvent<LobbyStartResultEventArgs>(
					LobbyStartResultEventArgs.Name,
					LobbyStartResultEventArgs.NameSpace
				)
				.Subscribe( OnLobbyStartResult );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_view.CreateLobby -= OnCreateLobby;
			_view.Back -= OnBack;

			_eventFactory
				.GetEvent<LobbyStartResultEventArgs>(
					LobbyStartResultEventArgs.Name,
					LobbyStartResultEventArgs.NameSpace
				)
				.Unsubscribe( OnLobbyStartResult );

			_isDisposed = true;
			GC.SuppressFinalize( this );
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

		private async void OnCreateLobby()
		{
			await _sessionService.StartHostAsync( _model.Info ).ConfigureAwait( false );
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

		private void OnLobbyStartResult( in LobbyStartResultEventArgs args )
		{
			if ( !args.Success ) {
				// TODO: create error dialogue
				GD.PushError( "Error creating lobby" );
				return;
			}
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>(
					MenuTransitionRequestedEventArgs.Name,
					MenuTransitionRequestedEventArgs.NameSpace
				)
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Multiplayer, MenuState.LobbyWaitingRoom ));
		}
	};
};
