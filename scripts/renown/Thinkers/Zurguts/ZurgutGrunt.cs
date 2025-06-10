using ChallengeMode;
using Godot;

namespace Renown.Thinkers {
	public partial class ZurgutGrunt : Thinker {
		private enum State : uint {
			Idle,
			Attacking,

			Count
		};

		private static readonly float BlowupDamage = 120.0f;

		private static readonly float AngleBetweenRays = Mathf.DegToRad( 8.0f );
		private static readonly float ViewAngleAmount = Mathf.DegToRad( 90.0f );
		private static readonly float MaxViewDistance = 220.0f;

		private static readonly int ChallengeMode_Score = 60;

		[Export]
		private float LoseInterestTime = 0.0f;
		[Export]
		private float SightDetectionTime = 5.0f;
		[Export]
		private float SightDetectionSpeed = 0.75f;

		// deaf, but their eyesight is incredible
		private RayCast2D[] SightLines;
		private MobAwareness Awareness = MobAwareness.Relaxed;
		private bool CanSeeTarget = false;
		private Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;

		//private HashSet<Entity> SightTargets = new HashSet<Entity>();
		private Entity SightTarget = null;

		private State CurrentState = State.Idle;

		private bool Enraged = false;
		private Entity Target = null;

		private Line2D DetectionMeter;

		private float StartHealth = 0.0f;
		private Godot.Vector2 StartPosition = Godot.Vector2.Zero;

		private Curve BlowupDamageCurve;
		private Timer BlowupTimer = null;

		private Color DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		private StringName MoveAnimation = "move";
		private StringName IdleAnimation = "idle";

		private AnimatedSprite2D ArmAnimations;
		private AnimatedSprite2D HeadAnimations;

		private float SightDetectionAmount = 0.0f;

		private CollisionShape2D HammerShape;
		private Area2D AreaOfEffect;
		private Timer AttackTimer;
		private Tween AngleTween;
		private Tween ChangeInvestigationAngleTween;

		private bool HitHead = false;

		public bool IsAlert() => Awareness == MobAwareness.Alert || SightDetectionAmount >= SightDetectionTime;
		public bool IsSuspicious() => Awareness == MobAwareness.Suspicious || SightDetectionAmount >= SightDetectionTime * 0.25f;

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

		private void GenerateRayCasts() {
			int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
			SightLines = new RayCast2D[ rayCount ];
			for ( int i = 0; i < rayCount; i++ ) {
				RayCast2D ray = new RayCast2D();
				float angle = AngleBetweenRays * ( i - rayCount / 2.0f );
				ray.TargetPosition = Godot.Vector2.Right.Rotated( angle ) * MaxViewDistance;
				ray.Enabled = true;
				ray.CollisionMask = 2;
				SightLines[i] = ray;
				HeadAnimations.AddChild( ray );
			}
		}

		// TODO: make the "valid target" thing for the grunt a lot looser
		private bool IsValidTarget( GodotObject target ) => target is Entity entity && entity != null && entity.GetFaction() != Faction;

		private void SetAlert() {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( Awareness == MobAwareness.Alert ) {
				SetNavigationTarget( LastTargetPosition );
				return;
			}

			// NOTE: this sound might be a little bit annoying to the sane mind
			PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_alert.ogg" ) );
			Awareness = MobAwareness.Alert;
//			SetNavigationTarget( LastTargetPosition );
		}

		public void OnHeadHit( Entity source ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			CallDeferred( "OnBlowupTimerTimeout" );
			HitHead = true;
		}

		protected override void OnDie( Entity source, Entity target ) {
			base.OnDie( source, target );

			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( source is Player ) {
				if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
					if ( BodyAnimations.Animation == "die_high" ) {
						ChallengeLevel.IncreaseScore( ChallengeMode_Score * ChallengeCache.ScoreMultiplier_HeadShot * Player.ComboCounter );
						System.Threading.Interlocked.Increment( ref ChallengeLevel.HeadshotCounter );
					} else if ( BodyAnimations.Animation == "die_low" ) {
						ChallengeLevel.IncreaseScore( ChallengeMode_Score * Player.ComboCounter );
					}
				} else if ( GameConfiguration.GameMode == GameMode.JohnWick ) {
					if ( BodyAnimations.Animation == "die_high" ) {
						JohnWickMode.IncreaseScore( ChallengeMode_Score * ChallengeCache.ScoreMultiplier_HeadShot * Player.ComboCounter );
						System.Threading.Interlocked.Increment( ref ChallengeLevel.HeadshotCounter );
					} else if ( BodyAnimations.Animation == "die_low" ) {
						JohnWickMode.IncreaseScore( ChallengeMode_Score * Player.ComboCounter );
					}
				}
			}

