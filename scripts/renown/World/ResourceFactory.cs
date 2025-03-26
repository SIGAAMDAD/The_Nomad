using Godot;

namespace Renown.World {
	public partial class ResourceFactory : Node2D {
		[Export]
		public Settlement Location = null;
		[Export]
		public uint Reserve = 0;
		
		public enum Type : uint {
			Bullets,
			Guns,
			Ale,
			Rations,
			Tobacco,
			Cocaine,
			
			Count
		};
		
		private Type ResourceType;
		private uint Stock = 0;
	};
};