using System;
using ChallengeMode;
using DialogueManagerRuntime;
using Godot;
using PlayerSystem;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Mercenary : Thinker {
		private enum State : uint {
			Guarding,
			Attacking,
			Investigating,

			Count
		};

		private static readonly float AngleBetweenRays = Mathf.DegToRad( 8.0f );
		private static readonly float ViewAngleAmount = Mathf.DegToRad( 80.0f );
		private static readonly float MaxViewDistance = 180.0f;

		private static readonly int ChallengeMode_Score = 40;

		[Export]
		private float LoseInterestTime = 0.0f;
		[Export]
		private float SightDetectionTime = 0.0f;
		[Export]
		private float SightDetectionSpeed = 0.0f;
		[Export]
		private Resource DefaultWeapon;
		[Export]
		private Resource DefaultAmmo;
		[Export]
		private AINodeCache NodeCache;
		[Export]
		private AIPatrolRoute PatrolRoute;
		[Export]
		private Resource Dialogue = ResourceLoader.Load( "res://resources/dialogue/mercenary.dialogue" );
		[Export]
		private string MercenaryReason;

		private float SightDetectionAmount = 0.0f;

		private Hitbox HeadHitbox;

		private State CurrentState;

		private Godot.Vector2 StartPosition;
		private float StartHealth;

		// combat variables
		private Timer AimTimer;
		private Timer AttackTimer;
		private RayCast2D AimLine;
		private bool Aiming = false;

//		private HashSet<Entity> SightTargets = new HashSet<Entity>();
		private Entity SightTarget = null;

		private Timer LoseInterestTimer;
		private Timer ChangeInvestigationAngleTimer;
		private Timer TargetMovedTimer;

		private WeaponEntity Weapon;
		private AmmoEntity Ammo;
		private AmmoStack AmmoStack;

		private MobAwareness Awareness = MobAwareness.Relaxed;

		private AudioStreamPlayer2D BarkChannel;

		private Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;
		private RayCast2D[] SightLines = null;
		private bool CanSeeTarget = false;
		private Tween MeleeTween;

		private Entity Target;

		private Line2D DetectionMeter;

		private AnimatedSprite2D HeadAnimations;
		private AnimatedSprite2D ArmAnimations;

		// if we have fear, move slower
		private float SpeedDegrade = 1.0f;

		private int Fear = 0;

		private Color DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		private BarkType LastBark = BarkType.Count;
		private BarkType SequencedBark = BarkType.Count;

#region Dialogue
		private void BegForLife() {
			DialogueGlobals.Instance.BotName = BotName;

			HeadsUpDisplay.StartDialogue( Dialogue, "beg_for_life", new Action<int>( ( choice ) => {
				switch ( choice ) {
				case 0:
					break;
				};
			} ) );
		}
#endregion

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
			Bark( BarkType.Confusion );
			LastTargetPosition = source.GlobalPosition;
			SightDetectionAmount = SightDetectionTime * 0.75f;

			Awareness = MobAwareness.Suspicious;
			LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
			
			float angle = Mathf.Atan2( LookDir.Y, LookDir.X );
			HeadAnimations.GlobalRotation = angle;
			ArmAnimations.GlobalRotation = angle;

			SetNavigationTarget( LastTargetPosition );

			CurrentState = State.Investigating;
		}

		public Godot.Vector2 GetInvestigationPosition() {
			return LastTargetPosition;
		}
		public AIPatrolRoute GetPatrolRoute() {
			return PatrolRoute;
		}
		private float RandomFloat( float min, float max ) {
			return (float)( min + Random.NextDouble() * ( min - max ) );
		}

		public override void AddStatusEffect( string effectName ) {
			base.AddStatusEffect( effectName );

			if ( Target is Player && Fear >= 80 ) {
				SteamAchievements.ActivateAchievement( "ACH_DANCE_DANCE_DANCE" );
			}
		}

		public bool IsAlert() => Awareness == MobAwareness.Alert || SightDetectionAmount >= SightDetectionTime;
		public bool IsSuspicious() => Awareness == MobAwareness.Suspicious || SightDetectionAmount >= SightDetectionTime * 0.25f;

		public override void SetLocation( WorldArea location ) {
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

		private void OnDeathAnimationFinished() {
			int finalFrame = BodyAnimations.SpriteFrames.GetFrameCount( BodyAnimations.Animation ) - 1;
			BodyAnimations.SpriteFrames.SetFrame( BodyAnimations.Animation, finalFrame, BodyAnimations.SpriteFrames.GetFrameTexture( BodyAnimations.Animation, finalFrame ) );
		}

		public override void Damage( Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );
			PlaySound( AudioChannel, ResourceCache.Pain[ Random.Next( 0, ResourceCache.Pain.Length - 1 ) ] );

			if ( Health <= 0.0f ) {
				DetectionMeter.CallDeferred( "hide" );

				AimTimer.Stop();
				Aiming = false;

				Velocity = Godot.Vector2.Zero;
				NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );
				
				GotoPosition = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;
				HeadAnimations.Hide();
				ArmAnimations.Hide();
				if ( BodyAnimations.Animation != "die_high" ) {
					CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_low.ogg" ) );
					BodyAnimations.CallDeferred( "play", "die_low" );
					BodyAnimations.Connect( "animation_finished", Callable.From( OnDeathAnimationFinished ) );
				}

				GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", true );
				SetDeferred( "collision_layer", 0 );
				SetDeferred( "collision_mask", 0 );
				return;
			}

			if ( Fear > 80 ) {
//				BegForLife();
				return;
			}

			if ( source.GetFaction() == Faction ) {
				// "CEASEFIRE!"
//				Bark( BarkType.Ceasefire );
			}

			float angle;
