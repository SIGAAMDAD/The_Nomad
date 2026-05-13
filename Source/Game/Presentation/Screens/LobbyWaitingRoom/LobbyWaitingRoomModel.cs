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
using Nomad.Core.OnlineServices;
using Nomad.Networking.Session;

namespace Nomad.Game.Presentation.Screens.LobbyWaitingRoom
{
	internal sealed class LobbyWaitingRoomModel
	{
		public bool IsHost => _sessionService.IsHost;

		private readonly Dictionary<PeerId, NetworkPeerInfo> _members = new();

		private readonly INetworkSessionService _sessionService;

		public event Action<string, PeerId, bool> PlayerConnected;
		public event Action<int> PlayerDisconnected;

		public LobbyWaitingRoomModel( INetworkSessionService sessionService )
		{
			_sessionService = sessionService ?? throw new ArgumentNullException( nameof( sessionService ) );

			_sessionService.PeerConnected.Subscribe( OnPeerConnected );
			_sessionService.PeerDisconnected.Subscribe( OnPeerDisconnected );
			_sessionService.SessionChanged.Subscribe( OnSessionChanged );

			RefreshMemberList();
		}

		private void OnPeerConnected( in PeerConnectedEventArgs args )
		{
			if ( _sessionService.CurrentSession == null ) {
				return;
			}

			foreach ( var peer in _sessionService.CurrentSession.Peers ) {
				if ( peer.PeerId == args.PeerId ) {
					_members.Add( peer.PeerId, peer );

					PlayerConnected?.Invoke(
						peer.DisplayName,
						peer.PeerId,
						peer.IsHost
					);
					return;
				}
			}
		}

		private void OnPeerDisconnected( in PeerDisconnectedEventArgs args )
		{
			int index = 0;
			foreach ( var member in _members ) {
				if ( member.Key == args.PeerId ) {
					break;
				}
				index++;
			}
			_members.Remove( args.PeerId );
			PlayerDisconnected?.Invoke( index );
		}

		private void OnSessionChanged( in NetworkSessionChangedEventArgs args )
		{
			RefreshMemberList();
		}

		public void RefreshMemberList()
		{
			_members.Clear();

			if ( _sessionService.CurrentSession == null ) {
				return;
			}

			foreach ( var peer in _sessionService.CurrentSession.Peers ) {
				_members.Add( peer.PeerId, peer );
				PlayerConnected?.Invoke( peer.DisplayName, peer.PeerId, peer.IsHost );
			}
		}
	};
};
