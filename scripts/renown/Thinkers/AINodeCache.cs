using Godot;
using System.Collections.Generic;
using Renown.World;

namespace Renown.Thinkers {
	public partial class AINodeCache : Node {
		private List<Vector2> CoverPositions = new List<Vector2>();
		private NavigationRegion2D NavRegion;

		private void FindCoverNodes( Node node ) {
			Godot.Collections.Array<Node> children = node.GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				FindCoverNodes( children[ i ] );
				if ( children[ i ].IsInGroup( "CoverNodes" ) ) {
					CoverPositions.Add( children[ i ].Get( "global_position" ).AsVector2() );
				}
			}
		}

		public List<Vector2> GetValidCoverPositions( Vector2 from, Vector2 target ) {
			List<Vector2> output = new List<Vector2>();

			Vector2[] path = NavigationServer2D.MapGetPath( NavRegion.GetWorld2D().NavigationMap, from, target, true );
			for ( int i = 0; i < CoverPositions.Count; i++ ) {
				if ( CoverPositions[ i ].DistanceTo( path[ ^1 ] ) < 300.0f ) {
					// near the end of the path
					output.Add( CoverPositions[ i ] );
				}
			}

			return output;
		}

		public override void _Ready() {
			base._Ready();

			if ( GetParent() is WorldArea area && area != null ) {
				NavRegion = LevelData.Instance.GetNode<NavigationRegion2D>( "GlobalNavMesh" );
				FindCoverNodes( area );
			} else {
				Console.PrintWarning( "AINodeCache._Ready: the parent node of an AINodeCache must be a WorldArea!" );
				return;
			}
		}
	};
};