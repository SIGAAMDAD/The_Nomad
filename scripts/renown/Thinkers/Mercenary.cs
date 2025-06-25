using System;
using ChallengeMode;
using Godot;
using PlayerSystem;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Mercenary : Thinker {
		public enum State : uint {
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
		public float SightDetectionTime {
			get;
			private set;
		} = 0.0f;
		[Export]
		private float SightDetectionSpeed = 0.0f;
		[Export]
		public Resource DefaultWeapon;
		[Export]
		public Resource DefaultAmmo;
		[Export]
		private AINodeCache NodeCache;
		[Export]
		private AIPatrolRoute PatrolRoute;
		[Export]
		private Resource Dialogue = ResourceLoader.Load( "res://resources/dialogue/mercenary.dialogue" );
		[Export]
		private string MercenaryReason;

		public float SightDetectionAmount {
			get;
			private set;
		} = 0.0f;

		private Hitbox HeadHitbox;
		public bool HitHead {
			get;
			private set;
		} = false;

		public State CurrentState {
			get;
			private set;
		}

		private Godot.Vector2 StartPosition;
		private float StartHealth;

		// combat variables
		private Timer AimTimer;
		private Timer AttackTimer;
		private RayCast2D AimLine;
		public bool Aiming {
			get;
			private set;
		} = false;
		private Line2D AttackMeter;
		private float AttackMeterProgress = 0.0f;
		private Godot.Vector2 AttackMeterFull;
		private Godot.Vector2 AttackMeterDone;

//		private HashSet<Entity> SightTargets = new HashSet<Entity>();
		private Entity SightTarget = null;

		private Timer LoseInterestTimer;
		private Timer ChangeInvestigationAngleTimer;
		private Timer TargetMovedTimer;

		public WeaponEntity Weapon {
			get;
			private set;
		}
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

		public AnimatedSprite2D HeadAnimations;
		public AnimatedSprite2D ArmAnimations;

		// if we have fear, move slower
		private float SpeedDegrade = 1.0f;

		public int Fear {
			get;
			private set;
		} = 0;

		private Color DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		private BarkType LastBark = BarkType.Count;
		private BarkType SequencedBark = BarkType.Count;

#region Dialogue
		private void BegForLife() {
			DialogueGlobals.Get().BotName = BotName;

			Player.StartDialogue( Dialogue, "beg_for_life", new Action<int>( ( choice ) => {
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
//			Bark( BarkType.Confusion );
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

		public Godot.Vector2 GetInvestigationPosition() => LastTargetPosition;
		public AIPatrolRoute GetPatrolRoute() => PatrolRoute;

		public override void AddStatusEffect( string effectName ) {
			base.AddStatusEffect( effectName );

			if ( Target is Player && Fear >= 80 ) {
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

		private void OnDeathAnimationFinished() {
			int finalFrame = BodyAnimations.SpriteFrames.GetFrameCount( BodyAnimations.Animation ) - 1;
			BodyAnimations.SpriteFrames.SetFrame( BodyAnimations.Animation, finalFrame, BodyAnimations.SpriteFrames.GetFrameTexture( BodyAnimations.Animation, finalFrame ) );
		}

		public override void Damage( in Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );
			PlaySound( AudioChannel, ResourceCache.Pain[ RNJesus.IntRange( 0, ResourceCache.Pain.Length - 1 ) ] );
			
			BloodParticleFactory.Create( Godot.Vector2.Zero, GlobalPosition );

			if ( Health <= 0.0f ) {
				AnimationStateMachine.CallDeferred( "fire_event", "die" );

				GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", true );
				SetDeferred( "collision_layer", 0 );
				SetDeferred( "collision_mask", 0 );

				HeadHitbox.SetDeferred( "monitoring", false );
				HeadHitbox.GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", true );

				DetectionMeter.CallDeferred( "hide" );

				AimTimer.Stop();
				Aiming = false;

				Velocity = Godot.Vector2.Zero;
				NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );

				GotoPosition = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;
				HeadAnimations.Hide();
				ArmAnimations.Hide();
				if ( HitHead ) {
					CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_low.ogg" ) );
					BodyAnimations.Connect( "animation_finished", Callable.From( OnDeathAnimationFinished ) );
				}
				return;
			}

			if ( Fear > 80 ) {
//				BegForLife();
				return;
			}

			if ( source != null ) {
				if ( source.GetFaction() == Faction ) {
					// "CEASEFIRE!"
					//				Bark( BarkType.Ceasefire );
				}

				LookDir = GlobalPosition.DirectionTo( source.GlobalPosition );
				float angle = Mathf.Atan2( LookDir.Y, LookDir.X );
				HeadAnimations.GlobalRotation = angle;
				ArmAnimations.GlobalRotation = angle;

				Target = source;
				LastTargetPosition = source.GlobalPosition;
				PatrolRoute = null;
				Awareness = MobAwareness.Alert;

				CurrentState = State.Attacking;
				SetNavigationTarget( LastTargetPosition );
			}
		}

		private void SetSuspicious() {
			if ( Awareness != MobAwareness.Suspicious ) {
				if ( !CanSeeTarget ) {
					// start patrolling near the last known position
					PatrolRoute = NodeCache.FindClosestRoute( LastTargetPosition );
				} else {
					//					SetNavigationTarget( LastTargetPosition );
				}
				Bark( BarkType.Confusion, Squad.GetMemberCount() > 0 ? BarkType.CheckItOut : BarkType.Count );
			}
			Awareness = MobAwareness.Suspicious;
			CurrentState = State.Investigating;
		}
		private void SetAlert() {
			if ( Awareness != MobAwareness.Alert ) {
				//				SetNavigationTarget( NodeCache.FindClosestCover( GlobalPosition, Target.GlobalPosition ).GlobalPosition );
				Bark( BarkType.TargetSpotted );
			}
			if ( GameConfiguration.GameMode != GameMode.JohnWick ) {
				Target = SightTarget;
			}
			SetNavigationTarget( LastTargetPosition );
			Awareness = MobAwareness.Alert;
		}

		private void SetFear( int nAmount ) {
			Fear = nAmount;
			if ( Fear >= 100 ) {
				SpeedDegrade = 0.0f;
//				ChangeInvestigationAngleTimer.WaitTime = 0.5f;
			} else if ( Fear >= 80 ) {
				SpeedDegrade = 0.25f;
//				ChangeInvestigationAngleTimer.WaitTime = 0.90f;
			} else if ( Fear >= 60 ) {
				SpeedDegrade = 0.5f;
//				ChangeInvestigationAngleTimer.WaitTime = 1.2f;
			} else {
				SpeedDegrade = 1.0f;
//				ChangeInvestigationAngleTimer.WaitTime = 2.0;
			}
		}

		public void LookAtTarget() {
			if ( SightTarget == null ) {
				return;
			}
			LookDir = GlobalPosition.DirectionTo( SightTarget.GlobalPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;
		}

		private bool IsValidTarget( Entity target ) => target is Player;
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
			DetectionMeter.SetDeferred( "default_color", DetectionColor );
		}
		private void OnChangeInvestigationAngleTimerTimeout() {
			float angle = RNJesus.FloatRange( 0, 360.0f );
			LookAngle = angle;
			AimAngle = angle;
//			ChangeInvestigationAngleTimer.CallDeferred( "start" );
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
			HitHead = true;
			CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_high.ogg" ) );
//			BodyAnimations.Play( "die_high" );
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

			Flags &= ~ThinkerFlags.Dead;

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", false );

			HeadHitbox.SetDeferred( "monitoring", true );
			HeadHitbox.GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", false );

			SetDeferred( "collision_layer", (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );
			SetDeferred( "collision_mask", (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );

			Target = null;
			SightTarget = null;

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
		}

		protected override void OnDie( Entity source, Entity target ) {
			base.OnDie( source, target );

			NavAgent.AvoidanceEnabled = false;

			SetDeferred( "collision_layer", (uint)PhysicsLayer.None );
			SetDeferred( "collision_mask", (uint)PhysicsLayer.None );

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
		private void OnHellbreakerBegin() {
			AudioChannel.ProcessMode = ProcessModeEnum.Disabled;
			NavAgent.ProcessMode = ProcessModeEnum.Disabled;

			ProcessMode = ProcessModeEnum.Disabled;
		}
		private void OnHellbreakerFinished() {
			AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
			NavAgent.ProcessMode = ProcessModeEnum.Pausable;

			ProcessMode = ProcessModeEnum.Pausable;
		}

		public override void _Ready() {
			base._Ready();

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				AddToGroup( "Enemies" );

				LevelData.Instance.PlayerRespawn += OnRestartCheckpoint;
				LevelData.Instance.HellbreakerBegin += OnHellbreakerBegin;
				LevelData.Instance.HellbreakerFinished += OnHellbreakerFinished;
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

			if ( GameConfiguration.GameMode != GameMode.JohnWick ) {
				NodeCache ??= Location.GetNodeCache();
			}

			LoseInterestTimer = new Timer();
			LoseInterestTimer.Name = "LoseInterestTimer";
			LoseInterestTimer.WaitTime = LoseInterestTime;
			LoseInterestTimer.OneShot = true;
			LoseInterestTimer.Connect( "timeout", Callable.From( OnLoseInterestTimerTimeout ) );
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
			AttackTimer.Connect( "timeout", Callable.From( () => { Aiming = false; } ) );
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
			Weapon.Data = DefaultWeapon;

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

			//			ChangeInvestigationAngleTimer = new Timer();
			//			ChangeInvestigationAngleTimer.Name = "ChangeInvestigationAngleTimer";
			//			ChangeInvestigationAngleTimer.OneShot = true;
			//			ChangeInvestigationAngleTimer.WaitTime = 2.5f;
			//			ChangeInvestigationAngleTimer.Connect( "timeout", Callable.From( OnChangeInvestigationAngleTimerTimeout ) );
			//			AddChild( ChangeInvestigationAngleTimer );

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

			if ( GameConfiguration.GameMode == GameMode.JohnWick ) {
				Target = LevelData.Instance.ThisPlayer;
				Awareness = MobAwareness.Alert;
				SightDetectionAmount = SightDetectionTime;
			}

			AnimationStateMachine.Call( "start" );
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
		public override void PickupWeapon( in WeaponEntity weapon ) {
			// TODO: evaluate if we actually want it
			Weapon = weapon;

			if ( ( Weapon.PropertyBits & WeaponEntity.Properties.IsFirearm ) != 0 ) {
				Weapon.SetUseMode( WeaponEntity.Properties.TwoHandedFirearm );
				AttackTimer.SetDeferred( "wait_time", Weapon.UseTime );
			//	AimTimer.SetDeferred( "wait_time", Weapon.GetReloadTime() );
				AimLine.SetDeferred( "target_position", Godot.Vector2.Right * (float)( (Godot.Collections.Dictionary)Ammo.Data.Get( "properties" ) )[ "range" ] );
			}
		}

		private void AttackMelee() {
			MeleeTween = CreateTween();
			MeleeTween.CallDeferred( "tween_property", ArmAnimations, "global_rotation", 0.0f, Weapon.Weight );
		}

		protected override void ProcessAnimations() {
			if ( !Visible || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( BodyAnimations.FlipH ) {
				ArmAnimations.SetDeferred( "sprite_frames", Weapon.AnimationsLeft );
			} else {
				ArmAnimations.SetDeferred( "sprite_frames", Weapon.AnimationsRight );
			}

			LookAtTarget();
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

		private void ResetAttackMeter() {
			AttackMeterProgress = AttackMeterFull.X;
			AttackMeter.Points[ 1 ] = AttackMeterFull;
			AttackMeter.Show();
		}
		private void UpdateAttackMeter() {
			AttackMeter.Points[ 1 ].X = AttackMeterProgress;
		}
		protected override void Think() {
			return;
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
//					ChangeInvestigationAngleTimer.Stop();
				} else {
					//					CurrentState = State.Investigating;
				}
			} else if ( PatrolRoute != null && GlobalPosition.DistanceTo( PatrolRoute.GetGlobalEndPosition() ) < 10.0f ) {
				PatrolRoute = PatrolRoute.GetNext();
				SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
			}

			if ( AimTimer.TimeLeft > 0.0f && Target is Player player && player != null ) {
				if ( ( player.GetFlags() & Player.PlayerFlags.Parrying ) != 0 ) {
					// shoot early
					AimTimer.Stop();
					OnAimTimerTimeout();
				}
			}

			switch ( CurrentState ) {
			case State.Investigating:
//				if ( ChangeInvestigationAngleTimer.IsStopped() ) {
//					ChangeInvestigationAngleTimer.CallDeferred( "start" );
//				}
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
				Godot.Vector2 position1 = LastTargetPosition;
				Godot.Vector2 position2 = GlobalPosition;

				if ( !AimTimer.IsStopped() ) {
					CallDeferred( "UpdateAttackMeter" );
				}

				if ( GlobalPosition.DistanceTo( LastTargetPosition ) > 1024.0f ) {
					//					Bark( BarkType.TargetRunning );
					Aiming = false;
					CurrentState = State.Investigating;
					AimTimer.CallDeferred( "stop" );

					const float interp = 0.5f;
					SetNavigationTarget( new Godot.Vector2( position1.X * ( 1 - interp ) + position2.X * interp, position1.Y * ( 1 - interp ) + position2.Y * interp ) );
				} else {
					if ( ( Aiming && AimTimer.TimeLeft > AimTimer.WaitTime * 0.15f ) || !Aiming ) {
						LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
						AimAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
						LookAngle = AimAngle;
					}
					if ( !Aiming ) {
						CallDeferred( "ResetAttackMeter" );

						Tween Tweener = CreateTween();
						Tweener.CallDeferred( "tween_property", this, "AttackMeterProgress", AttackMeterDone.X, AimTimer.WaitTime );
						Tweener.CallDeferred( "connect", "finished", Callable.From( AttackMeter.Hide ) );

						AimTimer.CallDeferred( "start" );
						Aiming = true;
					}
				}
				if ( Aiming && AimLine.GetCollider() is GodotObject collision && collision != null ) {
					if ( collision is Entity entity && entity != null && entity.GetHealth() > 0.0f && entity.GetFaction() == Faction ) {
						Bark( BarkType.OutOfTheWay );
						SetNavigationTarget( GlobalPosition + new Godot.Vector2( Godot.Vector2.Right.X + 50.0f, Godot.Vector2.Right.Y + 20.0f ) );

						AttackMeter.CallDeferred( "hide" );

						AimTimer.CallDeferred( "stop" );
						Aiming = false;
					}
				}
				break;
			case State.Guarding:
				if ( Awareness > MobAwareness.Relaxed ) {
					CurrentState = State.Investigating;
				}
				break;
			}
			;
		}

		public void CheckSight() {
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