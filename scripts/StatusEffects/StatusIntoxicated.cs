using Godot;

public partial class StatusIntoxicated : StatusEffect {
    private Timer DamageTimer;

    public override void _Ready() {
		base._Ready();

		DamageTimer = new Timer();
		DamageTimer.WaitTime = 4.0f;
		DamageTimer.Connect( "timeout", Callable.From( () => Victim.Damage( null, 2.0f * (float)GetProcessDeltaTime() ) ) );
		AddChild( DamageTimer );
	}
};