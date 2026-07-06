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
using Nomad.Core;
using Nomad.Core.OnlineServices;
using Nomad.Networking.Session;

namespace Nomad.Game.Multiplayer
{
	internal sealed class MultiplayerPeerRoster
	{
		public bool IsSessionActive => _sessionService.IsSessionActive;
		public bool IsHost => _sessionService.IsHost;
		public bool IsClient => _sessionService.IsClient;
		public NetworkSessionInfo? CurrentSession => _sessionService.CurrentSession;
		public PeerId LocalPeerId => _sessionService.CurrentSession?.LocalPeerId ?? default;
		public PeerId HostPeerId => _sessionService.CurrentSession?.HostPeerId ?? default;
		public int PeerCount => _sessionService.CurrentSession?.PeerCount ?? 0;

		private readonly INetworkSessionService _sessionService;

		public MultiplayerPeerRoster( INetworkSessionService sessionService )
		{
			_sessionService = sessionService ?? throw new ArgumentNullException( nameof( sessionService ) );
		}

		public NetworkPeerInfo? GetLocalPeer()
		{
			return FindPeer( LocalPeerId );
		}

		public NetworkPeerInfo? GetHostPeer()
		{
			return FindPeer( HostPeerId );
		}

		public NetworkPeerInfo? FindPeer( PeerId peerId )
		{
			NetworkSessionInfo? session = CurrentSession;
			if ( session == null ) {
				return null;
			}

			for ( int i = 0; i < session.Peers.Count; i++ ) {
				if ( session.Peers[i].PeerId.Equals( peerId ) ) {
					return session.Peers[i];
				}
			}

			return null;
		}

		public NetworkPeerInfo? GetFirstNonHostPeer()
		{
			NetworkSessionInfo? session = CurrentSession;
			if ( session == null ) {
				return null;
			}

			int bestSlot = int.MaxValue;
			NetworkPeerInfo? bestPeer = null;

			for ( int i = 0; i < session.Peers.Count; i++ ) {
				NetworkPeerInfo peer = session.Peers[i];
				if ( peer.PeerId.Equals( session.HostPeerId ) ) {
					continue;
				}

				if ( peer.PlayerSlot < bestSlot ) {
					bestSlot = peer.PlayerSlot;
					bestPeer = peer;
				}
			}

			return bestPeer;
		}

		public NetworkPeerInfo? GetPeerBySlot( int playerSlot )
		{
			NetworkSessionInfo? session = CurrentSession;
			if ( session == null ) {
				return null;
			}

			for ( int i = 0; i < session.Peers.Count; i++ ) {
				if ( session.Peers[i].PlayerSlot == playerSlot ) {
					return session.Peers[i];
				}
			}

			return null;
		}
	};
};
