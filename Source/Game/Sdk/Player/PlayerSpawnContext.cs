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

using System.Numerics;
using Nomad.Game.Sdk.Multiplayer;

namespace Nomad.Game.Sdk.Player
{
    public readonly struct PlayerSpawnContext
    {
        public PlayerSpawnReason Reason { get; }
        public Vector2 SpawnPosition { get; }
        public Vector3 SpawnWorldPosition { get; }
        public string? SpawnPointId { get; }
        public string? ProfileOverrideId { get; }
        public PlayerId RequestedPlayerId { get; }
        public int LocalPlayerIndex { get; }

        public PlayerSpawnContext(
            PlayerSpawnReason reason,
            Vector2 spawnPosition,
            string? spawnPointId = null,
            string? profileOverrideId = null,
            PlayerId requestedPlayerId = default,
            int localPlayerIndex = 0,
            Vector3? spawnWorldPosition = null
        )
        {
            Reason = reason;
            SpawnPosition = spawnPosition;
            SpawnWorldPosition = spawnWorldPosition ?? new Vector3( spawnPosition.X, 0.0f, spawnPosition.Y );
            SpawnPointId = spawnPointId;
            ProfileOverrideId = profileOverrideId;
            RequestedPlayerId = requestedPlayerId;
            LocalPlayerIndex = localPlayerIndex;
        }
    }
}