			Flags |= ThinkerFlags.Dead;

			Health = 0.0f;

			DetectionMeter.CallDeferred( "hide" );

			HeadAnimations.Hide();
			ArmAnimations.Hide();
			BodyAnimations.Show();
			BodyAnimations.Play( "dead" );
			
			Enraged = false;
			BlowupTimer.Stop();
		}
		public override void Damage( in Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );

			if ( Health <= 0.0f ) {
				ArmAnimations.Hide();
				HeadAnimations.Hide();
				BodyAnimations.Play( "dead" );

				GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", true );
				SetDeferred( "collision_layer", 0 );
				SetDeferred( "collision_mask", 0 );

				GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).SetDeferred( "monitoring", false );
				GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", true );

				DetectionMeter.CallDeferred( "hide" );

				Velocity = Godot.Vector2.Zero;
				NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );
				
				GotoPosition = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;
				HeadAnimations.Hide();
				ArmAnimations.Hide();
				CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_high.ogg" ) );
				BodyAnimations.CallDeferred( "play", "dead" );
				return;
			}

			if ( Health < Health * 0.20f ) {
				MoveAnimation = "move_fatal";
				IdleAnimation = "idle_fatal";
			} else if ( Health < Health * 0.60f ) {
				MoveAnimation = "move_wounded";
				IdleAnimation = "idle_wounded";
			}

			Target = source;

			if ( !Enraged && Health < Health * 0.25f ) {
				Enraged = true;
				BlowupTimer.Start();
				PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_scream.ogg" ) );
			}
		}

		private void OnBlowupTimerTimeout() {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_blowup.ogg" ) );

			Explosion explosion = ResourceCache.GetScene( "res://scenes/effects/big_explosion.tscn" ).Instantiate<Explosion>();
			explosion.Radius = 126.0f;
			explosion.Damage = BlowupDamage;
			explosion.DamageCurve = BlowupDamageCurve;
			AddChild( explosion );

			Health = 0.0f;
			OnDie( this, this );
		}

		private void OnHammerSwingFinished() {
			Explosion explosion = ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>();
			explosion.Radius = ( HammerShape.Shape as CircleShape2D ).Radius;
			explosion.Damage = BlowupDamage;
			explosion.DamageCurve = BlowupDamageCurve;
			explosion.Effects = AmmoEntity.ExtraEffects.Incendiary;
			AddChild( explosion );

			AreaOfEffect.SetDeferred( "monitoring", false );
			HammerShape.SetDeferred( "disabled", true );
			ArmAnimations.CallDeferred( "play", "idle" );
		}

		private void OnPlayerRespawn() {
			GlobalPosition = StartPosition;
			Health = StartHealth;

			DetectionMeter.CallDeferred( "show" );
			ArmAnimations.CallDeferred( "show" );
			HeadAnimations.CallDeferred( "show" );

			NavAgent.AvoidanceEnabled = true;

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", false );
			GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", false );

			SetDeferred( "collision_layer", (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );
			SetDeferred( "collision_mask", (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );

			Flags &= ~ThinkerFlags.Dead;

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", false );
			GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", false );

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

				LevelData.Instance.PlayerRespawn += OnPlayerRespawn;
				LevelData.Instance.HellbreakerBegin += OnHellbreakerBegin;
				LevelData.Instance.HellbreakerFinished += OnHellbreakerFinished;
			}

			Die += OnDie;

			StartHealth = Health;
			StartPosition = GlobalPosition;

			DetectionMeter = GetNode<Line2D>( "DetectionMeter" );

			HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );

			ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );

			AreaOfEffect = GetNode<Area2D>( "AreaOfEffect" );
			AreaOfEffect.CollisionLayer = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity );
			AreaOfEffect.CollisionMask = (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity );
			AreaOfEffect.Monitoring = false;

			HammerShape = GetNode<CollisionShape2D>( "AreaOfEffect/CollisionShape2D" );
			HammerShape.Disabled = true;
			//			AreaOfEffect.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnAreaOfEffectShape2DEntered ) );
			//			AreaOfEffect.Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnAreaOfEffectShape2DExited ) );

			AttackTimer = new Timer();
			AttackTimer.OneShot = true;
			AttackTimer.WaitTime = 2.0f;
			AddChild( AttackTimer );

			BlowupTimer = GetNode<Timer>( "BlowupTimer" );
			BlowupTimer.Connect( "timeout", Callable.From( OnBlowupTimerTimeout ) );

			BlowupDamageCurve = ResourceLoader.Load<Curve>( "res://resources/zurgut_grunt_blowup_damage_curve.tres" );

			Hitbox HeadHitbox = HeadAnimations.GetNode<Hitbox>( "HeadHitbox" );
			HeadHitbox.Hit += OnHeadHit;

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
				break;
			}
			;
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;

			GenerateRayCasts();
		}

		protected override void ProcessAnimations() {
			if ( !Visible || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( Target != null ) {
				if ( CanSeeTarget ) {
					LastTargetPosition = Target.GlobalPosition;
				}
				LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
				AimAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
				LookAngle = AimAngle;
			}

			ArmAnimations.SetDeferred( "global_rotation", AimAngle );
			HeadAnimations.SetDeferred( "global_rotation", LookAngle );

			if ( Velocity.X < 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", true );
			} else if ( Velocity.X > 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", false );
			}

			if ( LookAngle > 225.0f ) {
				HeadAnimations.SetDeferred( "flip_v", true );
			} else if ( LookAngle < 135.0f ) {
				HeadAnimations.SetDeferred( "flip_v", false );
			}

			if ( AimAngle > 225.0f ) {
				ArmAnimations.SetDeferred( "flip_v", true );
			} else if ( AimAngle < 135.0f ) {
				ArmAnimations.SetDeferred( "flip_v", false );
			}

			if ( Velocity != Godot.Vector2.Zero ) {
				HeadAnimations.CallDeferred( "play", MoveAnimation );
				BodyAnimations.CallDeferred( "play", MoveAnimation );
			} else {
//				if ( Awareness == MobAwareness.Relaxed ) {
//					BodyAnimations.CallDeferred( "play", "calm" );
//					HeadAnimations.CallDeferred( "hide" );
//					ArmAnimations.CallDeferred( "hide" );
//				} else {
					ArmAnimations.CallDeferred( "show" );
					HeadAnimations.CallDeferred( "show" );
					BodyAnimations.CallDeferred( "play", IdleAnimation );
					HeadAnimations.CallDeferred( "play", IdleAnimation );
//				}
			}
		}
		protected override void Think() {
			CheckSight();

			if ( Enraged ) {
				return;
			}

			switch ( CurrentState ) {
			case State.Attacking:
				if ( GlobalPosition.DistanceTo( LastTargetPosition ) < 80.0f && AttackTimer.IsStopped() ) {
					ArmAnimations.CallDeferred( "play", "attack" );
					AttackTimer.CallDeferred( "start" );
					StopMoving();
					CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( "res://sounds/player/melee.wav" ) );
					CallDeferred( "SwingHammer" );
				} else {
					SetNavigationTarget( LastTargetPosition );
				}
				break;
			case State.Idle:
				break;
			};
		}

		protected override bool MoveAlongPath() {
			if ( NavAgent.IsTargetReached() ) {
				Velocity = Godot.Vector2.Zero;
				return true;
			}
			Godot.Vector2 nextPathPosition = NavAgent.GetNextPathPosition();
			LookDir = GlobalPosition.DirectionTo( nextPathPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			Velocity = LookDir * MovementSpeed;
			GlobalPosition += Velocity * (float)GetPhysicsProcessDeltaTime();
			return true;
		}
		private void CheckSight() {
			if ( Awareness == MobAwareness.Alert ) {
				return;
			}

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
					Target = sightTarget;
					LastTargetPosition = sightTarget.GlobalPosition;
					CanSeeTarget = true;

					SightDetectionAmount += SightDetectionSpeed * 2.0f;
				}
			}

			if ( SightDetectionAmount >= SightDetectionTime * 0.90f ) {
				SetAlert();
				CurrentState = State.Attacking;
//				SetNavigationTarget( LastTargetPosition );
			} else if ( SightDetectionAmount >= SightDetectionTime * 0.90f ) {
				SetAlert();
			}
			if ( IsAlert() ) {
				SetAlert();
			}
			SetDetectionColor();
		}

		private void SwingHammer() {
			AngleTween = CreateTween();
			AreaOfEffect.Monitoring = true;
			HammerShape.Disabled = false;
			ArmAnimations.GlobalRotationDegrees = 0.0f;
			AngleTween.TweenProperty( ArmAnimations, "global_rotation_degrees", 180.0f, 1.5f );
			AngleTween.Connect( "finished", Callable.From( OnHammerSwingFinished ) );
		}
	};
};