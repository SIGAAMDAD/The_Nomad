using Godot;
using Renown.Thinkers;

namespace Renown.World {
	public partial class TradeRoute : Road {
		protected override void OnProcessAreaEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Merchant merchant && merchant != null ) {
			}
		}
	};
};