//			if ( Awareness == MobAwareness.Alert ) {
				LookDir = GlobalPosition.DirectionTo( source.GlobalPosition );
				angle = Mathf.Atan2( LookDir.Y, LookDir.X );
//			} else {
//				Bark( BarkType.Alert );
//				SetAlert();

//				angle = RandomFloat( 0, 360.0f );
//			}
			HeadAnimations.GlobalRotation = angle;
			ArmAnimations.GlobalRotation = angle;

			Target = source;
			LastTargetPosition = source.GlobalPosition;
			PatrolRoute = null;
			Awareness = MobAwareness.Alert;

			CurrentState = State.Attacking;
			SetNavigationTarget( LastTargetPosition );
		}

		private void SetSuspicious() {
			if ( Awareness != MobAwareness.Suspicious ) {
				if ( !CanSeeTarget ) {
					// start patrolling near the last known position
					PatrolRoute = NodeCache.FindClosestRoute( LastTargetPosition );
				} else {
//					SetNavigationTarget( LastTargetPosition );
				}
			}
			Awareness = MobAwareness.Suspicious;
			CurrentState = State.Investigating;
			Bark( BarkType.Confusion, Squad.GetMemberCount() > 0 ? BarkType.CheckItOut : BarkType.Count );
		}
		private void SetAlert() {
			if ( Awareness != MobAwareness.Alert ) {
//				SetNavigationTarget( NodeCache.FindClosestCover( GlobalPosition, Target.GlobalPosition ).GlobalPosition );
			}
			Target = SightTarget;
			SetNavigationTarget( LastTargetPosition );
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

		private bool IsValidTarget( Entity target ) => target is Player;
		private AudioStream GetBarkResource( BarkType bark ) {
			switch ( bark ) {
			case BarkType.ManDown:
				return ResourceCache.ManDown[ Random.Next( 0, ResourceCache.ManDown.Length - 1 ) ];
			case BarkType.MenDown2:
				return ResourceCache.ManDown2;
			case BarkType.MenDown3:
				return ResourceCache.ManDown3;
			case BarkType.TargetSpotted:
				return ResourceCache.TargetSpotted[ Random.Next( 0, ResourceCache.TargetSpotted.Length - 1 ) ];
			case BarkType.TargetPinned:
				return ResourceCache.TargetPinned[ Random.Next( 0, ResourceCache.TargetPinned.Length - 1 ) ];
			case BarkType.TargetRunning:
				return ResourceCache.TargetRunning[ Random.Next( 0, ResourceCache.TargetRunning.Length - 1 ) ];
			case BarkType.Confusion:
				return ResourceCache.Confusion[ Random.Next( 0, ResourceCache.Confusion.Length - 1 ) ];
			case BarkType.Alert:
				return ResourceCache.Alert[ Random.Next( 0, ResourceCache.Alert.Length - 1 ) ];
			case BarkType.OutOfTheWay:
				return ResourceCache.OutOfTheWay[ Random.Next( 0, ResourceCache.OutOfTheWay.Length - 1 ) ];
			case BarkType.NeedBackup:
				return ResourceCache.NeedBackup[ Random.Next( 0, ResourceCache.NeedBackup.Length - 1 ) ];
			case BarkType.SquadWiped:
				return ResourceCache.SquadWiped;
			case BarkType.Curse:
				return ResourceCache.Curse[ Random.Next( 0, ResourceCache.Curse.Length - 1 ) ];
			case BarkType.CheckItOut:
				return ResourceCache.CheckItOut[ Random.Next( 0, ResourceCache.CheckItOut.Length - 1 ) ];
			case BarkType.Quiet:
				return ResourceCache.Quiet[ Random.Next( 0, ResourceCache.Quiet.Length - 1 ) ];
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
			DetectionMeter.SetDeferred( "default_color", DetectionColor );
		}
		private void OnChangeInvestigationAngleTimerTimeout() {
			float angle = RandomFloat( 0, 360.0f );
			LookAngle = angle;
			AimAngle = angle;
			ChangeInvestigationAngleTimer.CallDeferred( "start" );
		}
		private void OnLoseInterestTimerTimeout() {
			CurrentState = State.Investigating;

			if ( Fear > 80 ) {
				Bark( BarkType.Curse, BarkType.Quiet );
			}

			if ( NodeCache != null ) {
				PatrolRoute = NodeCache.FindClosestRoute( GlobalPosition );
				SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
			}

			SetFear( Fear + 10 );
		}

		public void OnHeadHit( Entity source ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_high.ogg" ) );
			BodyAnimations.Play( "die_high" );
			Damage( source, Health );
		}

		private void OnRestartCheckpoint() {
			SetDeferred( "global_position", StartPosition );
			CurrentState = State.Guarding;
			Awareness = MobAwareness.Relaxed;
			Health = StartHealth;
			Flags = 0;
			SightDetectionAmount = 0.0f;
			
			NavAgent.AvoidanceEnabled = true;

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", false );

			SetDeferred( "collision_layer", (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );
			SetDeferred( "collision_mask", (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );
		}

		protected override void OnDie( Entity source, Entity target ) {
			base.OnDie( source, target );

			NavAgent.AvoidanceEnabled = false;

			SetDeferred( "collision_layer", (uint)PhysicsLayer.None );
			SetDeferred( "collision_mask", (uint)PhysicsLayer.None );

			if ( source is Player && GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				if ( BodyAnimations.Animation == "die_high" ) {
					ChallengeLevel.IncreaseScore( ChallengeMode_Score * ChallengeCache.ScoreMultiplier_HeadShot * Player.ComboCounter );
					System.Threading.Interlocked.Increment( ref ChallengeLevel.HeadshotCounter );
				} else if ( BodyAnimations.Animation == "die_low" ) {
					ChallengeLevel.IncreaseScore( ChallengeMode_Score * Player.ComboCounter );
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

			HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
			ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );

			HeadHitbox = HeadAnimations.GetNode<Hitbox>( "HeadHitbox" );
			HeadHitbox.Hit += OnHeadHit;

			CurrentState = State.Guarding;

			Squad = GroupManager.GetGroup( GroupType.Military, Faction, GlobalPosition );
			Squad.AddThinker( this );

			NodeCache ??= Location.GetNodeCache();

			LoseInterestTimer = new Timer();
			LoseInterestTimer.Name = "LoseInterestTimer";
			LoseInterestTimer.WaitTime = LoseInterestTime;
			LoseInterestTimer.OneShot = true;
			LoseInterestTimer.Connect( "timeout", Callable.From( OnLoseInterestTimerTimeout ) );
			AddChild( LoseInterestTimer );

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
			AttackTimer.Connect( "timeout", Callable.From( () => { Aiming = false; } ) );
			AddChild( AttackTimer );

			AimTimer = new Timer();
			AimTimer.Name = "AimTimer";
			AimTimer.WaitTime = 1.0f;
			AimTimer.OneShot = true;
			AimTimer.Connect( "timeout", Callable.From( OnAimTimerTimeout ) );
			AddChild( AimTimer );

			BarkChannel = new AudioStreamPlayer2D();
			BarkChannel.Name = "BarkChannel";
			BarkChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AddChild( BarkChannel );

			DetectionMeter = GetNode<Line2D>( "DetectionMeter" );
			DetectionMeter.Hide();

			Weapon = new WeaponEntity();
			Weapon.Name = "Weapon";
			Weapon.Data = DefaultWeapon;
			Weapon.SetResourcePath( "mobs/mercenary/" );

			Ammo = new AmmoEntity();
			Ammo.Name = "Ammo";
			Ammo.Data = DefaultAmmo;
			Ammo._Ready();

			AmmoStack = new AmmoStack();
			AmmoStack.Name = "InventoryStack";
			AmmoStack.SetType( Ammo );
			AmmoStack.Amount = (int)DefaultAmmo.Get( "max_stack" );

			AddChild( Weapon );

			Weapon.TriggerPickup( this );
			Weapon.OverrideRayCast( AimLine );
			Weapon.SetOwner( this );
			Weapon.SetReserve( AmmoStack );
			Weapon.SetAmmo( Ammo );

			ChangeInvestigationAngleTimer = new Timer();
			ChangeInvestigationAngleTimer.Name = "ChangeInvestigationAngleTimer";
			ChangeInvestigationAngleTimer.OneShot = true;
			ChangeInvestigationAngleTimer.WaitTime = 2.5f;
			ChangeInvestigationAngleTimer.Connect( "timeout", Callable.From( OnChangeInvestigationAngleTimerTimeout ) );
			AddChild( ChangeInvestigationAngleTimer );

			TargetMovedTimer = new Timer();
			TargetMovedTimer.Name = "TargetMovedTimer";
			TargetMovedTimer.WaitTime = 5.0f;
			TargetMovedTimer.OneShot = true;
			TargetMovedTimer.Connect( "timeout", Callable.From( () => { Bark( BarkType.TargetPinned ); } ) );
			AddChild( TargetMovedTimer );

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				ThreadSleep = Constants.THREADSLEEP_THINKER_PLAYER_IN_AREA;
			}
		
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
			Weapon.CallDeferred( "SetUseMode", (uint)WeaponEntity.Properties.TwoHandedFirearm );
			Weapon.CallDeferred( "UseDeferred", (uint)WeaponEntity.Properties.TwoHandedFirearm );
		}
		public override void PickupWeapon( WeaponEntity weapon ) {
			// TODO: evaluate if we actually want it
			Weapon = weapon;

			if ( ( Weapon.GetProperties() & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				Weapon.SetUseMode( WeaponEntity.Properties.TwoHandedFirearm );
				AttackTimer.SetDeferred( "wait_time", Weapon.GetUseTime() );
			//	AimTimer.SetDeferred( "wait_time", Weapon.GetReloadTime() );
				AimLine.SetDeferred( "target_position", Godot.Vector2.Right * (float)( (Godot.Collections.Dictionary)Ammo.Data.Get( "properties" ) )[ "range" ] );
			}
		}

		private void AttackMelee() {
			MeleeTween = CreateTween();
			MeleeTween.CallDeferred( "tween_property", ArmAnimations, "global_rotation", 0.0f, Weapon.GetWeight() );
		}

		protected override void ProcessAnimations() {
			if ( !Visible || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( BodyAnimations.FlipH ) {
				ArmAnimations.SetDeferred( "sprite_frames", Weapon.GetFramesLeft() );
			} else {
				ArmAnimations.SetDeferred( "sprite_frames", Weapon.GetFramesRight() );
			}

			if ( SightTarget != null ) {
				LookDir = GlobalPosition.DirectionTo( SightTarget.GlobalPosition );
				LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
				AimAngle = LookAngle;
			}
			ArmAnimations.SetDeferred( "global_rotation", AimAngle );
			HeadAnimations.SetDeferred( "global_rotation", LookAngle );

			if ( Velocity.X < 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", true );
			} else if ( Velocity.X > 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", false );
			}

			if ( LookAngle > 89.0f ) {
				HeadAnimations.SetDeferred( "flip_v", true );
			} else if ( LookAngle < 90.0f ) {
				HeadAnimations.SetDeferred( "flip_v", false );
			}

			if ( AimAngle > 89.0f ) {
				ArmAnimations.SetDeferred( "flip_v", true );
			} else if ( AimAngle < 90.0f ) {
				ArmAnimations.SetDeferred( "flip_v", false );
			}

			if ( Velocity != Godot.Vector2.Zero ) {
				HeadAnimations.CallDeferred( "play", "move" );
				BodyAnimations.CallDeferred( "play", "move" );
			} else {
//				if ( Awareness == MobAwareness.Relaxed ) {
//					BodyAnimations.CallDeferred( "play", "calm" );
//					HeadAnimations.CallDeferred( "hide" );
//					ArmAnimations.CallDeferred( "hide" );
//				} else {
					ArmAnimations.CallDeferred( "show" );
					HeadAnimations.CallDeferred( "show" );
					BodyAnimations.CallDeferred( "play", "idle" );
					HeadAnimations.CallDeferred( "play", "idle" );
//				}
			}
		}

		protected override void Think() {
			if ( !Visible ) {
				return;
			}

			CheckSight();

			if ( Target != null ) {
				if ( Target is Player ) {
					Player.InCombat = true;
				}
				if ( CanSeeTarget ) {
					CurrentState = State.Attacking;
					ChangeInvestigationAngleTimer.Stop();
				} else {
//					CurrentState = State.Investigating;
				}
			} else if ( PatrolRoute != null && GlobalPosition.DistanceTo( PatrolRoute.GetGlobalEndPosition() ) < 10.0f ) {
				PatrolRoute = PatrolRoute.GetNext();
				SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
			}

			switch ( CurrentState ) {
			case State.Investigating:
				if ( ChangeInvestigationAngleTimer.IsStopped() ) {
					ChangeInvestigationAngleTimer.CallDeferred( "start" );
				}
				if ( LoseInterestTimer.IsStopped() ) {
					LoseInterestTimer.CallDeferred( "start" );
				}
				if ( Target != null && CanSeeTarget ) {
					CurrentState = State.Attacking;
				}

				if ( Fear > 80 && CanSeeTarget ) {
					Bark( BarkType.Curse );
				}
				break;
			case State.Attacking:
				LastTargetPosition = SightTarget.GlobalPosition;

				Godot.Vector2 position1 = LastTargetPosition;
				Godot.Vector2 position2 = GlobalPosition;

				if ( GlobalPosition.DistanceTo( LastTargetPosition ) > 1024.0f ) {
//					Bark( BarkType.TargetRunning );
					Aiming = false;
					CurrentState = State.Investigating;
					AimTimer.Stop();

					const float interp = 0.5f;
					SetNavigationTarget( new Godot.Vector2( position1.X * ( 1 - interp ) + position2.X * interp, position1.Y * ( 1 - interp ) + position2.Y * interp ) );
				} else {
					if ( ( Aiming && AimTimer.TimeLeft > AimTimer.WaitTime * 0.15f ) || !Aiming ) {
						LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
						AimAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
						LookAngle = AimAngle;
					}
					if ( !Aiming ) {
						AimTimer.Start();
						Aiming = true;
					}
				}
				if ( Aiming && AimLine.GetCollider() is GodotObject collision && collision != null ) {
					if ( collision is Entity entity && entity != null && entity.GetHealth() > 0.0f && entity.GetFaction() == Faction ) {
						Bark( BarkType.OutOfTheWay );
						SetNavigationTarget( GlobalPosition + new Godot.Vector2( Godot.Vector2.Right.X + 50.0f, Godot.Vector2.Right.Y + 20.0f ) );
						AimTimer.Stop();
						Aiming = false;
					}
				}
				break;
			case State.Guarding:
				if ( Awareness > MobAwareness.Relaxed ) {
					CurrentState = State.Investigating;
				}
				break;
			};
		}

		private void CheckSight() {
			Entity sightTarget = null;
			for ( int i = 0; i < SightLines.Length; i++ ) {
				sightTarget = SightLines[i].GetCollider() as Entity;
				if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
					break;
				} else {
					sightTarget = null;
				}
			}

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
				CanSeeTarget = false;
				return;
			}

			if ( sightTarget != null ) {
				if ( sightTarget.GetHealth() <= 0.0f ) {
					// dead?
				} else {
					SightTarget = sightTarget;
					LastTargetPosition = sightTarget.GlobalPosition;
					CanSeeTarget = true;

					if ( Awareness >= MobAwareness.Suspicious ) {
						// if we're already suspicious, then detection rate increases as we're more alert
						SightDetectionAmount += SightDetectionSpeed * 2.0f;
					} else {
						SightDetectionAmount += SightDetectionSpeed;
					}
				}
			}

			if ( SightDetectionAmount >= SightDetectionTime * 0.25f && SightDetectionAmount < SightDetectionTime * 0.90f ) {
				SetSuspicious();
				CurrentState = State.Investigating;
				SetNavigationTarget( LastTargetPosition );
				if ( LoseInterestTimer.IsStopped() ) {
					LoseInterestTimer.Start();
				}
			} else if ( SightDetectionAmount >= SightDetectionTime * 0.90f ) {
				SetAlert();
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
				Family = FamilyCache.GetFamily( nodes[ Random.Next( 0, nodes.Count - 1 ) ] as Settlement, (SocietyRank)Random.Next( 0, (int)SocietyRank.Count ) );
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