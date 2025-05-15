using Godot;

namespace PlayerSystem {
	public partial class RageBar : ProgressBar {
		private Timer ShowTimer;

		public float Rage {
			set {
				ProcessMode = ProcessModeEnum.Pausable;
				Modulate = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
				ShowTimer.Start();
				Value = value;
			}
		}

		public void Init( float nRage ) {
			MaxValue = nRage;
			Value = nRage;

			ShowTimer = new Timer();
			ShowTimer.Name = "ShowTimer";
			ShowTimer.OneShot = true;
			ShowTimer.WaitTime = 4.5f;
			ShowTimer.Connect( "timeout", Callable.From(
				() => {
					Tween Tweener = CreateTween();
					Tweener.TweenProperty( this, "modulate", new Color( 0.0f, 0.0f, 0.0f, 0.0f ), 1.0f );
					Tweener.Connect( "finished", Callable.From( () => { ProcessMode = ProcessModeEnum.Disabled; } ) );
				}
			) );
			AddChild( ShowTimer );
		}
	};
};