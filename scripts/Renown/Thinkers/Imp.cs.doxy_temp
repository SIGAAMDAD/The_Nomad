using Godot;
using System.Diagnostics;

namespace Renown.Thinkers {
	public partial class Imp : Thinker {
		private Timer AttackTimer;
		private Entity Target;

		private Area2D MeleeArea;

		/*
		private AnimatedSprite2D Projectile;
		private Godot.Vector2 ProjectileDirection;

		private void OnProjectileAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player entity && entity != null ) {
				// nothing too serious
				entity.Damage( this, 6.0f );
//				entity.AddStatusEffect( "status_burning" );
			}
		}

		private void OnProjectAnimationFinished() {
			switch ( Projectile.Animation ) {
			case "throw":
				Projectile.CallDeferred( "play", "fly" );
				break;
			case "hit":
				Projectile.CallDeferred( "queue_free" );
				break;
			};
		}
		*/

		public override void Damage( in Entity source, float nAmount ) {
			base.Damage( source, nAmount );
			if ( Health <= 0.0f ) {
				Flags |= ThinkerFlags.Dead;
				return;
			}
		}

		private void OnAttackTimerTimeout() {
			Godot.Collections.Array<Node2D> entities = MeleeArea.GetOverlappingBodies();

			for ( int i = 0; i < entities.Count; i++ ) {
				if ( entities[i] is Player player && player != null ) {
					player.CallDeferred( "Damage", this, 8.0f );
				}
			}
		}

		private void OnHellbreakerFinished() {
			ProcessMode = ProcessModeEnum.Disabled;
			NavAgent.ProcessMode = ProcessModeEnum.Disabled;
			AudioChannel.ProcessMode = ProcessModeEnum.Disabled;
		}
		private void OnHellbreakerBegin() {
			ProcessMode = ProcessModeEnum.Pausable;
			NavAgent.ProcessMode = ProcessModeEnum.Pausable;
			AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
		}

		public override void _Ready() {
			base._Ready();

			if ( Hellbreaker.Active ) {
				LevelData.Instance.HellbreakerFinished += OnHellbreakerFinished;
				LevelData.Instance.HellbreakerBegin += OnHellbreakerBegin;
			}

			AttackTimer = new Timer();
			AttackTimer.Name = "AttackTimer";
			AttackTimer.OneShot = true;
			AttackTimer.WaitTime = 1.0f;
			AttackTimer.Connect( "timeout", Callable.From( OnAttackTimerTimeout ) );
			AddChild( AttackTimer );

			MeleeArea = GetNode<Area2D>( "Area2D" );

			Target = Hellbreaker.ThisPlayer;
		}
		/*
		public override void _PhysicsProcess( double delta ) {
			base._PhysicsProcess( delta );

			if ( Projectile != null ) {
				if ( Projectile.Animation == "fly" ) {
					Projectile.SetDeferred( "global_position", Projectile.GlobalPosition + ProjectileDirection * 80.0f * (float)GetPhysicsProcessDeltaTime() );
				}
			}
		}
		*/

		protected override void ProcessAnimations() {
			if ( Velocity.X < 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", true );
			} else {
				BodyAnimations.SetDeferred( "flip_h", false );
			}
			
			if ( AttackTimer.TimeLeft == 0.0f ) {
				if ( Velocity != Godot.Vector2.Zero ) {
					BodyAnimations.CallDeferred( "play", "move" );
				} else {
					BodyAnimations.CallDeferred( "play", "idle" );
				}
			}
		}
		/*
		private void SpawnFireball() {
			Projectile = ResourceCache.GetScene( "res://scenes/effects/fireball.tscn" ).Instantiate<AnimatedSprite2D>();
			Projectile.CallDeferred( "connect", "animation_finished", Callable.From( OnProjectAnimationFinished ) );

			ProjectileDirection = GlobalPosition.DirectionTo( Target.GlobalPosition );
			Projectile.SetDeferred( "global_rotation", Mathf.Atan2( ProjectileDirection.Y, ProjectileDirection.X ) );

			Projectile.CallDeferred( "play", "throw" );

			GetTree().CurrentScene.CallDeferred( "add_child", Projectile );
		}
		*/
		protected override void Think() {
			if ( !Visible || !Hellbreaker.Active ) {
				SetDeferred( "process_mode", (long)ProcessModeEnum.Disabled );
				return;
			} else {
				SetDeferred( "process_mode", (long)ProcessModeEnum.Pausable );
			}

			if ( Hellbreaker.Active ) {
				if ( AttackTimer.IsStopped() && Target.GlobalPosition.DistanceTo( GlobalPosition ) < 80.0f ) {
					SetDeferred( "velocity", Godot.Vector2.Zero );

					AttackTimer.CallDeferred( "start" );
					BodyAnimations.CallDeferred( "play", "attack" );
				} else if ( Target.GlobalPosition.DistanceTo( GlobalPosition ) > 40.0f ) {
					SetNavigationTarget( Target.GlobalPosition );
				}
			}
		}
	};
};