using Godot;

namespace Renown.World {
	public partial class MarketplaceSlot : Node2D {
		[Export]
		private Thinker User = null;
		
		public bool IsTaken() => User != null;
		public void SetUser( Thinker user ) => User = user;
	};
};
