using Godot;

namespace PlayerSystem {
	public partial class RageBar : ProgressBar {
		public void Init( float nRage ) {
			MaxValue = nRage;
			Value = nRage;
		}
	};
};