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

namespace Nomad.Game.Domain.Data.Gameplay {
	public readonly struct WorldBootstrapSucceededEventArgs {
		public Guid RequestId { get; }
		public WorldBootstrapMode Mode { get; }
		public string WorldId { get; }
		public Guid WorldInstanceId { get; }
		public Guid? LobbyId { get; }

		public WorldBootstrapSucceededEventArgs( Guid requestId, WorldBootstrapMode mode, string worldId, Guid worldInstanceId, Guid? lobbyId ) {
			RequestId = requestId;
			Mode = mode;
			WorldId = worldId;
			WorldInstanceId = worldInstanceId;
			LobbyId = lobbyId;
		}
	};
};