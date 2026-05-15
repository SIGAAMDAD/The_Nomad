using Godot;
using Nomad.EngineUtils;
using Nomad.Events.Extensions;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
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
