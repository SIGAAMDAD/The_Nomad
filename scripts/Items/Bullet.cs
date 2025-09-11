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

namespace Items {
	/*
	===================================================================================
	
	Bullet
	
	===================================================================================
	*/

	public partial class Bullet : Area2D {
		private readonly Ammo? Ammo = null;
		private readonly Vector2 StartingPosition = Vector2.Zero;
		private readonly float HitscanRange = 0.0f;

		public Bullet( float angle, Ammo? ammo ) {
			if ( !ammo.HasValue ) {
				throw new ArgumentNullException( nameof( ammo ) );
			} else if ( GetParent() is not Entity ) {
				throw new InvalidOperationException( "Bullet nodes must be children of Entity objects!" );
			}

			GlobalRotation = angle;
			Ammo = ammo;
			HitscanRange = Ammo.Value.Range / 4.0f;
			StartingPosition = GetParent<Node2D>().GlobalPosition;

			GroupProcessor.AddToPhysicsGroup( "Bullets", this );
		}

		/*
		===============
		CheckRayScanHit
		===============
		*/
		private void CheckRayScanHit() {
			RayIntersectionInfo collision = GodotServerManager.CheckRayCast( GlobalPosition, GlobalRotation, HitscanRange, GetParent<Entity>().GetRid() );
			if ( collision.Collider is Area2D parryBox ) {

			}
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();
		}

		/*
		===============
		PhysicsUpdate
		===============
		*/
		public void PhysicsUpdate( float delta ) {
			if ( GlobalPosition.DistanceTo( StartingPosition ) > HitscanRange ) {
				CheckRayScanHit();
				QueueFree();
				return;
			}

			Vector2 position = GlobalPosition;
			float rotation = GlobalRotation;

			position.X += Mathf.Cos( rotation ) + ( Ammo.Value.Velocity * delta );
			position.Y += Mathf.Sin( rotation ) + ( Ammo.Value.Velocity * delta );

			GlobalPosition = position;
		}
	};
};