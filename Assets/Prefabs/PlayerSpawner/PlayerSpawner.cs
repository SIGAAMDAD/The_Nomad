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

using Godot;
using Nomad.EngineUtils;
using Nomad.Events.Extensions;
using Nomad.Events.Globals;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Player;
using System;

namespace Nomad.Game.Prefabs
{
	public partial class PlayerSpawner : Node2D
	{
		public override void _Ready()
		{
			base._Ready();

			var spawnRequest = GameEventRegistry
				.GetEvent<PlayerSpawnRequestedEventArgs>( PlayerSpawnRequestedEventArgs.Name, PlayerSpawnRequestedEventArgs.NameSpace )
				.PublishAfter( 500 );

			spawnRequest.Publish( new PlayerSpawnRequestedEventArgs(
				Guid.NewGuid(),
				new PlayerSpawnContext( PlayerSpawnReason.NewGame, GlobalPosition.ToSystem(), Name ) )
			);
		}
	};
};
