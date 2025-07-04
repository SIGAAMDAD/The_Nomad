using System;
using ChallengeMode;
using GdUnit4;
using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Bandit : Thinker {
		private enum State : uint {
			Guarding,
			Attacking,
			Investigating,

			Count
		};

		private static readonly float AngleBetweenRays = Mathf.DegToRad( 8.0f );
		private static readonly float ViewAngleAmount = Mathf.DegToRad( 80.0f );
		private static readonly float MaxViewDistance = 180.0f;

		private static readonly int ChallengeMode_Score = 10;

		[Export]
		private float LoseInterestTime = 0.0f;
		[Export]
		private float SightDetectionTime = 0.0f;
		[Export]
		private float SightDetectionSpeed = 0.0f;
		[Export]
		private AINodeCache NodeCache;

		private float SightDetectionAmount = 0.0f;

		private Hitbox HeadHitbox;

		public bool HitHead {
			get;
			private set;
		}

		private State CurrentState;

		private Godot.Vector2 StartPosition;
		private float StartHealth;

		// combat variables
		private Timer AimTimer;
		private Timer AttackTimer;
		private RayCast2D AimLine;
		private bool Aiming = false;
		private Line2D AttackMeter;
		private float AttackMeterProgress = 0.0f;
		private Godot.Vector2 AttackMeterFull;
		private Godot.Vector2 AttackMeterDone;

		private Entity SightTarget = null;

		private Timer LoseInterestTimer;
		private Timer ChangeInvestigationAngleTimer;
		private Timer TargetMovedTimer;

		private WeaponEntity Weapon;
		private AmmoEntity Ammo;
		private AmmoStack AmmoStack;

		private MobAwareness Awareness = MobAwareness.Relaxed;

		private AudioStreamPlayer2D BarkChannel;

		public Godot.Vector2 LastTargetPosition { get; private set; } = Godot.Vector2.Zero;
		private RayCast2D[] SightLines = null;
		private bool CanSeeTarget = false;
		private Tween MeleeTween;

		public Entity Target { get; private set;  }

		private Line2D DetectionMeter;

		// if we have fear, move slower
		private float SpeedDegrade = 1.0f;

		private int Fear = 0;

		private Color DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		private BarkType LastBark = BarkType.Count;
		private BarkType SequencedBark = BarkType.Count;

		private void GenerateRayCasts() {
			int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
			SightLines = new RayCast2D[ rayCount ];
			for ( int i = 0; i < rayCount; i++ ) {
				RayCast2D ray = new RayCast2D();
				float angle = AngleBetweenRays * ( i - rayCount / 2.0f );
				ray.TargetPosition = Godot.Vector2.Right.Rotated( angle ) * MaxViewDistance;
				ray.Enabled = true;
				ray.CollisionMask = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity );
				SightLines[i] = ray;
				HeadAnimations.AddChild( ray );
			}
		}

		public override void Alert( Entity source ) {
			LastTargetPosition = source.GlobalPosition;

			SightDetectionAmount = Mathf.Lerp( SightDetectionAmount, SightDetectionTime, 1.0f / GlobalPosition.DistanceTo( source.GlobalPosition ) );
			LookDir = GlobalPosition.DirectionTo( source.GlobalPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;

			ChangeInvestigationAngleTimer.CallDeferred( "start" );

			if ( IsAlert() ) {
				SetAlert();
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			} else {
				Bark( BarkType.Confusion );
			}

			SetNavigationTarget( LastTargetPosition );
			CurrentState = State.Investigating;

			Fear += 5;
		}

		public Godot.Vector2 GetInvestigationPosition() => LastTargetPosition;

		public override void AddStatusEffect( string effectName ) {
			base.AddStatusEffect( effectName );

			if ( Target is Player && Fear >= 80 && effectName == "status_burning" ) {
				SteamAchievements.ActivateAchievement( "ACH_DANCE_DANCE_DANCE" );
			}
		}

		public bool IsAlert() => Awareness == MobAwareness.Alert || SightDetectionAmount >= SightDetectionTime;
		public bool IsSuspicious() => Awareness == MobAwareness.Suspicious || SightDetectionAmount >= SightDetectionTime * 0.25f;

		public override void SetLocation( in WorldArea location ) {
			base.SetLocation( location );
			NodeCache = location.GetNodeCache();
		}

		public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 && stream != ResourceCache.GetSound( "res://sounds/mobs/die_low.ogg" )
				&& stream != ResourceCache.GetSound( "res://sounds/mobs/die_high.ogg" ) )
			{
				return;
			}
			base.PlaySound( channel, stream );
		}

		public override void Damage( in Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );
			PlaySound( AudioChannel, ResourceCache.Pain[ RNJesus.IntRange( 0, ResourceCache.Pain.Length - 1 ) ] );

			AimTimer.Stop();
			Aiming = false;

			if ( Health <= 0.0f ) {
				HeadHitbox.SetDeferred( Area2D.PropertyName.Monitoring, false );
				HeadHitbox.GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );

				DetectionMeter.CallDeferred( MethodName.Hide );

				AimTimer.Stop();
				Aiming = false;

				Velocity = Godot.Vector2.Zero;
				NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );

				GotoPosition = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;

				HeadAnimations.CallDeferred( MethodName.Hide );
				ArmAnimations.CallDeferred( MethodName.Hide );
				if ( !HitHead ) {
					CallDeferred( MethodName.PlaySound, AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_low.ogg" ) );
					BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "die_low" );
				}
				return;
			}

			if ( source.GetFaction() == Faction ) {
				// "CEASEFIRE!"
				//				Bark( BarkType.Ceasefire );
			}

			if ( Awareness == MobAwareness.Alert ) {

			} else {
				SetAlert();
			}

			float angle = RNJesus.FloatRange( 0, 360.0f );
			HeadAnimations.GlobalRotation = angle;
			ArmAnimations.GlobalRotation = angle;

			Target = source;
			LastTargetPosition = source.GlobalPosition;

			CurrentState = State.Attacking;
			SetNavigationTarget( LastTargetPosition );
		}

		private void SetSuspicious() {
			Awareness = MobAwareness.Suspicious;
			CurrentState = State.Investigating;
			Bark( BarkType.Confusion, Squad.GetMemberCount() > 0 ? BarkType.CheckItOut : BarkType.Count );
		}
		private void SetAlert() {
			if ( Awareness != MobAwareness.Alert ) {
//				SetNavigationTarget( NodeCache.FindClosestCover( GlobalPosition, Target.GlobalPosition ).GlobalPosition );
			}
			ChangeInvestigationAngleTimer.CallDeferred( Timer.MethodName.Stop );
			Target = SightTarget;
			Bark( BarkType.TargetSpotted );
			Awareness = MobAwareness.Alert;
		}

		private void SetFear( int nAmount ) {
			Fear = nAmount;
			if ( Fear >= 100 ) {
				SpeedDegrade = 0.0f;
				ChangeInvestigationAngleTimer.WaitTime = 0.5f;
			} else if ( Fear >= 80 ) {
				SpeedDegrade = 0.25f;
				ChangeInvestigationAngleTimer.WaitTime = 0.90f;
			} else if ( Fear >= 60 ) {
				SpeedDegrade = 0.5f;
				ChangeInvestigationAngleTimer.WaitTime = 1.2f;
			} else {
				SpeedDegrade = 1.0f;
				ChangeInvestigationAngleTimer.WaitTime = 2.0;
			}
		}

		private bool IsValidTarget( in Entity target ) => target is Player;
		private AudioStream GetBarkResource( BarkType bark ) {
			switch ( bark ) {
			case BarkType.ManDown:
				return ResourceCache.ManDown[ RNJesus.IntRange( 0, ResourceCache.ManDown.Length - 1 ) ];
			case BarkType.MenDown2:
				return ResourceCache.ManDown2;
			case BarkType.MenDown3:
				return ResourceCache.ManDown3;
			case BarkType.TargetSpotted:
				return ResourceCache.TargetSpotted[ RNJesus.IntRange( 0, ResourceCache.TargetSpotted.Length - 1 ) ];
			case BarkType.TargetPinned:
				return ResourceCache.TargetPinned[ RNJesus.IntRange( 0, ResourceCache.TargetPinned.Length - 1 ) ];
			case BarkType.TargetRunning:
				return ResourceCache.TargetRunning[ RNJesus.IntRange( 0, ResourceCache.TargetRunning.Length - 1 ) ];
			case BarkType.Confusion:
				return ResourceCache.Confusion[ RNJesus.IntRange( 0, ResourceCache.Confusion.Length - 1 ) ];
			case BarkType.Alert:
				return ResourceCache.Alert[ RNJesus.IntRange( 0, ResourceCache.Alert.Length - 1 ) ];
			case BarkType.OutOfTheWay:
				return ResourceCache.OutOfTheWay[ RNJesus.IntRange( 0, ResourceCache.OutOfTheWay.Length - 1 ) ];
			case BarkType.NeedBackup:
				return ResourceCache.NeedBackup[ RNJesus.IntRange( 0, ResourceCache.NeedBackup.Length - 1 ) ];
			case BarkType.SquadWiped:
				return ResourceCache.SquadWiped;
			case BarkType.Curse:
				return ResourceCache.Curse[ RNJesus.IntRange( 0, ResourceCache.Curse.Length - 1 ) ];
			case BarkType.CheckItOut:
				return ResourceCache.CheckItOut[ RNJesus.IntRange( 0, ResourceCache.CheckItOut.Length - 1 ) ];
			case BarkType.Quiet:
				return ResourceCache.Quiet[ RNJesus.IntRange( 0, ResourceCache.Quiet.Length - 1 ) ];
			case BarkType.Unstoppable:
				return ResourceCache.Unstoppable;
			case BarkType.Count:
			default:
				break;
			};
			return null;
		}
		private void Bark( BarkType bark, BarkType sequenced = BarkType.Count ) {
			if ( SettingsData.GetCleanAudio() && ( bark == BarkType.Curse || sequenced == BarkType.Curse ) ) {
				return;
			}
			if ( Health <= 0.0f || LastBark == bark ) {
				return;
			}
			LastBark = bark;
			SequencedBark = sequenced;

			PlaySound( BarkChannel, GetBarkResource( bark ) );
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
				} else {
					DetectionColor.R = 0.0f;
					DetectionColor.G = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
					DetectionColor.B = 1.0f;
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
			};
			DetectionMeter.SetDeferred( Line2D.PropertyName.DefaultColor, DetectionColor );
		}
		private void OnChangeInvestigationAngleTimerTimeout() {
			float angle;
			if ( RNJesus.IntRange( 0, 99 ) > 50 ) {
				angle = RNJesus.FloatRange( -50.0f, 50.0f );
			} else {
				angle = RNJesus.FloatRange( 130.0f, 230.0f );
			}
			LookAngle = Mathf.DegToRad( angle );
			AimAngle = Mathf.DegToRad( angle );
			ChangeInvestigationAngleTimer.CallDeferred( Timer.MethodName.Start );
		}
		private void OnLoseInterestTimerTimeout() {
			CurrentState = State.Investigating;

			if ( Fear > 60 ) {
				Bark( BarkType.Curse, BarkType.Quiet );
			}

			SetFear( Fear + 20 );
		}

		public void OnHeadHit( Entity source ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			HitHead = true;
			CallDeferred( MethodName.PlaySound, AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_high.ogg" ) );
			BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "die_high" );
			Damage( source, Health );
		}

		private void OnRestartCheckpoint() {
			SetDeferred( PropertyName.GlobalPosition, StartPosition );
			CurrentState = State.Guarding;
			Awareness = MobAwareness.Relaxed;
			Health = StartHealth;
			Flags = 0;
			SightDetectionAmount = 0.0f;
		}
		private void ResetAttackMeter() {
			AttackMeterProgress = AttackMeterFull.X;
			AttackMeter.Points[ 1 ] = AttackMeterFull;
			AttackMeter.Show();
		}
		private void UpdateAttackMeter() {
			AttackMeter.Points[ 1 ].X = AttackMeterProgress;
		}

		protected override void OnDie( Entity source, Entity target ) {
			base.OnDie( source, target );

			if ( source is Player ) {
				if ( GameConfiguration.GameMode == GameMode.ChallengeMode || GameConfiguration.GameMode == GameMode.JohnWick ) {
					if ( BodyAnimations.Animation == "die_high" ) {
						FreeFlow.AddKill( KillType.Headshot, ChallengeMode_Score );
					} else if ( BodyAnimations.Animation == "die_low" ) {
						FreeFlow.AddKill( KillType.Bodyshot, ChallengeMode_Score );
					}
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				AddToGroup( "Enemies" );

				LevelData.Instance.PlayerRespawn += OnRestartCheckpoint;
			}

			StartPosition = GlobalPosition;
			StartHealth = Health;

			HeadHitbox = HeadAnimations.GetNode<Hitbox>( "HeadHitbox" );
			HeadHitbox.Hit += OnHeadHit;

			CurrentState = State.Guarding;

			Squad = GroupManager.GetGroup( GroupType.Bandit, Faction, GlobalPosition );
			Squad.AddThinker( this );

			//			NodeCache ??= Location.GetNodeCache();

			LoseInterestTimer = new Timer();
			LoseInterestTimer.Name = "LoseInterestTimer";
			LoseInterestTimer.WaitTime = LoseInterestTime;
			LoseInterestTimer.OneShot = true;
			LoseInterestTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnLoseInterestTimerTimeout ) );
			AddChild( LoseInterestTimer );

			AttackMeter = GetNode<Line2D>( "AttackMeter" );
			AttackMeterDone = AttackMeter.Points[ 0 ];
			AttackMeter.Points[ 1 ] = AttackMeterFull;
			AttackMeterProgress = AttackMeterFull.X;

			AimLine = new RayCast2D();
			AimLine.Name = "AimLine";
			AimLine.TargetPosition = Godot.Vector2.Right;
			AimLine.CollideWithAreas = true;
			AimLine.CollideWithBodies = true;
			AimLine.CollisionMask = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity );
			AimLine.HitFromInside = false;
			ArmAnimations.AddChild( AimLine );

			AttackTimer = new Timer();
			AttackTimer.Name = "AttackTimer";
			AttackTimer.OneShot = true;
			AttackTimer.Connect( Timer.SignalName.Timeout, Callable.From( () => { Aiming = false; } ) );
			AddChild( AttackTimer );

			AimTimer = new Timer();
			AimTimer.Name = "AimTimer";
			AimTimer.WaitTime = 2.5f;
			AimTimer.OneShot = true;
			AimTimer.Connect( "timeout", Callable.From( OnAimTimerTimeout ) );
			AddChild( AimTimer );

			BarkChannel = new AudioStreamPlayer2D();
			BarkChannel.Name = "BarkChannel";
			BarkChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AddChild( BarkChannel );

			DetectionMeter = GetNode<Line2D>( "DetectionMeter" );

			Weapon = new WeaponEntity();
			Weapon.Name = "Weapon";
			Weapon.Data = ResourceLoader.Load( "res://resources/weapons/firearms/desert_carbine.tres" );

			Ammo = new AmmoEntity();
			Ammo.Name = "Ammo";
			Ammo.Data = ResourceLoader.Load( "res://resources/ammo/556_ammo.tres" );
			AddChild( Ammo );

			AmmoStack = new AmmoStack();
			AmmoStack.SetType( Ammo );
			AmmoStack.Amount = (int)Ammo.Data.Get( "max_stack" );

			AddChild( Weapon );

			Weapon.TriggerPickup( this );
			Weapon.OverrideRayCast( AimLine );
			Weapon.SetOwner( this );
			Weapon.SetReserve( AmmoStack );
			Weapon.SetAmmo( Ammo );

			ChangeInvestigationAngleTimer = new Timer();
			ChangeInvestigationAngleTimer.Name = "ChangeInvestigationAngleTimer";
			ChangeInvestigationAngleTimer.OneShot = true;
			ChangeInvestigationAngleTimer.WaitTime = 1.0f;
			ChangeInvestigationAngleTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnChangeInvestigationAngleTimerTimeout ) );
			AddChild( ChangeInvestigationAngleTimer );

			TargetMovedTimer = new Timer();
			TargetMovedTimer.Name = "TargetMovedTimer";
			TargetMovedTimer.WaitTime = 5.0f;
			TargetMovedTimer.OneShot = true;
			TargetMovedTimer.Connect( Timer.SignalName.Timeout, Callable.From( () => { Bark( BarkType.TargetPinned ); } ) );
			AddChild( TargetMovedTimer );

			switch ( Direction ) {
			case DirType.North:
				LookDir = Godot.Vector2.Up;
				break;
			case DirType.East:
				LookDir = Godot.Vector2.Right;
				break;
			case DirType.South:
				LookDir = Godot.Vector2.Down;
				break;
			case DirType.West:
				LookDir = Godot.Vector2.Left;
				BodyAnimations.FlipH = true;
				break;
			};
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;

			GenerateRayCasts();
		}

		private void OnAimTimerTimeout() {
			if ( AimLine.GetCollider() is Entity entity && entity != null ) {
				if ( entity.GetFaction() == Faction ) {
					Bark( BarkType.OutOfTheWay );
					SetNavigationTarget( GlobalPosition + new Godot.Vector2( Godot.Vector2.Right.X + 50.0f, Godot.Vector2.Right.Y + 20.0f ) );
					return;
				}
			}

			AttackTimer.Start();
			Weapon.CallDeferred( WeaponEntity.MethodName.SetUseMode, (uint)WeaponEntity.Properties.TwoHandedFirearm );
			Weapon.CallDeferred( WeaponEntity.MethodName.UseDeferred, (uint)WeaponEntity.Properties.TwoHandedFirearm );
		}
		public override void PickupWeapon( in WeaponEntity weapon ) {
			// TODO: evaluate if we actually want it
			Weapon = weapon;

			if ( ( Weapon.PropertyBits & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				Weapon.SetUseMode( WeaponEntity.Properties.TwoHandedFirearm );
				AttackTimer.SetDeferred( Timer.PropertyName.WaitTime, Weapon.UseTime );
				AimLine.SetDeferred( RayCast2D.PropertyName.TargetPosition, Godot.Vector2.Right * (float)( (Godot.Collections.Dictionary)Ammo.Data.Get( "properties" ) )[ "range" ] );
			}
		}

		private void AttackMelee() {
			MeleeTween = CreateTween();
			MeleeTween.CallDeferred( Tween.MethodName.TweenProperty, ArmAnimations, PropertyName.GlobalRotation, 0.0f, Weapon.Weight );
		}

		protected override void ProcessAnimations() {
			LookAtTarget();
			base.ProcessAnimations();
		}
		protected override void Think() {
			if ( !Visible ) {
				return;
			}

			CheckSight();

			if ( AimTimer.TimeLeft > 0.0f && Target is Player player && player != null ) {
				if ( ( player.GetFlags() & Player.PlayerFlags.Parrying ) != 0 ) {
					// shoot early
					AimTimer.Stop();
					OnAimTimerTimeout();
				}
			}

			// do we have a target?
			if ( Awareness > MobAwareness.Relaxed ) {

				LookAtTarget();

				// can we see the target?
				if ( CanSeeTarget ) {

					// are we in range?
					if ( GlobalPosition.DistanceTo( LastTargetPosition ) > 2048.0f ) {
						// if not, stop aiming and move in closer
						Aiming = false;
						AimTimer.CallDeferred( Timer.MethodName.Stop );

						const float interp = 0.5f;
						Godot.Vector2 position1 = GlobalPosition;
						Godot.Vector2 position2 = LastTargetPosition;
						SetNavigationTarget( new Godot.Vector2( position1.X * ( 1 - interp ) + position2.X * interp, position1.Y * ( 1 - interp ) + position2.Y * interp ) );

						// cycle
						return;
					}

					// are we about to shoot?
					if ( Aiming ) {
						CallDeferred( MethodName.UpdateAttackMeter );

						// allow adjustments until we have 15% time left
						if ( AimTimer.TimeLeft > AimTimer.WaitTime * 0.15f ) {
							LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
							AimAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
							LookAngle = AimAngle;

							CallDeferred( MethodName.StopMoving );
						}
					} else {
						// if not, start
						CallDeferred( MethodName.ResetAttackMeter );

						Tween Tweener = CreateTween();
						Tweener.CallDeferred( Tween.MethodName.TweenProperty, this, "AttackMeterProgress", AttackMeterDone.X, AimTimer.WaitTime );
						Tweener.CallDeferred( Tween.MethodName.Connect, Tween.SignalName.Finished, Callable.From( AttackMeter.Hide ) );

						AimTimer.CallDeferred( Timer.MethodName.Start );
						Aiming = true;

						CallDeferred( MethodName.StopMoving );
					}

				} else {
					// are we at the last known position?
					if ( GotoPosition == LastTargetPosition ) {
						// start the lose interest timer
						if ( LoseInterestTimer.IsStopped() ) {
							LoseInterestTimer.CallDeferred( Timer.MethodName.Start );
						} else if ( LoseInterestTimer.TimeLeft == 0.0f ) {
							// if we have lost interest, go back to starting position
							Awareness = MobAwareness.Suspicious;
							SetNavigationTarget( StartPosition );
							CurrentState = State.Guarding;
						}

						// cycle
						return;
					}

					// investigate the last known position
					if ( CurrentState != State.Investigating ) {
						Awareness = MobAwareness.Suspicious;
						SetNavigationTarget( LastTargetPosition );
						CurrentState = State.Investigating;
						ChangeInvestigationAngleTimer.CallDeferred( Timer.MethodName.Start );

						// cycle
						return;
					}
				}
			}
		}

		public void LookAtTarget() {
			if ( SightTarget == null || ChangeInvestigationAngleTimer.TimeLeft > 0.0f ) {
				return;
			}
			LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;
		}

		private void CheckSight() {
			Entity sightTarget = null;
			for ( int i = 0; i < SightLines.Length; i++ ) {
				sightTarget = SightLines[ i ].GetCollider() as Entity;
				if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
					break;
				} else {
					sightTarget = null;
				}
			}
			
			if ( SightDetectionAmount >= SightDetectionTime * 0.25f && SightDetectionAmount < SightDetectionTime * 0.90f && sightTarget == null ) {
				SetSuspicious();
				CurrentState = State.Investigating;
				SetNavigationTarget( LastTargetPosition );
				if ( LoseInterestTimer.IsStopped() ) {
					LoseInterestTimer.Start();
				}
			}

			CanSeeTarget = sightTarget != null;

			if ( sightTarget == null && SightDetectionAmount > 0.0f ) {
				// out of sight, but we got something
				switch ( Awareness ) {
				case MobAwareness.Relaxed:
					SightDetectionAmount -= SightDetectionAmount * (float)GetProcessDeltaTime();
					if ( SightDetectionAmount < 0.0f ) {
						SightDetectionAmount = 0.0f;
					}
					break;
				case MobAwareness.Suspicious:
					SetSuspicious();
					break;
				case MobAwareness.Alert:
					SetAlert();
					break;
				};
				SetDetectionColor();
				return;
			}

			if ( sightTarget != null ) {
				if ( sightTarget.GetHealth() <= 0.0f && sightTarget.GetFaction() == Faction ) {
					Bark( BarkType.ManDown );

					Awareness = MobAwareness.Alert;
					ChangeInvestigationAngleTimer.Start();
				} else {
					SightTarget = sightTarget;
					LastTargetPosition = sightTarget.GlobalPosition;
					CanSeeTarget = true;

					LookAtTarget();

					if ( Awareness >= MobAwareness.Suspicious ) {
						// if we're already suspicious, then detection rate increases as we're more alert
						SightDetectionAmount += SightDetectionSpeed * 2.0f * (float)GetProcessDeltaTime();
					} else {
						SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();
					}
				}
			}

			if ( IsAlert() ) {
				SetAlert();
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			}
			SetDetectionColor();
		}

		protected override void InitRenownStats() {
			InitBaseStats();

			if ( !IsPremade ) {
				Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Cities" );
				Family = FamilyCache.GetFamily( nodes[ RNJesus.IntRange( 0, nodes.Count - 1 ) ] as Settlement, (SocietyRank)RNJesus.IntRange( 0, (int)SocietyRank.Count ) );
				FirstName = NameGenerator.GenerateName();
				BotName = string.Format( "{0} {1}", FirstName, Family.GetFamilyName() );
				Name = string.Format( "{0}{1}{2}", this, FirstName, Family.GetFamilyName() );
			}

			GD.Print( "Generated Mercenary " + this + ":" );
			GD.Print( "\t[Renown Data]:" );
			GD.Print( "\t\tBotName: " + BotName );
			GD.Print( "\t\tBirthPlace: " + BirthPlace.GetAreaName() );
			GD.Print( "\t\tFamily: " + Family.GetFamilyName() );
		}
	};
};