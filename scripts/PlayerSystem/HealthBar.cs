using Godot;

namespace PlayerSystem {
	public partial class HealthBar : ProgressBar {
		private Timer DamageTimer;
		private Timer ShowTimer;
		private ProgressBar DamageBar;

		public void SetHealth( float health ) {
			ProcessMode = ProcessModeEnum.Pausable;

			Modulate = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
			ShowTimer.Start();

			float prevHealth = (float)Value;
			Value = Mathf.Min( health, (float)MaxValue );
			if ( (float)Value <= prevHealth ) {
				DamageTimer.Start();
			} else {
				DamageBar.Value = Value;
			}
		}

		public void Init( float nHealth ) {
			DamageBar = GetNode<ProgressBar>( "DamageBar" );

			MaxValue = nHealth;
			Value = nHealth;
			DamageBar.MaxValue = nHealth;
			DamageBar.Value = nHealth;

			ProcessMode = ProcessModeEnum.Disabled;

			ShowTimer = new Timer();
			ShowTimer.Name = "ShowTimer";
			ShowTimer.OneShot = true;
			ShowTimer.WaitTime = 8.5f;
			ShowTimer.Connect( "timeout", Callable.From(
				() => {
					Tween Tweener = CreateTween();
					Tweener.TweenProperty( this, "modulate", new Color( 0.0f, 0.0f, 0.0f, 0.0f ), 1.0f );
					Tweener.Connect( "finished", Callable.From( () => { ProcessMode = ProcessModeEnum.Disabled; } ) );
				}
			) );
			AddChild( ShowTimer );

			DamageTimer = GetNode<Timer>( "Timer" );
			DamageTimer.Connect( "timeout", Callable.From(
				() => {
					DamageBar.Value = Value;
				}
			) );
		}
	};
};