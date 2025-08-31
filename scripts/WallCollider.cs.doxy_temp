using Godot;
using Renown;
using System;

public partial class WallCollider : StaticBody2D {
	private void OnArea2DBodyShapeEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			if ( entity.Velocity.Length() > Constants.DAMAGE_VELOCITY ) {
				// THUMP!
			}
		}
	}
};
