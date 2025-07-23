using Godot;

namespace PlayerSystem.Upgrades {
	public abstract class DashModule {
		public abstract StringName Name { get; }
		public abstract StringName Description { get; }

		public abstract void ApplyEffect( Player player );
	};
};