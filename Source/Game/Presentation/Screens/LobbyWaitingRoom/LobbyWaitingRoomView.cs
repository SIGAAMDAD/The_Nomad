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
using System.Linq;
using Godot;
using Nomad.Core.Engine.Globals;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.Util;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Networking.Session;

namespace Nomad.Game.Presentation.Screens.LobbyWaitingRoom
{
	internal sealed partial class LobbyWaitingRoomView : Control
	{
		private ILobbyWaitingRoomService _waitingRoomService;
		private IVotingService _votingService;
		private INetworkSessionService _networkSession;

		private VBoxContainer _memberList;

		public event Action Leave;
		public event Action StartGame;
		public event Action ReadyUp;
		public event Action ChangeSettings;

		public override void _Ready()
		{
			base._Ready();

			_memberList = GetNode<VBoxContainer>( "MemberList" );

			var locator = ServiceLocator.Instance;
			_waitingRoomService = locator.GetService<ILobbyWaitingRoomService>();
			_votingService = locator.GetService<IVotingService>();
			_networkSession = locator.GetService<INetworkSessionService>();

			Button lobbyOption1Button = GetNode<Button>( "ButtonContainer/LobbyOption1Button" );
			Button lobbyOption2Button = GetNode<Button>( "ButtonContainer/LobbyOption2Button" );

			// if we're not the host, we can only ready up.
			if ( !_waitingRoomService.CanStart ) {
				lobbyOption1Button.Text = LocalizationService.Translate( new InternString( "UI_LOBBY_VOTE_START" ) );
				lobbyOption2Button.Text = LocalizationService.Translate( new InternString( "UI_LOBBY_GAME_READY" ) );
			} else {
				lobbyOption1Button.Text = LocalizationService.Translate( new InternString( "UI_LOBBY_CHANGE_SETTINGS" ) );
				lobbyOption2Button.Text = LocalizationService.Translate( new InternString( "UI_LOBBY_START_GAME" ) );

				_waitingRoomService.OpenWaitingRoom();
			}
			_networkSession.PeerConnected.Subscribe( OnPeerConnected );
			_networkSession.PeerDisconnected.Subscribe( OnPeerDisconnected );

			var members = _networkSession.CurrentSession.Peers;
			foreach ( var peer in members ) {
				var label = new Label() {
					Text = peer.DisplayName
				};
				label.SetMeta( "PeerId", peer.PeerId.GetHashCode() );
				_memberList.AddChild( label );
			}
		}

		private void OnPeerConnected( in PeerConnectedEventArgs args )
		{
			var peerId = args.PeerId;
			var peer = _networkSession.CurrentSession.Peers.First( p => p.PeerId == peerId );

			var label = new Label() {
				Text = peer.DisplayName,
			};
			label.SetMeta( "PeerId", peer.PeerId.GetHashCode() );
			_memberList.AddChild( label );
		}

		private void OnPeerDisconnected( in PeerDisconnectedEventArgs args )
		{
			foreach ( var child in _memberList.GetChildren() ) {
				if ( child.GetMeta( "PeerId" ).AsInt32() == args.PeerId.GetHashCode() ) {
					_memberList.RemoveChild( child );
					break;
				}
			}
		}

		public override void _Process( double delta )
		{
			base._Process( delta );

			_waitingRoomService.Frame();
		}
	};
};
