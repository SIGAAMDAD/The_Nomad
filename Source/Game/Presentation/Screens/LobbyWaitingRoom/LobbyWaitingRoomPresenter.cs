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
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Multiplayer.Lobby;
using Nomad.Networking.Session;

namespace Nomad.Game.Presentation.Screens.LobbyWaitingRoom
{
	/*
	===================================================================================

	LobbyWaitingRoomPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class LobbyWaitingRoomPresenter
	{
		private readonly LobbyWaitingRoomView _view;
		private readonly LobbyWaitingRoomModel _model;

		private readonly INetworkSessionService _sessionService;
		private readonly IVotingService _votingService;
		private readonly ILobbyWaitingRoomService _waitingRoomService;

		public LobbyWaitingRoomPresenter(
			LobbyWaitingRoomView view,
			LobbyWaitingRoomModel model,
			INetworkSessionService sessionService,
			IVotingService votingService,
			ILobbyWaitingRoomService waitingRoomService
		)
		{
			_sessionService = sessionService ?? throw new ArgumentNullException( nameof( sessionService ) );
			_votingService = votingService ?? throw new ArgumentNullException( nameof( votingService ) );
			_waitingRoomService = waitingRoomService ?? throw new ArgumentNullException( nameof( waitingRoomService ) );
			_view = view;
			_model = model;

			view.Leave += OnLeaveLobby;
			view.ReadyUp += OnReadyUp;
			view.StartGame += OnStartGame;
			view.VoteStart += OnVoteStart;

			model.PlayerConnected += OnPlayerConnected;
			model.PlayerDisconnected += OnPlayerDisconnected;

			// open up the waiting room
			waitingRoomService.OpenWaitingRoom();

			model.RefreshMemberList();
		}

		public void Update()
		{
			_waitingRoomService.Frame();
		}

		private void OnPlayerConnected( string displayName, PeerId peerId, bool isHost )
		{
			_view.AddMember( displayName, peerId );
		}

		private void OnPlayerDisconnected( int index )
		{
			_view.RemoveMember( index );
		}

		private void OnReadyUp()
		{
			if ( !_waitingRoomService.CanReady || !_sessionService.IsClient ) {
				return;
			}
			if ( !_waitingRoomService.TryGetPeerStatus( _sessionService.CurrentSession.LocalPeerId, out var status ) ) {
				return;
			}
			if ( status.ReadyState == LobbyReadyState.Ready ) {
				_waitingRoomService.RequestNotReady();
			} else {
				_waitingRoomService.RequestReady();
			}
		}

		private async void OnLeaveLobby()
		{
			await _sessionService.StopAsync();
		}

		private void OnVoteStart()
		{
			if ( !_votingService.CanRequestVote || !_sessionService.IsClient ) {
				return;
			}
			_votingService.RequestStartGameVote();
		}

		private void OnStartGame()
		{
			if ( !_waitingRoomService.CanStart || !_sessionService.IsHost ) {
				return;
			}
			if ( _waitingRoomService.IsOpen ) {
				_waitingRoomService.CloseWaitingRoom();
				_waitingRoomService.StartCountdown( 5 );
			} else {
				_waitingRoomService.OpenWaitingRoom();
				_waitingRoomService.CancelCountdown();
			}
		}
	};
};
