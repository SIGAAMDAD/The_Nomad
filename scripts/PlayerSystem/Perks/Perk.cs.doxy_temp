using Godot;

namespace PlayerSystem.Perks {
	public abstract partial class Perk : Resource {
		[Export]
		protected Texture2D Icon;
		[Export]
		public StringName Name { get; protected set; }
		[Export]
		public StringName Description { get; protected set; }
		
		protected Player User;

		public Perk( Player user ) {
			User = user;
		}

		public abstract void Connect();
		public abstract void Disconnect();
	};
};