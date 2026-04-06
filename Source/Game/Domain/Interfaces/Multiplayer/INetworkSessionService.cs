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
using System.Threading;
using System.Threading.Tasks;
using Nomad.Game.Domain.Data.Multiplayer;

namespace Nomad.Game.Domain.Interfaces.Multiplayer {
	public interface INetworkSessionService : IDisposable {
		bool IsSessionActive { get; }
		bool IsHost { get; }
		bool IsClient { get; }
		NetworkSessionInfo? CurrentSession { get; }

		Task SendToHostAsync<TMessage>( TMessage message, CancellationToken ct = default );
		Task SendToPeerAsync<TMessage>( PeerId peerId, TMessage message, CancellationToken ct = default );
		Task BroadcastAsync<TMessage>( TMessage message, CancellationToken ct = default );
	};
};