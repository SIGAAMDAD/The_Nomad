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

namespace Nomad.Game.Domain.Data.Multiplayer {
	public readonly struct NetworkPeerInfo {
		public PeerId PeerId { get; }
		public string DisplayName { get; }
		public bool IsHost { get; }
		public bool IsLocal { get; }
		public bool IsReady { get; }
		public int PlayerSlot { get; }

		public NetworkPeerInfo( PeerId peerId, string displayName, bool isHost, bool isLocal, bool isReady, int playerSlot ) {
			PeerId = peerId;
			DisplayName = displayName;
			IsHost = isHost;
			IsLocal = isLocal;
			IsReady = isReady;
			PlayerSlot = playerSlot;
		}
	};
};