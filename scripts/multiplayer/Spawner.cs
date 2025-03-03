using Godot;
using System;

public partial class Spawner : Node2D {
	[Export]
	public Timer SpawnTime;
	[Export]
	public Resource Item;

	private void OnSpawnTimeTimeout() {
	}

	public override void _Ready() {
		SpawnTime.Connect( "timeout", Callable.From( OnSpawnTimeTimeout ) );
	}
}
