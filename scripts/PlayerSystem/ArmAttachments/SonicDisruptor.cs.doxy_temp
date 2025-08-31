using Godot;

namespace PlayerSystem.ArmAttachments {
	public partial class SonicDisruptor : ArmAttachment {
		private bool Usable = true;

		public override void Use() {
			if ( !Usable ) {
				return;
			}
			Usable = false;
			CooldownTimer.Start();
		}

		private void OnCooldownTimerTimeout() {
			Usable = true;
		}

		public override void _Ready() {
			base._Ready();

			CooldownTimer = GetNode<Timer>( "CooldownTimer" );
			CooldownTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnCooldownTimerTimeout ) );
		}
	};
};