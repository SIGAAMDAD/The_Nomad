using System;
using Godot;

public partial class BulletShell : Sprite2D {
	public AudioStreamPlayer2D GroundedSfx;

	private float VSpeed = 0.0f;
	private float HSpeed = 0.0f;

	private void OnSoundFinished() {
		GroundedSfx.QueueFree();
	}

	private void OnTimerTimeout() {
		SetPhysicsProcess( false );
		SetProcess( false );
		SetProcessInput( false );

		GroundedSfx.GlobalPosition = GlobalPosition;
		GroundedSfx.Play();
		GetNode<Timer>( "Timer" ).QueueFree();
	}

	public override void _Ready() {
		base._Ready();

		RandomNumberGenerator random = new RandomNumberGenerator();
		VSpeed = random.RandfRange( -2.75f, 2.75f );
		HSpeed = random.RandfRange( -2.5f, 2.5f );

		GroundedSfx = new AudioStreamPlayer2D();
		GroundedSfx.Connect( "finished", Callable.From( OnSoundFinished ) );
		GroundedSfx.VolumeDb = 20.0f;
		GroundedSfx.MaxDistance = 900.0f;
		AddChild( GroundedSfx );

		GetNode<Timer>( "Timer" ).Connect( "timeout", Callable.From( OnTimerTimeout ) );

		VSpeed = Mathf.Lerp( VSpeed, 5.0f, 0.02f );
		HSpeed = Mathf.Lerp( HSpeed, 0.0f, 0.02f );
	}

	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		Godot.Vector2 position = Position;
		position.X += VSpeed;
		position.Y += HSpeed;
		Position = position;
	}
};