using System;
using Godot;

namespace PlayerSystem {
	public partial class HealthBar : ProgressBar {
		private Timer Timer;
		private ProgressBar DamageBar;

		public void SetHealth( float health ) {
			float prevHealth = (float)Value;
			Value = Mathf.Min( health, (float)MaxValue );
			if ( (float)Value <= prevHealth ) {
				Timer.Start();
			} else {
				DamageBar.Value = Value;
			}
		}

		private void OnTimerTimeout() {
			DamageBar.Value = Value;
		}

		public void Init( float nHealth ) {
			Timer = GetNode<Timer>( "Timer" );
			Timer.SetProcess( false );
			Timer.SetProcessInternal( false );

			DamageBar = GetNode<ProgressBar>( "DamageBar" );
			DamageBar.SetProcess( false );
			DamageBar.SetProcessInternal( false );

			MaxValue = nHealth;
			Value = nHealth;
			DamageBar.MaxValue = nHealth;
			DamageBar.Value = nHealth;

			Timer.Connect( "timeout", Callable.From( OnTimerTimeout ) );
		}
	};
};