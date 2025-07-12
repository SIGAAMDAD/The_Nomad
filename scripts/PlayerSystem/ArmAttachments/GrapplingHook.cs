using Godot;

namespace PlayerSystem.ArmAttachments {
	public partial class GrapplingHook : ArmAttachment {
		[Export]
		private float ;

		private Line2D Line;

		public override void _Ready() {
			base._Ready();
		}
	};
};