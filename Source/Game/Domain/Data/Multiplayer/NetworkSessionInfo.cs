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

namespace Nomad.Game.Domain.Data.Multiplayer {
	public sealed record NetworkSessionInfo {
		public Guid SessionId { get; init; }
		public NetworkSessionMode Mode { get; init; }
		public NetworkConnectionState State { get; init; }
		public int MaxPlayers { get; init; }
		public int MinPlayers { get; init; }
		public PeerId LocalPeerId { get; init; }
		public PeerId HostPeerId { get; init; }
		public IReadOnlyList<NetworkPeerInfo> Peers { get; init; } = Array.Empty<NetworkPeerInfo>();
	};
};