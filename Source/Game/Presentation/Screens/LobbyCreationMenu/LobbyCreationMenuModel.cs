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
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Multiplayer.Caching;

namespace Nomad.Game.Presentation.Screens.LobbyCreationMenu
{
	internal sealed class LobbyCreationMenuModel
	{
		public LobbyCreateInfo Info { get; private set; }

		public LobbyCreationMenuModel()
		{
			// FIXME: this is a placeholder
			Info = new LobbyCreateInfo {
				MaxPlayers = 16,
				GameMode = GameModeCache.Modes[ MultiplayerMode.Deathmatch ].DisplayName
			};
		}

		public void SetName( string name )
		{
			Info = Info with { Name = name };
		}

		public void SetMap( string map )
		{
			Info = Info with { Map = map };
		}

		public void SetGameMode( string gameMode )
		{
			Info = Info with { GameMode = gameMode };
		}

		public void SetVisibility( LobbyVisibility visibility )
		{
			Info = Info with { Visibility = visibility };
		}
	};
};
