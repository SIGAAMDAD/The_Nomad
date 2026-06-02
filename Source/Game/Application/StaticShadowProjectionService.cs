using Godot;
using System;
using System.Collections.Generic;

public sealed class StaticShadowProjectionService
{
	public Vector2[] ProjectPolygon(
		Vector2[] sourceLocalPolygon,
		Transform2D sourceGlobalTransform,
		Transform2D receiverGlobalTransform,
		float casterHeightPx,
		Vector2 shadowDirection,
		float lengthPerHeightPixel
	)
	{
		if ( sourceLocalPolygon == null || sourceLocalPolygon.Length < 3 ) {
			return Array.Empty<Vector2>();
		}

		Vector2 dir = shadowDirection.LengthSquared() > 0.0001f
			? shadowDirection.Normalized()
			: Vector2.Right;

		Vector2 shadowOffset = dir * Mathf.Max( 0.0f, casterHeightPx ) * lengthPerHeightPixel;

		var worldPoints = new List<Vector2>( sourceLocalPolygon.Length * 2 );

		foreach ( Vector2 localPoint in sourceLocalPolygon ) {
			Vector2 worldPoint = sourceGlobalTransform * localPoint;

			worldPoints.Add( worldPoint );
			worldPoints.Add( worldPoint + shadowOffset );
		}

		List<Vector2> hullWorld = BuildConvexHull( worldPoints );

		Transform2D receiverInverse = receiverGlobalTransform.AffineInverse();

		var result = new Vector2[hullWorld.Count];

		for ( int i = 0; i < hullWorld.Count; i++ ) {
			result[i] = receiverInverse * hullWorld[i];
		}

		return result;
	}

	private static List<Vector2> BuildConvexHull( List<Vector2> points )
	{
		points.Sort( ( a, b ) => {
			int x = a.X.CompareTo( b.X );
			return x != 0 ? x : a.Y.CompareTo( b.Y );
		} );

		var unique = new List<Vector2>( points.Count );

		foreach ( Vector2 p in points ) {
			if ( unique.Count == 0 || p.DistanceSquaredTo( unique[^1] ) > 0.0001f ) {
				unique.Add( p );
			}
		}

		if ( unique.Count <= 2 ) {
			return unique;
		}

		var lower = new List<Vector2>();

		foreach ( Vector2 p in unique ) {
			while ( lower.Count >= 2 && Cross( lower[^2], lower[^1], p ) <= 0.0f ) {
				lower.RemoveAt( lower.Count - 1 );
			}

			lower.Add( p );
		}

		var upper = new List<Vector2>();

		for ( int i = unique.Count - 1; i >= 0; i-- ) {
			Vector2 p = unique[i];

			while ( upper.Count >= 2 && Cross( upper[^2], upper[^1], p ) <= 0.0f ) {
				upper.RemoveAt( upper.Count - 1 );
			}

			upper.Add( p );
		}

		lower.RemoveAt( lower.Count - 1 );
		upper.RemoveAt( upper.Count - 1 );

		lower.AddRange( upper );
		return lower;
	}

	private static float Cross( Vector2 origin, Vector2 a, Vector2 b )
	{
		Vector2 oa = a - origin;
		Vector2 ob = b - origin;

		return oa.X * ob.Y - oa.Y * ob.X;
	}
}
