using Godot;

namespace PlayerSystem {
	public partial class HealthBar : ProgressBar {
		private Timer DamageTimer;
		private Timer ShowTimer;
		private ProgressBar DamageBar;

		public void SetHealth( float health ) {
			ProcessMode = ProcessModeEnum.Pausable;

			Show();
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
			DamageBar.SetProcess( false );
			DamageBar.SetProcessInternal( false );

			MaxValue = nHealth;
			Value = nHealth;
			DamageBar.MaxValue = nHealth;
			DamageBar.Value = nHealth;

			ProcessMode = ProcessModeEnum.Disabled;

			ShowTimer = new Timer();
			ShowTimer.OneShot = true;
			ShowTimer.WaitTime = 3.5f;
			ShowTimer.Connect( "timeout", Callable.From(
				() => {
					Hide();
					ProcessMode = ProcessModeEnum.Disabled;
				}
			) );
			AddChild( ShowTimer );

			DamageTimer = GetNode<Timer>( "Timer" );
			DamageTimer.SetProcess( false );
			DamageTimer.SetProcessInternal( false );
			DamageTimer.Connect( "timeout", Callable.From(
				() => {
					DamageBar.Value = Value;

				}
			) );
		}
	};
};