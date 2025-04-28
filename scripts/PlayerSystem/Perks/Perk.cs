using Godot;

namespace PlayerSystem.Perks {
	public abstract class Perk {
		protected Texture2D Icon;
		protected StringName Name;
		protected StringName Description;
		protected Player User;

		public Perk( Player user ) {
			User = user;
		}

		public abstract void Connect();
		public abstract void Disconnect();
	};
};