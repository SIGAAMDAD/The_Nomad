using Godot;
using System.Collections.Generic;
using Renown.Thinkers;

namespace Renown.World {
	public partial class Marketplace : Area2D {
		[Export]
		private MarketplaceSlot[] TradingSpaces;

		private List<Merchant> ActiveMerchants = null;
		
		public List<Merchant> GetActiveMerchants() => ActiveMerchants;
		public MarketplaceSlot GetFreeTradingSpace() {
			for ( int i = 0; i < TradingSpaces.Length; i++ ) {
				if ( !TradingSpaces[i].IsTaken() ) {
					return TradingSpaces[i];
				}
			}
			return null;
		}
		
		private void OnInteractionBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Entity entity && entity != null ) {
				if ( entity is Merchant merchant && merchant != null ) {
					ActiveMerchants.Add( merchant );
				}
			}
		}
		private void OnInteractionBodyShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Entity entity && entity != null ) {
				if ( entity is Merchant merchant && merchant != null ) {
					ActiveMerchants.Remove( merchant );
				}
			}
		}
		
		public override void _Ready() {
			base._Ready();
			
			ActiveMerchants = new List<Merchant>();
			
			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionBodyShape2DEntered ) );
			Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionBodyShape2DExited ) );
		}
	};
};
