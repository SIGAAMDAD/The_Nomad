using Godot;

namespace PlayerSystem.Perks {
	public abstract partial class Perk : Resource {
		[Export]
		protected Texture2D Icon;
		[Export]
		public StringName Name { get; private set; }
		[Export]
		public StringName Description { get; private set; }
		
		protected Player User;

		public Perk( Player user ) {
			User = user;
		}

		public abstract void Connect();
		public abstract void Disconnect();
	};
};