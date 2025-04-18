using Godot;
using Renown.Thinkers;

namespace Renown.World {
	public partial class MarketplaceSlot : Node2D {
		[Export]
		private Merchant User = null;
		
		public bool IsTaken() => User != null;
		public void SetUser( Merchant user ) => User = user;
	};
};
