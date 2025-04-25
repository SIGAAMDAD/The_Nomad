using Godot;

namespace PlayerSystem {
	public partial class RageBar : ProgressBar {
		private Timer ShowTimer;

		public float Rage {
			set {
				ShowTimer.Start();
				ProcessMode = ProcessModeEnum.Pausable;
				Value = value;
			}
		}

		public void Init( float nRage ) {
			MaxValue = nRage;
			Value = nRage;

			ShowTimer = ;
		}
	};
};