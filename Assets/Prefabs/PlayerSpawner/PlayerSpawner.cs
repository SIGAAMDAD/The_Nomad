using Godot;
using Nomad.EngineUtils;
using Nomad.Events.Extensions;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using System;

namespace Nomad.Game.Prefabs {
	public partial class PlayerSpawner : Node2D {
		public override void _Ready() {
			base._Ready();

			GameEventRegistry.GetEvent<PlayerSpawnRequestedEventArgs>( EventNames.PLAYER_SPAWN_REQUESTED, EventNames.NAMESPACE )
				.PublishAfter( new PlayerSpawnRequestedEventArgs( Guid.NewGuid(), GlobalPosition.ToSystem(), true ), 2000 );
		}
	};
};
