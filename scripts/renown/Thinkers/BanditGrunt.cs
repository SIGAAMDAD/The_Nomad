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
			State = AIState.PatrolStart;
			SightTarget = null;
			Bark( BarkType.Curse );

			// once we've lost the target for a long period of time, resume patrol routes with a little more suspicion
			PatrolRoute = NodeCache.FindClosestRoute( GlobalPosition );
			
			AIPatrolRoute route = PatrolRoute as AIPatrolRoute;
			if ( ( Squad as BanditGroup ).IsRouteOccupied( route ) ) {
				PatrolRoute = route.GetNext();
			}
			SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );

			// a little more on edge
			Fear += 10;
		}

		private void OnTargetMoveTimerTimeout() {
			// "target's pinned!"
			Bark( BarkType.TargetPinned );
		}

		public override void Notify( GroupEvent nEventType, Thinker source ) {
			if ( source == this ) {
				return;
			}
			switch ( nEventType ) {
			case GroupEvent.TargetChanged:
				SightTarget = ( source as MobBase ).GetSightTarget();
				LastTargetPosition = ( source as MobBase ).GetLastTargetPosition();
				Awareness = AIAwareness.Alert;
				State = AIState.Investigating;

				SetNavigationTarget( LastTargetPosition );
				Bark( BarkType.Curse );
				break;
			case GroupEvent.Count:
			default:
				return;
			};
		}

		protected override void SendPacket() {
			SyncObject.Write( PhysicsPosition.X );
			SyncObject.Write( PhysicsPosition.Y );
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

			if ( GameConfiguration.GameDifficulty == GameDifficulty.Intended ) {
				DetectionMeter.Hide();
			}

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

			/*
			ShootLine = new Line2D();
			ShootLine.Name = "ShootLine";
			ShootLine.DefaultColor = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
			ShootLine.Hide();
			ArmAnimations.AddChild( ShootLine );
			*/

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
			if ( Floor != null ) {
				if ( Floor.GetUpper() != null && Floor.GetUpper().GetPlayerStatus() ) {
					Visible = true;
				} else if ( Floor.IsInside() ) {
					Visible = Floor.GetPlayerStatus();
				}
			} else {
				Visible = true;
			}

			if ( SightTarget != null ) {
				LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
				AimAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
				LookAngle = AimAngle;
			}

			ArmAnimations.GlobalRotation = AimAngle;
			HeadAnimations.GlobalRotation = LookAngle;

			if ( LookAngle > 0.0f ) {
				HeadAnimations.FlipV = true;
			} else if ( LookAngle < 0.0f ) {
				HeadAnimations.FlipV = false;
			}
			if ( AimAngle > 0.0f ) {
				ArmAnimations.FlipV = true;
			} else if ( AimAngle < 0.0f ) {
				ArmAnimations.FlipV = false;
			}
			if ( LinearVelocity.X > 0.0f ) {
				BodyAnimations.FlipH = false;
				ArmAnimations.FlipH = false;
			} else if ( LinearVelocity.X < 0.0f ) {
				BodyAnimations.FlipH = true;
				ArmAnimations.FlipH = true;
			}

			if ( Health <= 0.0f ) {
				return;
			}

			if ( LinearVelocity != Godot.Vector2.Zero ) {
				BodyAnimations.Play( "move" );
				ArmAnimations.Play( "move" );
				HeadAnimations.Play( "move" );
			} else {
				if ( Awareness == AIAwareness.Relaxed ) {
					BodyAnimations.Play( "calm" );
					ArmAnimations.Hide();
					HeadAnimations.Hide();
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
				if ( CanSeeTarget ) {
					State = AIState.Attacking;
					ChangeInvestigateAngleTimer.Stop();
				} else {
					State = AIState.Investigating;
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

			switch ( State ) {
			case AIState.Investigating: {
				Investigate();
				// if we've got any suspicion, then start patrolling

				if ( Fear > 0 && PatrolRoute == null ) {
					PatrolRoute = NodeCache.FindClosestRoute( GlobalPosition );

					// if the route is occupied, just pick a different one
					AIPatrolRoute route = PatrolRoute as AIPatrolRoute;
					if ( ( Squad as BanditGroup ).IsRouteOccupied( route ) ) {
						PatrolRoute = route.GetNext();
					}

					State = AIState.PatrolStart;
					SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
				}
				if ( Fear > 80 ) {
				}
				LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
				AimAngle = LookAngle;
				break; }
			case AIState.Attacking:
				if ( AimTimer.TimeLeft > AimTimer.TimeLeft * 0.25f ) {
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
					Aiming = false;
					AimTimer.Stop();
					Bark( BarkType.OutOfTheWay );
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
					State = AIState.PatrolStart;
					SetNavigationTarget( NextRoute.GetGlobalStartPosition() );
				}
				break;
			case AIState.PatrolStart:
				if ( GlobalPosition.DistanceTo( PatrolRoute.GetGlobalStartPosition() ) < 10.0f ) {
					State = AIState.Patrolling;
					SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
				}
				break;
			case AIState.Guarding:
				if ( Awareness > AIAwareness.Relaxed ) {
					State = AIState.Investigating;
				}
				break;
			};
		}

		protected override void SetNavigationTarget( Godot.Vector2 target ) {
			NavAgent.TargetPosition = target;
			TargetReached = false;
			GotoPosition = target;
			if ( NextRoute != null ) {
				if ( NextRoute.GetGlobalStartPosition() == target ) {
					State = AIState.Patrolling;
				}
			}
		}
		protected override void OnTargetReached() {
			TargetReached = true;

			switch ( State ) {
			case AIState.Patrolling:
				PatrolRoute = NextRoute;
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
		}

		/*
		private void CheckSight( float delta ) {
			if ( ( Engine.GetPhysicsFrames() % 30 ) != 0 ) {
				RecalcSight();
			}

			GodotObject sightTarget = null;
			for ( int i = 0; i < SightLines.Length; i++ ) {
				sightTarget = SightLines[i].GetCollider();
				if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
					break;
				} else {
					sightTarget = null;
				}
			}

			if ( SightTarget != null && Awareness == AIAwareness.Alert ) {
	//			return; // hunt them down
			}

			// we saw something, but it slipped out of view
			if ( SightDetectionAmount > 0.0f && sightTarget == null ) {
				CreateAfterImage();
				switch ( Awareness ) {
				case AIAwareness.Relaxed:
					// "must be nothing"
					SightDetectionAmount -= SightDetectionSpeed * delta;
					if ( SightDetectionAmount <= 0.0f ) {
						SightDetectionAmount = 0.0f;
					}
					break;
				case AIAwareness.Suspicious:
					SetSuspicious();
					break;
				case AIAwareness.Alert:
					SetAlert( true );
					break;
				};
				SetDetectionColor();
				CanSeeTarget = false;
				return;
			}

			AfterImageUpdated = false;

			SightTarget = (Entity)sightTarget;
			if ( sightTarget is Player || sightTarget is NetworkPlayer ) {
				CanSeeTarget = true;
				LastTargetPosition = SightTarget.GlobalPosition;
				if ( Awareness >= AIAwareness.Suspicious ) {
					SightDetectionAmount += ( SightDetectionSpeed * 2.0f );
					SetNavigationTarget( LastTargetPosition );
					Squad.NotifyGroup( GroupEvent.TargetChanged, this );
				} else if ( Awareness == AIAwareness.Relaxed ) {
					SightDetectionAmount += SightDetectionSpeed;
				}
			}
			if ( SightDetectionAmount > SightDetectionTime * 0.5f ) {
				Awareness = AIAwareness.Suspicious;
			}
			if ( IsAlert() ) {
				SetAlert( false );
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			}
			SetDetectionColor();
		}
		*/
	};
};