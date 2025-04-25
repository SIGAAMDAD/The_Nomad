using Godot;

namespace PlayerSystem {
	public partial class RageBar : ProgressBar {
		private Timer ShowTimer;

		public float Rage {
			set {
				ProcessMode = ProcessModeEnum.Pausable;
				Show();
				ShowTimer.Start();
				Value = value;
			}
		}

		public void Init( float nRage ) {
			MaxValue = nRage;
			Value = nRage;

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
		}
	};
};