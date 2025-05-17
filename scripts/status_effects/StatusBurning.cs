using Godot;

public partial class StatusBurning : StatusEffect {
	private Timer DamageTimer;

	public override void _Ready() {
		base._Ready();

		DamageTimer = new Timer();
		DamageTimer.WaitTime = 1.0f;
		DamageTimer.Autostart = true;
		DamageTimer.Connect( "timeout", Callable.From( () => { Victim.Damage( null, 15.0f * (float)GetProcessDeltaTime() ); } ) );
		AddChild( DamageTimer );
	}
};
