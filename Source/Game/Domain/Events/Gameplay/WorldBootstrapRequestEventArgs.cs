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
using Nomad.Game.Domain.Data.Gameplay;

namespace Nomad.Game.Domain.Events.Gameplay {
	public readonly struct WorldBootstrapRequestEventArgs {
		public Guid RequestId { get; }
		public WorldBootstrapMode Mode { get; }
		public string WorldId { get; }
		public DifficultyPreset Difficulty { get; }
		public Guid? LobbyId { get; }

		public WorldBootstrapRequestEventArgs( Guid requestId, WorldBootstrapMode mode, string worldId, DifficultyPreset difficulty, Guid? lobbyId = null ) {
			RequestId = requestId;
			Mode = mode;
			WorldId = worldId;
			Difficulty = difficulty;
			LobbyId = lobbyId;
		}
	};
};