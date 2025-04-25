using System.Linq;
using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Thinker : Entity {
		public partial class Bandit : Occupation {
			private static readonly float Range = 300.0f;
			private static readonly float AngleBetweenRays = Mathf.DegToRad( 2.0f );

			private bool Aiming = false;

			private Timer TargetMovedTimer;
			private Timer AttackTimer;
			private Timer HealTimer;
			private Timer AimTimer;
			private RayCast2D AimLine;
			private Line2D ShootLine;
			private Vector2 GuardPosition;

			private AudioStreamPlayer2D BarkChannel;

			private MobAwareness Awareness;
			private MobState MobState;

			private BarkType LastBark;
			private BarkType SequencedBark;

			private bool CanSeeTarget = false;

			private AINodeCache NodeCache;
			private AIPatrolRoute PatrolRoute;

			private Vector2 LastTargetPosition;

			private Timer LoseInterestTimer;
			private Timer ChangeInvestigateAngleTimer;
			private Node2D SightDetector;
			private Line2D DetectionMeter;
			private Color DetectionColor;

			private AIPatrolRoute NextRoute;

			private float SightDetectionAmount = 0.0f;

			private int Fear = 0;

			private float SoundDetectionLevel;
			private float ViewAngleAmount = 45.0f;
			private float MaxViewDistance = 100.0f;
			private float SightDetectionSpeed = 0.1f;
			private float SightDetectionTime = 8.0f;
			private float LoseInterestTime = 8.5f;
			private float SoundTolerance = 150.0f;

			private RayCast2D[] SightLines;

			private Entity Target;

			public Bandit( Thinker worker, Faction faction )
				: base( worker, faction )
			{
				worker.Damaged += OnDamage;

				ViewAngleAmount = Mathf.DegToRad( ViewAngleAmount );

				worker.LookDir = Vector2.Right;
				worker.LookAngle = Mathf.Atan2( worker.LookDir.Y, worker.LookDir.X );
				worker.AimAngle = worker.LookAngle;

				Awareness = MobAwareness.Relaxed;
				MobState = MobState.Guarding;

				NodeCache = Worker.Location.GetNodeCache();

				GuardPosition = worker.GlobalPosition;

				ProcessMode = ProcessModeEnum.Disabled;
			}

			private void Cache() {
				SightDetector = new Node2D();
				Worker.CallDeferred( "add_child", SightDetector );
				CallDeferred( "GenerateRaycasts" );

				Worker.HeadAnimations = new AnimatedSprite2D();
				Worker.HeadAnimations.SpriteFrames = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/thinkers/bandit/head.tres" );
				Worker.CallDeferred( "add_child", Worker.HeadAnimations );

				Worker.ArmAnimations = new AnimatedSprite2D();
				Worker.ArmAnimations.SpriteFrames = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/thinkers/bandit/arms.tres" );
				Worker.CallDeferred( "add_child", Worker.ArmAnimations );

				Worker.BodyAnimations = new AnimatedSprite2D();
				Worker.BodyAnimations.SpriteFrames = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/thinkers/bandit/body.tres" );
				Worker.CallDeferred( "add_child", Worker.BodyAnimations );

				AimTimer = new Timer();
				AimTimer.Name = "AimTimer";
				AimTimer.WaitTime = 1.0f;
				AimTimer.OneShot = true;
				AimTimer.Connect( "timeout", Callable.From( OnAimTimerTimeout ) );
				Worker.CallDeferred( "add_child", AimTimer );

				LoseInterestTimer = new Timer();
				LoseInterestTimer.WaitTime = LoseInterestTime;
				LoseInterestTimer.OneShot = true;
				LoseInterestTimer.Autostart = false;
				LoseInterestTimer.Connect( "timeout", Callable.From( OnLoseInterestTimerTimeout ) );
				Worker.CallDeferred( "add_child", LoseInterestTimer );

				ChangeInvestigateAngleTimer = new Timer();
				ChangeInvestigateAngleTimer.OneShot = true;
				ChangeInvestigateAngleTimer.WaitTime = 1.5f;
				ChangeInvestigateAngleTimer.Autostart = false;
				ChangeInvestigateAngleTimer.Connect( "timeout", Callable.From( OnChangeInvestigateAngleTimerTimeout ) );
				Worker.CallDeferred( "add_child", ChangeInvestigateAngleTimer );

				TargetMovedTimer = new Timer();
				TargetMovedTimer.OneShot = true;
				TargetMovedTimer.WaitTime = 10.0f;
				Worker.CallDeferred( "add_child", TargetMovedTimer );

				AttackTimer = new Timer();
				AttackTimer.WaitTime = 1.5f;
				AttackTimer.OneShot = true;
				AttackTimer.Connect( "timeout", Callable.From( () => { Aiming = false; } ) );
				Worker.CallDeferred( "add_child", AttackTimer );

				DetectionMeter = new Line2D();
				DetectionMeter.AddPoint( new Vector2( -25.0f, -19.0f ), 0 );
				DetectionMeter.AddPoint( new Vector2( 4.0f, -19.0f ), 1 );
				DetectionMeter.Width = 6.0f;
				Worker.CallDeferred( "add_child", DetectionMeter );

				AimLine = new RayCast2D();
				AimLine.Name = "AimLine";
				AimLine.TargetPosition = Vector2.Right * Range;
				AimLine.CollisionMask = 2 | 5;
				Worker.ArmAnimations.CallDeferred( "add_child", AimLine );

				Worker.AudioChannel = new AudioStreamPlayer2D();
				Worker.AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
				Worker.CallDeferred( "add_child", Worker.AudioChannel );
			}

			public override void OnPlayerEnteredArea() {
				Cache();
			}
			public override void OnPlayerExitedArea() {
				Worker.CallDeferred( "remove_child", Worker.AudioChannel );
				Worker.AudioChannel.CallDeferred( "queue_free" );

				Worker.Animations.CallDeferred( "remove_child", Worker.HeadAnimations );
				Worker.HeadAnimations.CallDeferred( "queue_free" );

				Worker.Animations.CallDeferred( "remove_child", Worker.ArmAnimations );
				Worker.ArmAnimations.CallDeferred( "queue_free" );

				Worker.Animations.CallDeferred( "remove_child", Worker.BodyAnimations );
				Worker.BodyAnimations.CallDeferred( "queue_free" );

				Worker.CallDeferred( "remove_child", LoseInterestTimer );
				LoseInterestTimer.CallDeferred( "queue_free" );

				Worker.CallDeferred( "remove_child", ChangeInvestigateAngleTimer );
				ChangeInvestigateAngleTimer.CallDeferred( "queue_free" );

				Worker.CallDeferred( "remove_child", TargetMovedTimer );
				TargetMovedTimer.CallDeferred( "queue_free" );

				Worker.CallDeferred( "remove_child", DetectionMeter );
				DetectionMeter.CallDeferred( "queue_free" );

				Worker.CallDeferred( "remove_child", AttackTimer );
				AttackTimer.CallDeferred( "queue_free" );

				Worker.CallDeferred( "remove_child", AimLine );
				AimLine.CallDeferred( "queue_free" );

				for ( int i = 0; i < SightLines.Length; i++ ) {
					SightDetector.CallDeferred( "remove_child", SightLines[i] );
					SightLines[i].QueueFree();
					SightLines[i] = null;
				}
				SightLines = null;

				Worker.CallDeferred( "remove_child", SightDetector );
				SightDetector.CallDeferred( "queue_free" );
			}

			private void OnChangeInvestigateAngleTimerTimeout() {
				float angle = RandomFloat( 0.0f, 360.0f );
				if ( Worker.LookAngle == angle ) {
					angle = RandomFloat( 0.0f, 360.0f );
				}
				Worker.LookDir = Worker.GlobalPosition.Rotated( angle );
				Worker.AimAngle = angle;
				Worker.LookAngle = angle;
			}

			public AIPatrolRoute GetPatrolRoute() {
				return PatrolRoute;
			}
			private float RandomFloat( float min, float max ) {
				return (float)( min + Worker.Random.NextDouble() * ( min - max ) );
			}

			private void GenerateRaycasts() {
				int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
				SightLines = new RayCast2D[ rayCount ];
				for ( int i = 0; i < rayCount; i++ ) {
					RayCast2D ray = new RayCast2D();
					float angle = AngleBetweenRays * ( i - rayCount / 2.0f );
					ray.TargetPosition = Vector2.Right.Rotated( angle ) * MaxViewDistance;
					ray.Enabled = true;
					ray.CollisionMask = 2;
					SightDetector.AddChild( ray );
					SightLines[i] = ray;
				}
			}
			private void RecalcSight() {
				for ( int i = 0; i < SightLines.Length; i++ ) {
					RayCast2D ray = SightLines[i];
					float angle = AngleBetweenRays * ( i - SightLines.Length / 2.0f );
					ray.TargetPosition = Vector2.Right.Rotated( angle ) * MaxViewDistance;
				}
			}
			private void SetDetectionColor() {
				if ( SightDetectionAmount > SightDetectionTime ) {
					SightDetectionAmount = SightDetectionTime;
				}
				switch ( Awareness ) {
				case MobAwareness.Relaxed:
					if ( SightDetectionAmount == 0.0f ) {
						DetectionColor.R = 1.0f;
						DetectionColor.G = 1.0f;
						DetectionColor.B = 1.0f;
					}
					else {
						// blue cuz colorblind people will struggle with the difference between suspicious and alert
						DetectionColor.R = 0.0f;
						DetectionColor.G = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
						DetectionColor.B = 0.0f;
					}
					break;
				case MobAwareness.Suspicious:
					DetectionColor.R = 0.0f;
					DetectionColor.G = 0.0f;
					DetectionColor.B = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
					break;
				case MobAwareness.Alert:
					DetectionColor.R = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
					DetectionColor.G = 0.0f;
					DetectionColor.B = 0.0f;
					break;
				default:
					break;
				};

				DetectionMeter.DefaultColor = DetectionColor;
			}
			public void Alert( Entity target ) {
				if ( ( Worker.Flags & ThinkerFlags.Dead ) != 0 ) {
					return;
				}

				LastTargetPosition = target.GlobalPosition;
				Worker.LookDir = Worker.GlobalPosition.DirectionTo( LastTargetPosition );
				if ( Fear > 60 ) {
					MobState = MobState.Investigating;
					Bark( BarkType.Curse, Worker.Random.Next( 0, 100 ) > 25 ? BarkType.Quiet : BarkType.Count );
				} else {
					if ( Awareness == MobAwareness.Relaxed ) {
						Awareness = MobAwareness.Suspicious;
					}
					MobState = MobState.Investigating;
					PatrolRoute = null;
					Bark( BarkType.Confusion, BarkType.CheckItOut );
				}
				SetNavigationTarget( LastTargetPosition );
				Fear += 20;
				
				// TODO: make everyone else suspicious
			}

			private bool IsValidTarget( GodotObject target ) => target is Entity entity && entity != null && entity.GetFaction() != Worker.Faction;
			private bool IsAlert() => Awareness == MobAwareness.Alert || SightDetectionAmount >= SightDetectionTime;
			private bool IsSuspicious() => ( !IsAlert() && Awareness == MobAwareness.Suspicious ) || SightDetectionAmount >= SightDetectionTime * 0.5f;
			private void SetAlert( bool bRunning ) {
				if ( Awareness != MobAwareness.Alert ) {
					// increase alertness
					MaxViewDistance += MaxViewDistance * 0.5f;
					ViewAngleAmount += 30.0f;
					RecalcSight();

					Bark( BarkType.TargetSpotted );
				}
				Awareness = MobAwareness.Alert;
			}
			private void SetSuspicious() {
				if ( Awareness != MobAwareness.Suspicious ) {
					Bark( BarkType.Confusion );
				}
				Awareness = MobAwareness.Suspicious;
			}

			private AudioStream GetBarkResource( BarkType bark ) {
				switch ( bark ) {
				case BarkType.ManDown:
					return ResourceCache.ManDown[ Worker.Random.Next( 0, ResourceCache.ManDown.Length - 1 ) ];
				case BarkType.MenDown2:
					return ResourceCache.ManDown2;
				case BarkType.MenDown3:
					return ResourceCache.ManDown3;
				case BarkType.TargetSpotted:
					return ResourceCache.TargetSpotted[ Worker.Random.Next( 0, ResourceCache.TargetSpotted.Length - 1 ) ];
				case BarkType.TargetPinned:
					return ResourceCache.TargetPinned[ Worker.Random.Next( 0, ResourceCache.TargetPinned.Length - 1 ) ];
				case BarkType.TargetRunning:
					return ResourceCache.TargetRunning[ Worker.Random.Next( 0, ResourceCache.TargetRunning.Length - 1 ) ];
				case BarkType.Confusion:
					return ResourceCache.Confusion[ Worker.Random.Next( 0, ResourceCache.Confusion.Length - 1 ) ];
				case BarkType.Alert:
					return ResourceCache.Alert[ Worker.Random.Next( 0, ResourceCache.Alert.Length - 1 ) ];
				case BarkType.OutOfTheWay:
					return ResourceCache.OutOfTheWay[ Worker.Random.Next( 0, ResourceCache.OutOfTheWay.Length - 1 ) ];
				case BarkType.NeedBackup:
					return ResourceCache.NeedBackup[ Worker.Random.Next( 0, ResourceCache.NeedBackup.Length - 1 ) ];
				case BarkType.SquadWiped:
					return ResourceCache.SquadWiped;
				case BarkType.Curse:
					return ResourceCache.Curse[ Worker.Random.Next( 0, ResourceCache.Curse.Length - 1 ) ];
				case BarkType.CheckItOut:
					return ResourceCache.CheckItOut[ Worker.Random.Next( 0, ResourceCache.CheckItOut.Length - 1 ) ];
				case BarkType.Quiet:
					return ResourceCache.Quiet[ Worker.Random.Next( 0, ResourceCache.Quiet.Length - 1 ) ];
				case BarkType.Unstoppable:
					return ResourceCache.Unstoppable;
				case BarkType.Count:
				default:
					break;
				};
				return null;
			}
			protected void Bark( BarkType bark, BarkType sequenced = BarkType.Count ) {
				if ( Worker.Health <= 0.0f || LastBark == bark ) {
					return;
				}
				LastBark = bark;
				SequencedBark = sequenced;

				Worker.PlaySound( BarkChannel, GetBarkResource( bark ) );
			}

			private void OnDamage( Entity source, Entity target, float nAmount ) {
				Worker.PlaySound( Worker.AudioChannel, ResourceCache.Pain[ Worker.Random.Next( 0, ResourceCache.Pain.Length - 1 ) ] );

				if ( Worker.Health < 0.0f ) {
					Worker.Velocity = Godot.Vector2.Zero;
					Worker.Flags |= ThinkerFlags.Dead;
					Worker.HeadAnimations.Hide();
					Worker.ArmAnimations.Hide();
					Worker.BodyAnimations.Play( "die_high" );
					return;
				}
				if ( source.GetFaction() == Worker.Faction ) {
					// "CEASEFIRE!"
				}

				Awareness = MobAwareness.Alert;
				float angle = RandomFloat( 0.0f, 360.0f );
				Worker.HeadAnimations.GlobalRotation = angle;
				Worker.HeadAnimations.GlobalRotation = angle;

				LastTargetPosition = source.GlobalPosition;
				PatrolRoute = null;

				if ( Awareness == MobAwareness.Alert ) {

				} else {
					Bark( BarkType.Alert );
				}
			}

			private void OnAimTimerTimeout() {
				AttackTimer.Start();

				if ( AimLine.IsColliding() ) {
					if ( AimLine.GetCollider() is Entity entity && entity == Target ) {
						if ( entity.GetFaction() == Worker.Faction ) {
							// TODO: dodge animation
							Bark( BarkType.OutOfTheWay );
						} else {
							// FIXME:
							entity.Damage( Worker, 20.0f );
						}
					}
				}
			}

			private void OnLoseInterestTimerTimeout() {
//				MobState = MobState.Patrolling;
				Target = null;

				if ( Fear > 80 ) {
					Bark( BarkType.NeedBackup );
				}
				// once we've lost the target for a long period of time, resume patrol routes with a little more suspicion
				PatrolRoute = NodeCache.FindClosestRoute( Worker.GlobalPosition );
			
				AIPatrolRoute route = PatrolRoute;
//				if ( ( Worker.Squad as BanditGroup ).IsRouteOccupied( route ) ) {
//					PatrolRoute = route.GetNext();
//				}
				SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );

				// a little more on edge
				Fear += 10;
			}
			private void OnTargetMoveTimerTimeout() {
				// "target's pinned!"
				Bark( BarkType.TargetPinned );
			}
			
			public override void Notify( GroupEvent nEventType, Thinker source ) {
				if ( source == Worker || ( Worker.Flags & ThinkerFlags.Dead ) != 0 ) {
					return;
				}
				switch ( nEventType ) {
				case GroupEvent.TargetChanged:
					Bandit bandit = source.Job as Bandit;
					Target = bandit.Target;
					LastTargetPosition = bandit.LastTargetPosition;
					Awareness = MobAwareness.Alert;
					MobState = MobState.Investigating;

					SetNavigationTarget( LastTargetPosition );
					Bark( BarkType.Curse );
					break;
				case GroupEvent.Count:
				default:
					return;
				};
			}
			public override void ProcessAnimations() {
				if ( !Worker.Location.IsPlayerHere() || ( Worker.Flags & ThinkerFlags.Dead ) != 0 ) {
					return;
				}
				/*
				if ( Worker.Floor != null ) {
					if ( Worker.Floor.GetUpper() != null && Worker.Floor.GetUpper().GetPlayerStatus() ) {
						Worker.Visible = true;
					} else if ( Worker.Floor.IsInside() ) {
						Worker.Visible = Worker.Floor.GetPlayerStatus();
					}
				} else {
//					Worker.Visible = true;
				}
				*/
				
				if ( Target != null ) {
					Worker.LookDir = Worker.GlobalPosition.DirectionTo( LastTargetPosition );
					Worker.AimAngle = Mathf.Atan2( Worker.LookDir.Y, Worker.LookDir.X );
					Worker.LookAngle = Worker.AimAngle;
				}
				
				Worker.ArmAnimations.SetDeferred( "global_rotation", Worker.AimAngle );
				Worker.HeadAnimations.SetDeferred( "global_rotation", Worker.LookAngle );

				if ( Worker.LookAngle > 225.0f ) {
					Worker.HeadAnimations.SetDeferred( "flip_v", true );
				} else if ( Worker.LookAngle < 135.0f ) {
					Worker.HeadAnimations.SetDeferred( "flip_v", false );
				}
				if ( Worker.AimAngle > 225.0f ) {
					Worker.ArmAnimations.SetDeferred( "flip_v", true );
				} else if ( Worker.AimAngle < 135.0f ) {
					Worker.ArmAnimations.SetDeferred( "flip_v", false );
				}

				if ( Worker.Velocity != Godot.Vector2.Zero ) {
					Worker.BodyAnimations.CallDeferred( "play", "move" );
					Worker.ArmAnimations.CallDeferred( "play", "move" );
					Worker.HeadAnimations.CallDeferred( "play", "move" );
				} else {
					if ( Awareness == MobAwareness.Relaxed ) {
						Worker.BodyAnimations.CallDeferred( "play", "calm" );
						Worker.ArmAnimations.CallDeferred( "hide" );
					} else {
						Worker.ArmAnimations.CallDeferred( "show" );
						Worker.HeadAnimations.CallDeferred( "show" );
						Worker.BodyAnimations.CallDeferred( "play", "idle" );
						Worker.ArmAnimations.CallDeferred( "play", "idle" );
						Worker.HeadAnimations.CallDeferred( "play", "idle" );
					}
				}
			}
			public override void Process() {
				if ( !Worker.Location.IsPlayerHere() ) {
					return;
				}
				CheckSight();

				if ( Target != null ) {
					if ( CanSeeTarget && Awareness == MobAwareness.Alert ) {
						MobState = MobState.Attacking;
						ChangeInvestigateAngleTimer.Stop();
					}
				} else if ( PatrolRoute != null && Worker.GlobalPosition.DistanceTo( PatrolRoute.GetGlobalEndPosition() ) < 10.0f ) {
					PatrolRoute = PatrolRoute.GetNext();
					SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
				}
				if ( Aiming ) {
					Worker.StopMoving();
				}

				switch ( MobState ) {
				case MobState.Investigating:
					Investigate();

					if ( Fear > 80 && CanSeeTarget ) {
						Bark( BarkType.Curse );
					}
					Worker.LookAngle = Mathf.Atan2( Worker.LookDir.Y, Worker.LookDir.X );
					Worker.AimAngle = Worker.LookAngle;

					if ( Target is Player ) {
						Player.InCombat = true;
					}
					break;
				case MobState.Attacking:
					if ( Target is Player ) {
						Player.InCombat = true;
					}
					if ( !CanSeeTarget ) {
						if ( TargetMovedTimer.IsStopped() ) {
							TargetMovedTimer.Start();
						} else if ( TargetMovedTimer.TimeLeft == 0.0f ) {
							MobState = MobState.Investigating;
						}
						break;
					}
					if ( AimTimer.TimeLeft > AimTimer.WaitTime * 0.25f ) {
						Worker.LookDir = Worker.GlobalPosition.DirectionTo( Target.GlobalPosition );
						Worker.AimAngle = Mathf.Atan2( Worker.LookDir.Y, Worker.LookDir.X );
						Worker.LookAngle = Worker.AimAngle;
						AimLine.GlobalRotation = Worker.AimAngle;
					}
					if ( AimTimer.TimeLeft > 0.0f && !CanSeeTarget ) {
						Bark( BarkType.TargetRunning );
						Aiming = false;
						AimTimer.Stop();
					}
					if ( AimLine.GetCollider() is Entity entity && entity != null ) {
						Worker.StopMoving();
						if ( entity.GetHealth() > 0.0f ) {
							if ( Aiming && entity.GetFaction() == Worker.Faction ) {
								Aiming = false;
								AimTimer.Stop();
								Bark( BarkType.OutOfTheWay );
							} else if ( entity == Target ) {
								Aiming = true;
								Worker.BodyAnimations.Play( "attack" );
								Worker.HeadAnimations.Play( "idle" );
								Worker.ArmAnimations.Play( "attack" );

								AimTimer.Start();
								Aiming = true;

								Worker.PlaySound( Worker.AudioChannel, ResourceCache.GetSound( "res://sounds/weapons/desert_rifle_reload.ogg" ) );
							}
						}
					}
					break;
				case MobState.Patrolling:
					if ( Worker.GlobalPosition.DistanceTo( Worker.GotoPosition ) < 10.0f ) {
						NextRoute = PatrolRoute.GetNext();
						MobState = MobState.PatrolStart;
						SetNavigationTarget( NextRoute.GetGlobalStartPosition() );
					}
					break;
				case MobState.PatrolStart:
					if ( Worker.GlobalPosition.DistanceTo( PatrolRoute.GetGlobalStartPosition() ) < 10.0f ) {
						MobState = MobState.Patrolling;
						SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
					}
					break;
				case MobState.Guarding:
					if ( Awareness > MobAwareness.Relaxed ) {
						MobState = MobState.Investigating;
					}
					break;
				};
			}

			public override void SetNavigationTarget( Vector2 target ) {
				if ( MobState == MobState.Patrolling && NextRoute != null ) {
					if ( NextRoute.GetGlobalStartPosition() == target ) {
						MobState = MobState.Patrolling;
					}
				}
			}
			public override void OnTargetReached() {
				switch ( MobState ) {
				case MobState.Patrolling:
					PatrolRoute = NextRoute;
					PatrolRoute ??= NodeCache.FindClosestRoute( Worker.GlobalPosition );
					SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
					break;
				default:
					Worker.StopMoving();
					break;
				};
			}
			private void Investigate() {
				if ( ChangeInvestigateAngleTimer.IsStopped() ) {
					if ( Fear >= 60 ) {
						ChangeInvestigateAngleTimer.WaitTime = 0.6f;
					}
					ChangeInvestigateAngleTimer.Start();
				}
				if ( LoseInterestTimer.IsStopped() ) {
					LoseInterestTimer.Start();
				}
				if ( Target != null && CanSeeTarget ) {
					MobState = MobState.Attacking;

				}
			}
			private void CheckSight() {
				GodotObject sightTarget = null;
				for ( int i = 0; i < SightLines.Length; i++ ) {
					sightTarget = SightLines[i].GetCollider();
					if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
						break;
					} else {
						sightTarget = null;
					}
				}

				// we saw something, but is slipped out of view
				if ( SightDetectionAmount > 0.0f && sightTarget == null ) {
//					if ( Player.AfterImageUpdated == 0 ) {
//						Player.AfterImage.Update();
//						Player.AfterImageUpdated = 1;
//					}

					switch ( Awareness ) {
					case MobAwareness.Relaxed:
						// "must be nothing"
						SightDetectionAmount -= SightDetectionSpeed;
						if ( SightDetectionAmount <= 0.0f ) {
							SightDetectionAmount = 0.0f;
						}
						break;
					case MobAwareness.Suspicious:
						SetSuspicious();
						break;
					case MobAwareness.Alert:
						SetAlert( true );
						break;
					};
					SetDetectionColor();
					CanSeeTarget = false;
					return;
				}
//				System.Threading.Interlocked.Exchange( ref Player.AfterImageUpdated, 0 );

				if ( sightTarget is Entity entity && entity != null ) {
					if ( entity.GetHealth() <= 0.0f ) {

					} else if ( entity.GetFaction() != Worker.Faction ) {
						// not in the same faction, evaluate hostility
						// TODO: make this a little less linear
						Target = entity;
						CanSeeTarget = true;
						LastTargetPosition = entity.GlobalPosition;

						if ( Awareness >= MobAwareness.Suspicious ) {
							// if we're already suspicious, then detection rate increases as we're more alert
							SightDetectionAmount += SightDetectionSpeed * 2.0f;
						} else {
							SightDetectionAmount += SightDetectionSpeed;
						}
					}
				}
				if ( SightDetectionAmount >= SightDetectionTime * 0.5f && SightDetectionAmount < SightDetectionTime * 0.90f ) {
					Awareness = MobAwareness.Suspicious;
					MobState = MobState.Investigating;
					SetNavigationTarget( LastTargetPosition );
					if ( LoseInterestTimer.IsStopped() ) {
						LoseInterestTimer.Start();
					}
				} else if ( SightDetectionAmount >= SightDetectionAmount * 0.90f ) {
					SetAlert( false );
				}
				if ( IsAlert() ) {
					SetAlert( false );
				} else if ( IsSuspicious() ) {
					SetSuspicious();
				}
				SetDetectionColor();
			}
		};
	};
};