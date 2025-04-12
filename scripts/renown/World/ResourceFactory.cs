using Godot;

namespace Renown.World {
	public partial class ResourceFactory : Node2D {
		[Export]
		private Settlement Location = null;
		[Export]
		private uint Reserve = 0;
		[Export]
		private ResourceType ResourceType;

		private uint Stock = 0;

		private Timer CraftTimer;

		[Signal]
		public delegate void NeedsResourcesEventHandler();

		public override void _Ready() {
			base._Ready();
		}
	};
};