using Godot;

namespace PlayerSystem.Upgrades {
	public sealed partial class CoolingVents : DashModule {
		public override StringName Name => "Cooling Vents";
		public override StringName Description => "Enhanced cooling mechanisms for the jumpkit.";

		public override void ApplyEffect( Player player ) {
		}
	};
};