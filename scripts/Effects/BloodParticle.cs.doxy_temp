using Godot;

public partial class BloodParticle : Sprite2D {
	private GpuParticles2D Splatter;
	private Timer Timer;

	private float VSpeed = 0.0f;
	private float HSpeed = 0.0f;

	private void OnSplatterTimeout() {
		Timer.Start();
	}
	private void OnTimerTimeout() {
		SetPhysicsProcess( false );
		Timer.QueueFree();
	}

	public override void _Ready() {
		base._Ready();

		RandomNumberGenerator random = new RandomNumberGenerator();
		VSpeed = random.RandfRange( -8.0f, 8.0f );
		HSpeed = random.RandfRange( -6.0f, 6.0f );
		
		Splatter = GetNode<GpuParticles2D>( "BloodSplatter" );
		Splatter.Emitting = true;
		( (Timer)Splatter.GetChild( 0 ) ).Connect( "timeout", Callable.From( OnSplatterTimeout ) );

		Timer = GetNode<Timer>( "Timer" );
		Timer.Connect( "timeout", Callable.From( OnTimerTimeout ) );
	}

	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		Godot.Vector2 position = Position;
		position.X += VSpeed;
		position.Y += HSpeed;
		Position = position;
	}
};