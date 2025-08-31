/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using Renown;

namespace PlayerSystem {
	/*
	===================================================================================
	
	AimAssist
	
	manages aim-assist
	
	===================================================================================
	*/

	public partial class AimAssist : Node2D {
		[Export]
		public float ConeAngle = 45.0f;
		[Export]
		public float DetectionRadius = 500.0f;
		[Export]
		public float AimSnapStrength = 0.4f;

		private Area2D? DetectionArea;

		/*
		===============
		SetupDetectionArea
		===============
		*/
		private void SetupDetectionArea() {
			DetectionArea = new Area2D();
			DetectionArea.CollisionLayer = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity | PhysicsLayer.SpecialHitboxes ); // Set your enemy collision layer
			DetectionArea.CollisionMask = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity | PhysicsLayer.SpecialHitboxes );
			DetectionArea.Monitoring = true;

			CircleShape2D circle = new CircleShape2D();
			circle.Radius = DetectionRadius;

			CollisionShape2D DetectionShape = new CollisionShape2D();
			DetectionShape.Shape = circle;

			DetectionArea.AddChild( DetectionShape );
			AddChild( DetectionArea );
		}

		/*
		===============
		GetAssistedAimDirection
		===============
		*/
		public Vector2 GetAssistedAimDirection( Vector2 originalAimDirection ) {
			Vector2 playerPosition = LevelData.Instance.ThisPlayer.GlobalPosition;
			Godot.Collections.Array<Node2D> targets = DetectionArea.GetOverlappingBodies();

			if ( targets.Count == 0 ) {
				return originalAimDirection;
			}

			Entity bestTarget = null;
			float bestScore = float.MinValue;

			for ( int i = 0; i < targets.Count; i++ ) {
				if ( targets[ i ] is Entity entity && entity != null ) {
					Vector2 directionToTarget = ( entity.GlobalPosition - playerPosition ).Normalized();
					float distance = playerPosition.DistanceTo( entity.GlobalPosition );

					float angleScore = 1.0f - Mathf.Abs( originalAimDirection.AngleTo( directionToTarget ) ) / Mathf.DegToRad( ConeAngle / 2.0f );
					if ( angleScore < 0.0f ) {
						continue;
					}

					float distanceScore = 1 - Mathf.Clamp( distance / DetectionRadius, 0.0f, 1.0f );
					float totalScore = angleScore * 0.7f + distanceScore * 0.3f;

					if ( totalScore > bestScore ) {
						bestScore = totalScore;
						bestTarget = entity;
					}
				}
			}

			if ( bestTarget != null ) {
				Vector2 targetDirection = ( bestTarget.GlobalPosition - playerPosition ).Normalized();
				return originalAimDirection.Lerp( targetDirection, AimSnapStrength ).Normalized();
			}

			return originalAimDirection;
		}
		
		/*
		===============
		_Ready

		godot initialization override
		===============
		*/
		public override void _Ready() {
			SetupDetectionArea();
		}
	};
};