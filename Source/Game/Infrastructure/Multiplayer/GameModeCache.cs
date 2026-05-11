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

using System.Collections.Generic;
using System.Collections.Immutable;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer.Lobby;

namespace Nomad.Game.Infrastructure.Multiplayer
{
	internal static class GameModeCache
	{
		// TODO: make this a json/binary cached loader instead of a baked data table

		public static readonly GameModeDefinition Bloodbath = new GameModeDefinition {
			Id = new InternString( "multiplayer.mode.bloodbath.id" ),
			DisplayName = new InternString( "multiplayer.mode.bloodbath.displayname" ),
			Description = new InternString( "multiplayer.mode.bloodbath.description" ),
			Mode = Mode.Deathmatch,
			MinPlayers = 1,
			MaxPlayers = 16
		};

		public static readonly ImmutableDictionary<Mode, GameModeDefinition> Modes = new Dictionary<Mode, GameModeDefinition> {
			[Mode.Deathmatch] = Bloodbath
		}.ToImmutableDictionary();
	};
};
