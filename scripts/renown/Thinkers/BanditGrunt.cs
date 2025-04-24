/*
using System;
using Godot;

namespace Renown.Thinkers {
	public partial class BanditGrunt : MobBase {
		private static readonly float Range = 300.0f;

		private Timer TargetMovedTimer;
		private Timer AttackTimer;
		private Timer HealTimer;
		private Timer AimTimer;
		private RayCast2D AimLine;
		private Line2D ShootLine;
		private Godot.Vector2 GuardPosition;

		private AIPatrolRoute NextRoute;

		private bool Aiming = false;

		public override void Save() {
			base.Save();
		}
		public override void Load() {
			base.Load();
		}

		public override void Damage( Entity source, float nAmount ) {
			BloodParticleFactory.Create( source.GlobalPosition, GlobalPosition );

			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );

			PlaySound( AudioChannel, ResourceCache.Pain[ RandomFactory.Next( 0, ResourceCache.Pain.Length - 1 ) ] );

			if ( Health < 0.0f ) {
				LinearVelocity = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;
				HeadAnimations.Hide();
				ArmAnimations.Hide();
				BodyAnimations.Play( "die_high" );
				return;
			}
			if ( source.GetFaction() == Faction ) {
				// "CEASEFIRE!"
			}

			Awareness = AIAwareness.Alert;
			float angle = Randf( 0.0f, 360.0f );
			AimAngle = angle;
			LookAngle = angle;

			LastTargetPosition = source.GlobalPosition;
			PatrolRoute = null;

			if ( Awareness == AIAwareness.Alert ) {
				
			} else {
				Bark( BarkType.Alert );
			}
		}

		protected override void OnLoseInterestTimerTimeout() {
			AIState = AIState.Investigating;
			SightTarget = null;

			if ( Fear > 80 ) {
//				Bark( BarkType.Curse );
			}

			// once we've lost the target for a long period of time, resume patrol routes with a little more suspicion
//			PatrolRoute = NodeCache.FindClosestRoute( GlobalPosition );
			
//			AIPatrolRoute route = PatrolRoute as AIPatrolRoute;
//			if ( ( Squad as BanditGroup ).IsRouteOccupied( route ) ) {
//				PatrolRoute = route.GetNext();
//			}
//			SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );

			// a little more on edge
			Fear += 10;
		}

		private void OnTargetMoveTimerTimeout() {
			// "target's pinned!"
			Bark( BarkType.TargetPinned );
		}

		public override void Notify( GroupEvent nEventType, Thinker source ) {
			if ( source == this || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			switch ( nEventType ) {
			case GroupEvent.TargetChanged:
				SightTarget = ( source as MobBase ).GetSightTarget();
				LastTargetPosition = ( source as MobBase ).GetLastTargetPosition();
				Awareness = AIAwareness.Alert;
				AIState = AIState.Investigating;

				SetNavigationTarget( LastTargetPosition );
				Bark( BarkType.Curse );
				break;
			case GroupEvent.Count:
			default:
				return;
			};
		}

		protected override void SendPacket() {
			SyncObject.Write( GlobalPosition.X );
			SyncObject.Write( GlobalPosition.Y );
			SyncObject.Write( Health );
			SyncObject.Write( Fear );
			SyncObject.Write( (uint)State );
			SyncObject.Sync();
		}
		protected override void ReceivePacket( System.IO.BinaryReader reader ) {
			Godot.Vector2 position = Godot.Vector2.Zero;
			position.X = (float)reader.ReadDouble();
			position.Y = (float)reader.ReadDouble();
			GlobalPosition = position;
		}
	    public override void _Ready() {
			if ( Initialized ) {
				return;
			}
			Initialized = true;

			base._Ready();

			if ( SettingsData.GetNetworkingEnabled() ) {
				SyncObject = new NetworkWriter( 256 );
			}

			AttackTimer = new Timer();
			AttackTimer.WaitTime = 0.5f;
			AttackTimer.OneShot = true;
			AttackTimer.Connect( "timeout", Callable.From( () => { Aiming = false; } ) );
			AddChild( AttackTimer );

			TargetMovedTimer = new Timer();
			TargetMovedTimer.WaitTime = 10.0f;
			TargetMovedTimer.OneShot = true;
			TargetMovedTimer.Connect( "timeout", Callable.From( OnTargetMoveTimerTimeout ) );
			AddChild( TargetMovedTimer );

			Squad = GroupManager.GetGroup( GroupType.Bandit, Faction, GlobalPosition );
			Squad.AddThinker( this );

			GuardPosition = GlobalPosition;
			DemonEyeColor = new Color();

			AimTimer = new Timer();
			AimTimer.Name = "AimTimer";
			AimTimer.WaitTime = 1.0f;
			AimTimer.OneShot = true;
			AimTimer.Connect( "timeout", Callable.From( OnAimTimerTimeout ) );
			AddChild( AimTimer );

			AimLine = new RayCast2D();
			AimLine.Name = "AimLine";
			AimLine.TargetPosition = Godot.Vector2.Right * Range;
			AimLine.CollisionMask = 2 | 5;
			ArmAnimations.AddChild( AimLine );

			GuardPosition = GlobalPosition;
		}
		
		private void OnAimTimerTimeout() {
			// desert carbine
			AttackTimer.Start();
			
			if ( AimLine.IsColliding() ) {
				if ( AimLine.GetCollider() is Entity entity && entity != null ) {
					if ( entity.GetFaction() == Faction ) {
						Bark( BarkType.OutOfTheWay );
					} else {
						// FIXME:
						entity.Damage( this, 20.0f );
						PlaySound( AudioChannel, ResourceCache.GetSound( "res://sounds/weapons/desert_rifle_use.ogg" ) );
					}
				} else {
					DebrisFactory.Create( AimLine.GetCollisionPoint() );
					PlaySound( AudioChannel, ResourceCache.GetSound( "res://sounds/weapons/desert_rifle_use.ogg" ) );
				}
			}
		}
		protected override void ProcessAnimations()  {
			if ( SightTarget != null ) {
				LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
				AimAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
				LookAngle = AimAngle;
			}
			if ( Floor != null ) {
				if ( Floor.GetUpper() != null && Floor.GetUpper().GetPlayerStatus() ) {
					Visible = true;
				} else if ( Floor.IsInside() ) {
					Visible = Floor.GetPlayerStatus();
				}
			} else {
				Visible = true;
			}
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			ArmAnimations.GlobalRotation = AimAngle;
			HeadAnimations.GlobalRotation = LookAngle;

			if ( LookAngle > 225.0f ) {
				HeadAnimations.FlipV = true;
			} else if ( LookAngle < 135.0f ) {
				HeadAnimations.FlipV = false;
			}
			if ( AimAngle > 225.0f ) {
				ArmAnimations.FlipV = true;
			} else if ( AimAngle < 135.0f ) {
				ArmAnimations.FlipV = false;
			}
			if ( LinearVelocity.X > 0.0f ) {
				BodyAnimations.FlipH = false;
				ArmAnimations.FlipH = false;
			} else if ( LinearVelocity.X < 0.0f ) {
				BodyAnimations.FlipH = true;
				ArmAnimations.FlipH = true;
			}

			if ( LinearVelocity != Godot.Vector2.Zero ) {
				BodyAnimations.Play( "move" );
				ArmAnimations.Play( "move" );
				HeadAnimations.Play( "move" );
			} else {
				if ( Awareness == AIAwareness.Relaxed ) {
					BodyAnimations.Play( "calm" );
					ArmAnimations.Hide();
				} else {
					ArmAnimations.Show();
					HeadAnimations.Show();
					BodyAnimations.Play( "idle" );
					ArmAnimations.Play( "idle" );
					HeadAnimations.Play( "idle" );
				}
			}
		}
		protected override void Think( float delta ) {
			CheckSight( delta );

			if ( SightTarget != null ) {
				if ( CanSeeTarget && Awareness == AIAwareness.Alert ) {
					AIState = AIState.Attacking;
					ChangeInvestigateAngleTimer.Stop();
				} else {
					AIState = AIState.Investigating;
				}
			}
			if ( Aiming ) {
				MoveTimer.Stop();
				GotoPosition = GlobalPosition;
				LinearVelocity = Godot.Vector2.Zero;
			}
			if ( PatrolRoute != null && GlobalPosition.DistanceTo( PatrolRoute.GetGlobalEndPosition() ) < 10.0f ) {
				PatrolRoute = ( PatrolRoute as AIPatrolRoute ).GetNext();
				SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
			}

			switch ( AIState ) {
			case AIState.Investigating: {
				Investigate();
				// if we've got any suspicion, then start patrolling

//				if ( Fear > 0 && PatrolRoute == null ) {
//					PatrolRoute = NodeCache.FindClosestRoute( GlobalPosition );
//
//					// if the route is occupied, just pick a different one
//					AIPatrolRoute route = PatrolRoute as AIPatrolRoute;
//					if ( ( Squad as BanditGroup ).IsRouteOccupied( route ) ) {
//						PatrolRoute = route.GetNext();
//					}
//
//					State = AIState.PatrolStart;
//					SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
//				}
				if ( Fear > 80 && CanSeeTarget ) {
					Bark( BarkType.Curse );
//					OnAimTimerTimeout();
				}
				LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
				AimAngle = LookAngle;
				break; }
			case AIState.Attacking:
				if ( !CanSeeTarget ) {
					AIState = AIState.Investigating;
					break;
				}
				if ( AimTimer.TimeLeft > AimTimer.WaitTime * 0.25f ) {
					LookDir = GlobalPosition.DirectionTo( SightTarget.GlobalPosition );
					AimAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
					LookAngle = AimAngle;
					AimLine.GlobalRotation = AimAngle;
				}
				if ( AimTimer.TimeLeft > 0.0f && !CanSeeTarget ) {
					Bark( BarkType.TargetRunning );
				}
				if ( Aiming && !AimLine.IsColliding() ) {
					// running
					Bark( BarkType.TargetRunning );
				} else if ( Aiming && AimLine.GetCollider() is MobBase mob && mob != null ) {
					if ( mob.GetHealth() > 0.0f && mob.GetFaction() != Faction ) {
						Aiming = false;
						AimTimer.Stop();
						Bark( BarkType.OutOfTheWay );
					}
				}

				if ( ( GlobalPosition.DistanceTo( SightTarget.GlobalPosition ) < Range ) && AimLine.GetCollider() is Entity entity && entity == SightTarget && !Aiming ) {
					LinearVelocity = Godot.Vector2.Zero;
					GotoPosition = GlobalPosition;
					BodyAnimations.Play( "attack" );
					HeadAnimations.Play( "idle" );

					ArmAnimations.Play( "attack" );
					AimTimer.Start();
					Aiming = true;

					PlaySound( AudioChannel, ResourceCache.GetSound( "res://sounds/weapons/desert_rifle_reload.ogg" ) );
				}
				break;
			case AIState.Patrolling:
				if ( GlobalPosition.DistanceTo( GotoPosition ) < 10.0f ) {
					NextRoute = ( PatrolRoute as AIPatrolRoute ).GetNext();
					AIState = AIState.PatrolStart;
					SetNavigationTarget( NextRoute.GetGlobalStartPosition() );
				}
				break;
			case AIState.PatrolStart:
				if ( GlobalPosition.DistanceTo( PatrolRoute.GetGlobalStartPosition() ) < 10.0f ) {
					AIState = AIState.Patrolling;
					SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
				}
				break;
			case AIState.Guarding:
				if ( Awareness > AIAwareness.Relaxed ) {
					AIState = AIState.Investigating;
				}
				break;
			};
		}

		protected override void SetNavigationTarget( Godot.Vector2 target ) {
			NavAgent.TargetPosition = target;
			TargetReached = false;
			GotoPosition = target;
			if ( AIState == AIState.Patrolling && NextRoute != null ) {
				if ( NextRoute.GetGlobalStartPosition() == target ) {
					AIState = AIState.Patrolling;
				}
			}
		}
		protected override void OnTargetReached() {
			TargetReached = true;

			switch ( AIState ) {
			case AIState.Patrolling:
				PatrolRoute = NextRoute;
				PatrolRoute ??= NodeCache.FindClosestRoute( GlobalPosition );
				SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
				break;
			default:
				GotoPosition = GlobalPosition;
				LinearVelocity = Godot.Vector2.Zero;
				break;
			};
		}

		private void Investigate() {
			if ( ChangeInvestigateAngleTimer.IsStopped() ) {
				if ( Fear >= 60.0f ) {
					ChangeInvestigateAngleTimer.WaitTime = 0.6f;
				}
				ChangeInvestigateAngleTimer.Start();
			}
			if ( LoseInterestTimer.IsStopped() ) {
				LoseInterestTimer.Start();
			}
			if ( SightTarget != null && CanSeeTarget ) {
				AIState = AIState.Attacking;
				SetAlert( false );
				Squad.NotifyGroup( GroupEvent.TargetChanged, this );
			}
		}
	};
};
*/