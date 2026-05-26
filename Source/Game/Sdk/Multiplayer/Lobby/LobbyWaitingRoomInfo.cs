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

namespace Nomad.Game.Sdk.Multiplayer.Lobby
{
	public sealed record LobbyWaitingRoomInfo
	{
		public Guid LobbyId { get; init; }
		public LobbyWaitingRoomState State { get; init; }
		public int MinPlayers { get; init; }
		public int MaxPlayers { get; init; }
		public int PlayerCount { get; init; }
		public int ReadyCount { get; init; }
		public bool CanStart { get; init; }
		public bool CanReady { get; init; }
		public bool LateJoinAllowed { get; init; }
		public int CountdownSecondsRemaining { get; init; }
		public uint StateVersion { get; init; }
		public uint RosterVersion { get; init; }
	}
}
