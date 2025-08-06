using Godot;

namespace PlayerSystem.Upgrades {
	public struct CoolingVents : DashModule {
		public readonly StringName Name => "Cooling Vents";
		public readonly StringName Description => "Enhanced cooling mechanisms for the jumpkit.";

		public void ApplyEffect( DashKit dashKit ) {
		}
	};
};