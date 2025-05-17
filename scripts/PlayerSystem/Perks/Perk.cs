using Godot;

namespace PlayerSystem.Perks {
	public abstract partial class Perk : Resource {
		[Export]
		protected Texture2D Icon;
		[Export]
		protected StringName Name;
		[Export]
		protected StringName Description;
		
		protected Player User;

		public Perk( Player user ) {
			User = user;
		}

		public abstract void Connect();
		public abstract void Disconnect();
	};
};