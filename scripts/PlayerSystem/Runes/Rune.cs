using Godot;

namespace PlayerSystem.Runes {
	public abstract partial class Rune : Resource {
		[Export]
		public StringName Name { get; private set; }
		[Export]
		public Texture2D Icon { get; private set; }

		protected Player Owner;

		public abstract void Connect( Player user );
		public abstract void Activate();
		public abstract void Disconnect();
	};
};