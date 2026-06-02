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
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using System;
using NumericsVector2 = System.Numerics.Vector2;

namespace Nomad.Game.Prefabs
{
	public partial class PlayerSpawner : Node2D
	{
		private const int MaxLocalPlayers = 4;

		[Export( PropertyHint.Range, "1,4,1" )]
		public int LocalPlayerCount { get; set; } = 1;

		public override void _Ready()
		{
			base._Ready();

			var spawnRequest = GameEventRegistry
				.GetEvent<PlayerSpawnRequestedEventArgs>( PlayerSpawnRequestedEventArgs.Name, PlayerSpawnRequestedEventArgs.NameSpace )
				.PublishAfter( 500 );

			int playerCount = Math.Clamp( LocalPlayerCount, 1, MaxLocalPlayers );
			for ( int i = 0; i < playerCount; i++ ) {
				var requestedPlayerId = new PlayerId( new PeerId( Guid.NewGuid() ) );
				var spawnPosition = GlobalPosition.ToSystem() + GetSpawnOffset( i );

				spawnRequest.Publish(
					new PlayerSpawnRequestedEventArgs(
						Guid.NewGuid(),
						new PlayerSpawnContext(
							PlayerSpawnReason.NewGame,
							spawnPosition,
							$"{Name}:{i + 1}",
							requestedPlayerId: requestedPlayerId,
							localPlayerIndex: i
						)
					)
				);
			}
		}

		private static NumericsVector2 GetSpawnOffset( int playerIndex )
		{
			return playerIndex switch {
				1 => new NumericsVector2( 48.0f, 0.0f ),
				2 => new NumericsVector2( 0.0f, 48.0f ),
				3 => new NumericsVector2( 48.0f, 48.0f ),
				_ => NumericsVector2.Zero
			};
		}
	};
};
