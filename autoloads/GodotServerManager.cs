/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public struct RayIntersectionInfo {
	public GodotObject Collider;
	public Vector2 Position;

	public RayIntersectionInfo() {
		Collider = null;
		Position = Vector2.Zero;
	}
};

/*
===================================================================================

GodotServerManager

===================================================================================
*/
/// <summary>
/// 
/// </summary>

[GlobalClass]
public partial class GodotServerManager : Node {
	private static ConcurrentBag<PhysicsRayQueryParameters2D> RayQueryCache;
	private static PhysicsDirectSpaceState2D SpaceState2D;
	private static Rid GlobalNavigationMap;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Rid GetNavigationMap() => GlobalNavigationMap;

	/*
	===============
	CheckRayCast
	===============
	*/
	public static RayIntersectionInfo CheckRayCast( Vector2 position, float angle, float distance, Rid caster ) {
		if ( !RayQueryCache.TryTake( out PhysicsRayQueryParameters2D rayQuery ) ) {
			rayQuery = new PhysicsRayQueryParameters2D();
		}
		rayQuery.From = position;
		rayQuery.To = new Vector2(
			position.X + distance * Mathf.Cos( angle ),
			position.Y + distance * Mathf.Sin( angle )
		);
		rayQuery.Exclude = [ caster ];
		rayQuery.CollisionMask = (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player | PhysicsLayer.SpecialHitboxes | PhysicsLayer.StaticGeometry );
		rayQuery.CollideWithAreas = true;
		rayQuery.CollideWithBodies = true;
		rayQuery.HitFromInside = false;

		Godot.Collections.Dictionary collision = SpaceState2D.IntersectRay( rayQuery );

		// return to the pool
		RayQueryCache.Add( rayQuery );

		RayIntersectionInfo info = new RayIntersectionInfo();

		if ( collision.Count > 0 ) {
			info.Collider = collision[ "collider" ].AsGodotObject();
			info.Position = collision[ "position" ].AsVector2();
			return info;
		}

		return info;
	}

	/*
	===============
	GetCollidingObjects
	===============
	*/
	public static List<GodotObject> GetCollidingObjects( Rid areaRid ) {
		PhysicsShapeQueryParameters2D shapeQuery = new PhysicsShapeQueryParameters2D();

		shapeQuery.CollideWithAreas = false;
		shapeQuery.CollideWithBodies = true;
		shapeQuery.Transform = PhysicsServer2D.AreaGetTransform( areaRid );
		shapeQuery.ShapeRid = PhysicsServer2D.AreaGetShape( areaRid, 0 );
		shapeQuery.CollisionMask = PhysicsServer2D.AreaGetCollisionMask( areaRid );

		Godot.Collections.Array<Godot.Collections.Dictionary> objects = SpaceState2D.IntersectShape( shapeQuery, 512 );
		List<GodotObject> collisions = new List<GodotObject>( objects.Count );
		for ( int i = 0; i < objects.Count; i++ ) {
			collisions.Add( objects[ i ][ "collider" ].AsGodotObject() );
		}

		return collisions;
	}

	/*
	===============
	InitServers
	===============
	*/
	public static void InitServers( Rid navigationMap ) {
		SpaceState2D = LevelData.Instance.GetWorld2D().DirectSpaceState;
		RayQueryCache = new ConcurrentBag<PhysicsRayQueryParameters2D>();
		GlobalNavigationMap = navigationMap;

		// activate servers
		PhysicsServer2D.SetActive( true );
		NavigationServer2D.MapSetActive( navigationMap, true );
	}
};