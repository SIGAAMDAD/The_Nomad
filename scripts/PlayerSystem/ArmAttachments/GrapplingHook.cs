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

namespace PlayerSystem.ArmAttachments {
	/*
	===================================================================================
	
	GrapplingHook
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class GrapplingHook : ArmAttachment {
		private enum State : byte {
			Ready,
			Shooting,
			PullingPlayer,
			PullingObject,
			Reeling,

			Cooling,
		};

		private static readonly float GRAVITY = 1200.0f;
		private static readonly float HOOK_SPEED = 1900.0f;
		private static readonly float RETURN_SPEED = 1200.0f;
		private static readonly float ROPE_SAG_AMOUNT = 2.0f;
		private static readonly int ROPE_SEGMENTS = 72;
		private static readonly float PLAYER_PULL_FORCE = 800.0f;
		private static readonly float OBJECT_PULL_FORE = 1000.0f;

		[Export]
		private float Length = 800.0f;

		private Vector2 HookOffset = new Vector2( 0.0f, -12.0f );
		private Line2D Rope;
		private State Status;
		private float Speed = 0.0f;

		private Area2D Latch;
		private Vector2 HookPosition;
		private float HookDistance = 0.0f;

		private Sprite2D HookSprite;
		private PhysicsBody2D HookedObject;

		private Vector2 Direction;
		private Vector2 StartPosition;

		private CollisionShape2D Collision;

		/*
		===============
		Use
		===============
		*/
		public override void Use() {
			if ( Status != State.Ready ) {
				if ( Status == State.Shooting || Status == State.PullingPlayer || Status == State.PullingObject ) {
					SetPhysicsProcess( true );
					Reel();
				}
				return;
			}

			float angle = _Owner.GetArmAngle();
			Direction = new Vector2( (float)Math.Cos( angle ), (float)Math.Sin( angle ) );

			HookPosition = _Owner.GlobalPosition + HookOffset;
			HookDistance = 0.0f;
			HookSprite.Visible = true;
			HookSprite.GlobalRotation = angle;
			Latch.GlobalPosition = HookPosition;

			Rope.Visible = true;
			Status = State.Shooting;

			SetPhysicsProcess( true );
		}

		/*
		===============
		Reel
		===============
		*/
		private void Reel() {
			if ( Status == State.Ready || Status == State.Reeling ) {
				return;
			}
			Status = State.Reeling;
		}

		/*
		===============
		UpdateShooting
		===============
		*/
		private void UpdateShooting( float delta ) {
			float moveDistance = HOOK_SPEED * delta;
			//			float angle = _Owner.GetArmAngle();
			//			Direction = new Vector2( (float)Math.Cos( angle ), (float)Math.Sin( angle ) );

			HookPosition += Direction * moveDistance;
			HookDistance += moveDistance;
			Latch.GlobalPosition = HookPosition;

			if ( HookDistance > Length ) {
				Reel();
				return;
			}
		}

		/*
		===============
		UpdatePlayerPull
		===============
		*/
		private void UpdatePlayerPull( float delta ) {
			//			float angle = _Owner.GetArmAngle();
			//			Direction = new Vector2( (float)Math.Cos( angle ), (float)Math.Sin( angle ) );

			_Owner.Velocity = Direction * PLAYER_PULL_FORCE;
			_Owner.MoveAndSlide();

			float distance = _Owner.GlobalPosition.DistanceTo( HookPosition );
			if ( distance < 90.0f ) {
				Reel();
			}
		}

		/*
		===============
		UpdateObjectPull
		===============
		*/
		private void UpdateObjectPull( float delta ) {
			if ( HookedObject == null ) {
				Reel();
				return;
			}

			Vector2 direction = ( _Owner.GlobalPosition - HookedObject.GlobalPosition ).Normalized();

			if ( HookedObject is CharacterBody2D characterBody ) {
				characterBody.Velocity += direction * OBJECT_PULL_FORE;
				characterBody.MoveAndSlide();
			}

			HookPosition = HookedObject.GlobalPosition;

			Latch.GlobalPosition = HookPosition;

			float distance = _Owner.GlobalPosition.DistanceTo( HookPosition );
			if ( distance < 90.0f ) {
				Reel();
			}
		}

		/*
		===============
		UpdateReeling
		===============
		*/
		private void UpdateReeling( float delta ) {
			Vector2 direction = ( _Owner.GlobalPosition - HookPosition ).Normalized();
			HookPosition += direction * RETURN_SPEED * delta;

			Latch.GlobalPosition = HookPosition;

			float distance = _Owner.GlobalPosition.DistanceTo( HookPosition );
			if ( distance < 90.0f ) {
				Rope.Visible = false;
				Status = State.Cooling;
				CooldownTimer.Start();
			}
		}

		/*
		===============
		OnHookHit
		===============
		*/
		private void OnHookHit( Node2D body ) {
			if ( body is Entity entity && entity != null ) {
				Status = State.PullingObject;
				HookedObject = entity;
			} else if ( body is StaticBody2D ) {
				Status = State.PullingPlayer;
			} else {
				// invalid grippy
				Reel();
			}
		}

		/*
		===============
		UpdateRope
		===============
		*/
		private void UpdateRope() {
			Rope.ClearPoints();

			Vector2 hookPos = HookPosition;
			if ( Status == State.Reeling ) {
				hookPos = Latch.GlobalPosition;
			}

			float distance = _Owner.GlobalPosition.DistanceTo( hookPos );
			Vector2 start = ToLocal( _Owner.GlobalPosition + HookOffset );
			Vector2 end = ToLocal( hookPos );

			float dynamicSag = ROPE_SAG_AMOUNT * ( distance / Length );
			for ( int i = 0; i <= ROPE_SEGMENTS; i++ ) {
				float t = i / (float)ROPE_SEGMENTS;
				Vector2 point = start.Lerp( end, t );

				float sagFactor = 4 * ( t * ( 1 - t ) );
				point.Y += sagFactor * dynamicSag;

				Rope.AddPoint( point );
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			_Owner = GetParent<Player>();

			Rope = GetNode<Line2D>( "Rope" );

			Latch = GetNode<Area2D>( "Hook" );
			GameEventBus.ConnectSignal( Latch, Area2D.SignalName.BodyShapeEntered, this, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, bodyShapeIndex, localShapeIndex ) => OnHookHit( body ) ) );
			GameEventBus.ConnectSignal( Latch, Area2D.SignalName.BodyEntered, this, Callable.From<Node2D>( OnHookHit ) );

			HookSprite = GetNode<Sprite2D>( "Hook/HookSprite" );

			Collision = GetNode<CollisionShape2D>( "Hook/CollisionShape2D" );

			CooldownTimer = GetNode<Timer>( "CooldownTimer" );
			GameEventBus.ConnectSignal( CooldownTimer, Timer.SignalName.Timeout, this, Callable.From( () => {
				Status = State.Ready;
				SetPhysicsProcess( false );
			} ) );

			HookPosition = Latch.GlobalPosition;

			Status = State.Ready;

			SetPhysicsProcess( false );
		}

		/*
		===============
		_PhysicsProcess
		===============
		*/
		public override void _PhysicsProcess( double delta ) {
			switch ( Status ) {
				case State.Shooting:
					UpdateShooting( (float)delta );
					break;
				case State.PullingPlayer:
					UpdatePlayerPull( (float)delta );
					break;
				case State.PullingObject:
					UpdateObjectPull( (float)delta );
					break;
				case State.Reeling:
					UpdateReeling( (float)delta );
					break;
			}

			UpdateRope();
		}
	};
};