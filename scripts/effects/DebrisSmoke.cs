using System;
using Godot;

public partial class DebrisSmoke : Sprite2D {
	private float VSpeed = 0.0f;
	private float HSpeed = 0.0f;

	/*
	private float Randf( Random random, float min, float max ) {
		return (float)( min + random.NextDouble() * ( min - max ) );
	}
	*/

	public override void _Ready() {
		base._Ready();

		RandomNumberGenerator random = new RandomNumberGenerator();
		VSpeed = random.RandfRange( -0.25f, 0.25f );
		HSpeed = random.RandfRange( -0.1f, 0.1f );

		ZIndex = 3;

		SetProcessInternal( false );
		SetPhysicsProcess( false );
		SetPhysicsProcessInternal( false );
	}
    public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
			return;
		}

		base._Process( delta );

		Godot.Vector2 position = Position;
		position.X += VSpeed;
		position.Y += HSpeed;
		Position = position;
	}
};