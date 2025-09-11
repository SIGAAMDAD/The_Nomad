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
using Renown;
using System;

namespace PlayerSystem {
	/*
	===================================================================================
	
	AimAssist
	
	===================================================================================
	*/

	public partial class AimAssist : Node2D {
		private static readonly Color AIMING_AT_TARGET = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
		private static readonly Color AIMING_AT_NULL = new Color( 0.5f, 0.5f, 0.0f, 1.0f );

		[Export]
		public float ConeAngle = 45.0f;
		[Export]
		public float DetectionRadius = 500.0f;
		[Export]
		public float AimSnapStrength = 0.4f;

		private Area2D? DetectionArea;

		private Line2D AimLine;
		private Player Player;

		public AimAssist( Player? owner ) {
			ArgumentNullException.ThrowIfNull( owner );

			Player = owner;

			AimLine = GetNode<Line2D>( "AimLine" );
			AimLine.Points[ 1 ].X = AimLine.Points[ 0 ].X * ( Player.ScreenSize.X / 2.0f );
		}

		/*
		===============
		GetAssistedAimDirection
		===============
		*/
		public Vector2 GetAssistedAimDirection( in Vector2 originalAimDirection ) {
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
				return originalAimDirection.Lerp(
					( bestTarget.GlobalPosition - playerPosition ).Normalized(),
					AimSnapStrength
				).Normalized();
			}

			return originalAimDirection;
		}

		/*
		===============
		SetupDetectionArea
		===============
		*/
		private void SetupDetectionArea() {
			CollisionShape2D DetectionShape = new CollisionShape2D() {
				Shape = new CircleShape2D() {
					Radius = DetectionRadius
				}
			};

			DetectionArea = new Area2D() {
				CollisionLayer = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity | PhysicsLayer.SpecialHitboxes ),
				CollisionMask = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity | PhysicsLayer.SpecialHitboxes ),
				Monitoring = true
			};
			DetectionArea.AddChild( DetectionShape );
			AddChild( DetectionArea );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			SetupDetectionArea();
		}

		/*
		===============
		_PhysicsProcess
		===============
		*/
		public override void _PhysicsProcess( double delta ) {
			base._PhysicsProcess( delta );

			RayIntersectionInfo collision = GodotServerManager.CheckRayCast( GlobalPosition, Player.ArmAngle, Player.ScreenSize.X * 0.5f, Player.GetRid() );
			if ( collision.Collider is Entity entity && entity != null && entity.Faction != Player.Faction ) {
				AimLine.DefaultColor = AIMING_AT_TARGET;
			} else if ( collision.Collider is Hitbox hitbox && hitbox != null && ( (Node2D)hitbox.GetMeta( "Owner" ) as Entity ).Faction != Player.Faction ) {
				AimLine.DefaultColor = AIMING_AT_TARGET;
			} else {
				AimLine.DefaultColor = AIMING_AT_NULL;
			}
		}
	};
};