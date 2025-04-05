using Godot;

public partial class AINodeCache : Node {
	[Export]
	private Node2D[] CoverCache;
	[Export]
	private NavigationLink2D[] PatrolRoutes;

	public Node2D FindClosestCover( Godot.Vector2 position, Godot.Vector2 target ) {
		Node2D node = null;
		float bestDistance = float.MaxValue;

		RayCast2D sightLineCheck = new RayCast2D();
		sightLineCheck.TargetPosition = target;

		for ( int i = 0; i < CoverCache.Length; ++i ) {
			Godot.Vector2 from = CoverCache[i].GlobalPosition;
			float dist = from.DistanceTo( position );

			sightLineCheck.GlobalPosition = from;
			if ( dist < bestDistance ) {
				if ( sightLineCheck.GetCollider() is TileMapLayer ) {
					continue; // a wall's in the way
				}
				node = CoverCache[i];
				bestDistance = dist;
			}
		}
		return node;
	}
	public AIPatrolRoute FindClosestRoute( Godot.Vector2 position ) {
		NavigationLink2D route = null;
		float bestDistance = float.MaxValue;

		for ( int i = 0; i < PatrolRoutes.Length; ++i ) {
			Godot.Vector2 from = PatrolRoutes[i].GlobalPosition;
			float dist = from.DistanceTo( position );

			if ( dist < bestDistance ) {
				route = PatrolRoutes[i];
				bestDistance = dist;
			}
		}
		return (AIPatrolRoute)route;
	}
};