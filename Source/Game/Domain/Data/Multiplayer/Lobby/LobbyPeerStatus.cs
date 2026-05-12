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

using Nomad.Core.OnlineServices;

namespace Nomad.Game.Domain.Data.Multiplayer.Lobby
{
	public readonly struct LobbyPeerStatus
	{
		public PeerId PeerId { get; }
		public LobbyReadyState ReadyState { get; }
		public bool IsHost { get; }
		public bool IsLocal { get; }
		public bool IsConnected { get; }

		public LobbyPeerStatus( PeerId peerId, LobbyReadyState readyState, bool isHost, bool isLocal, bool isConnected )
		{
			PeerId = peerId;
			ReadyState = readyState;
			IsHost = isHost;
			IsLocal = isLocal;
			IsConnected = isConnected;
		}
	};
};
