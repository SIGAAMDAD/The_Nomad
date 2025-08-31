using Godot;

public partial class Incendiary : Grenade {
	private Timer Timer;

	private void OnTimerTimeout() {
		Explosion explosion = ResourceCache.GetScene( "res://scenes/effects/explosion" ).Instantiate<Explosion>();
		explosion.Radius = 126.0f;
		explosion.Damage = 60.0f;
		explosion.DamageCurve = (Curve)ResourceCache.GetResource( "res://resources/damage_curves/explosions/incendiary.tres" );
		AddChild( explosion );

		CallDeferred( MethodName.QueueFree );
	}

	public override void _Ready() {
		base._Ready();

		Timer = GetNode<Timer>( "Timer" );
		Timer.Connect( Timer.SignalName.Timeout, Callable.From( OnTimerTimeout ) );
	}
};