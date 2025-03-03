using Godot;
using System;

public partial class DebrisCloud : Node2D {
	private Timer Timer = null;
	private Texture2D Texture = null;

	private void OnTimerTimeout() {
		Timer.QueueFree();
		QueueFree();
	}

	public override void _Ready() {
		Timer = GetNode<Timer>( "Timer" );
		Timer.Connect( "timeout", Callable.From( OnTimerTimeout ) );

		Texture = ResourceLoader.Load<Texture2D>( "res://textures/env/dustcloud.png" );
	}

    public void Create( Vector2 position ) {
		const int numSmokeClouds = 64;

		for ( int i = 0; i < numSmokeClouds; i++ ) {
			DebrisSmoke cloud = new DebrisSmoke();
			cloud.Texture = Texture;
			cloud.GlobalPosition = position;
			AddChild( cloud );
		}
	}
};
