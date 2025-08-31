using Godot;
using Renown;
using System;

namespace PlayerSystem.ArmAttachments {
	public partial class GrapplingHook : ArmAttachment {
		private enum State : byte {
			Ready,
			Shooting,
			PullingPlayer,
			PullingObject,
			Reeling,

			Cooling,
		};

		private static readonly float Gravity = 1200.0f;
		private static readonly float HookSpeed = 1900.0f;
		private static readonly float ReturnSpeed = 1200.0f;
		private static readonly float RopeSagAmount = 2.0f;
		private static readonly int RopeSegments = 72;
		private static readonly float PlayerPullForce = 800.0f;
		private static readonly float ObjectPullForce = 1000.0f;

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
		private void Reel() {
			if ( Status == State.Ready || Status == State.Reeling ) {
				return;
			}
			Status = State.Reeling;
		}
		private void UpdateShooting( float delta ) {
			float moveDistance = HookSpeed * delta;
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
		private void UpdatePlayerPull( float delta ) {
//			float angle = _Owner.GetArmAngle();
//			Direction = new Vector2( (float)Math.Cos( angle ), (float)Math.Sin( angle ) );

			_Owner.Velocity = Direction * PlayerPullForce;
			_Owner.MoveAndSlide();

			float distance = _Owner.GlobalPosition.DistanceTo( HookPosition );
			if ( distance < 90.0f ) {
				Reel();
			}
		}
		private void UpdateObjectPull( float delta ) {
			if ( HookedObject == null ) {
				Reel();
				return;
			}

			Vector2 direction = ( _Owner.GlobalPosition - HookedObject.GlobalPosition ).Normalized();

			if ( HookedObject is CharacterBody2D characterBody ) {
				characterBody.Velocity += direction * ObjectPullForce;
				characterBody.MoveAndSlide();
			}

			HookPosition = HookedObject.GlobalPosition;

			Latch.GlobalPosition = HookPosition;

			float distance = _Owner.GlobalPosition.DistanceTo( HookPosition );
			if ( distance < 90.0f ) {
				Reel();
			}
		}
		private void UpdateReeling( float delta ) {
			Vector2 direction = ( _Owner.GlobalPosition - HookPosition ).Normalized();
			HookPosition += direction * ReturnSpeed * delta;

			Latch.GlobalPosition = HookPosition;

			float distance = _Owner.GlobalPosition.DistanceTo( HookPosition );
			if ( distance < 90.0f ) {
				Rope.Visible = false;
				Status = State.Cooling;
				CooldownTimer.Start();
			}
		}

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
		private void UpdateRope() {
			Rope.ClearPoints();

			Vector2 hookPos = HookPosition;
			if ( Status == State.Reeling ) {
				hookPos = Latch.GlobalPosition;
			}

			float distance = _Owner.GlobalPosition.DistanceTo( hookPos );
			Vector2 start = ToLocal( _Owner.GlobalPosition + HookOffset );
			Vector2 end = ToLocal( hookPos );

			float dynamicSag = RopeSagAmount * ( distance / Length );
			for ( int i = 0; i <= RopeSegments; i++ ) {
				float t = i / (float)RopeSegments;
				Vector2 point = start.Lerp( end, t );

				float sagFactor = 4 * ( t * ( 1 - t ) );
				point.Y += sagFactor * dynamicSag;

				Rope.AddPoint( point );
			}
		}

		public override void _Ready() {
			base._Ready();

			_Owner = GetParent<Player>();

			Rope = GetNode<Line2D>( "Rope" );

			Latch = GetNode<Area2D>( "Hook" );
			Latch.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, bodyShapeIndex, localShapeIndex ) => OnHookHit( body ) ) );
			Latch.Connect( Area2D.SignalName.BodyEntered, Callable.From<Node2D>( OnHookHit ) );

			HookSprite = GetNode<Sprite2D>( "Hook/HookSprite" );

			Collision = GetNode<CollisionShape2D>( "Hook/CollisionShape2D" );

			CooldownTimer = GetNode<Timer>( "CooldownTimer" );
			CooldownTimer.Connect( Timer.SignalName.Timeout, Callable.From( () => {
				Status = State.Ready;
				SetPhysicsProcess( false );
			} ) );

			HookPosition = Latch.GlobalPosition;

			Status = State.Ready;

			SetPhysicsProcess( false );
		}
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
			};

			UpdateRope();
		}
	};
};