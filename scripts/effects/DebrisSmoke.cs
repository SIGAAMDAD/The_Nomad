using System;
using Godot;

public partial class DebrisSmoke : Sprite2D {
	private float VSpeed = 0.0f;
	private float HSpeed = 0.0f;

	public override void _Ready() {
		base._Ready();

		RandomNumberGenerator random = new RandomNumberGenerator();
		VSpeed = Mathf.Lerp( random.RandfRange( -0.025f, 0.025f ), 5.0f, 0.002f );
		HSpeed = Mathf.Lerp( random.RandfRange( -0.001f, 0.001f ), 0.0f, 0.0002f );
	}

    public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		Godot.Vector2 position = Position;
		position.X += VSpeed;
		position.Y += HSpeed;
		Position = position;
	}
};