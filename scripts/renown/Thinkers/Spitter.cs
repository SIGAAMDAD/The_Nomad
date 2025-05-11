using System;
using Godot;

namespace Renown.Thinkers {
	public partial class Spitter : Thinker {
		private enum State : byte {
			Idle,
			Chasing,
			Attacking,
			
			Count
		};

		private Entity Target;
		private State CurrentState = State.Idle;
		private Timer AttackTimer;

		public override void _Ready() {
			base._Ready();

			Target = Hellbreaker.ThisPlayer;

			AttackTimer = new Timer();
			AttackTimer.Name = "AttackTimer";
			AttackTimer.WaitTime = 1.5f;
			AttackTimer.OneShot = true;
			AttackTimer.Connect( "timeout", Callable.From( OnAttackTimerTimeout ) );
			AddChild( AttackTimer );
		}

		private void OnAttackTimerTimeout() {

		}

		public override void Damage( Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );

			if ( Health <= 0.0f ) {
				Flags |= ThinkerFlags.Dead;

				BodyAnimations.CallDeferred( "play", "dead" );
			}

			BodyAnimations.CallDeferred( "play", "hurt" );
		}

		protected override void ProcessAnimations() {
			if ( !Visible || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( Velocity.X > 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", false );
			} else if ( Velocity.X < 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", true );
			}

			if ( CurrentState != State.Attacking ) {
				if ( Velocity != Godot.Vector2.Zero ) {
					BodyAnimations.CallDeferred( "play", "move" );
				} else {
					BodyAnimations.CallDeferred( "play", "idle" );
				}
			}
		}
		protected override void Think() {
			if ( !Visible ) {
				return;
			}

			if ( Hellbreaker.Active ) {
				switch ( CurrentState ) {
				case State.Idle:
					if ( GlobalPosition.DistanceTo( Target.GlobalPosition ) > 128.0f ) {
						SetNavigationTarget( Target.GlobalPosition );
					} else {
						CurrentState = State.Attacking;
					}
					break;
				case State.Attacking:
					BodyAnimations.CallDeferred( "play", "attack" );
					break;
				};
			}
		}
	};
};