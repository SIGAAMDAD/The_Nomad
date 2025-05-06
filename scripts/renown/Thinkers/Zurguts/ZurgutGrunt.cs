using Godot;

namespace Renown.Thinkers {
	public partial class ZurgutGrunt : Thinker {
		private enum State : uint {
			Idle,
			Attacking,
			Hunting,

			Count
		};

		private static readonly float BlowupDamage = 120.0f;

		private static readonly float AngleBetweenRays = Mathf.DegToRad( 8.0f );
		private static readonly float ViewAngleAmount = Mathf.DegToRad( 90.0f );
		private static readonly float MaxViewDistance = 220.0f;

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

		private Curve BlowupDamageCurve;
		private Area2D BlowupArea = null;
		private Timer BlowupTimer = null;

		private StringName MoveAnimation = "move";
		private StringName IdleAnimation = "idle";

		private AnimatedSprite2D ArmAnimations;
		private AnimatedSprite2D HeadAnimations;

		private float SightDetectionAmount = 0.0f;

		private Tween AngleTween;
		private Tween ChangeInvestigationAngleTween;

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

			// NOTE: this sound might be a little bit annoying to the sane mind
			PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_alert.ogg" ) );
			Awareness = MobAwareness.Alert;
			SetNavigationTarget( LastTargetPosition );
		}

		public void OnHeadHit( Entity source ) {
			BlowupArea.SetDeferred( "monitoring", true );
			BlowupArea.GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", false );
			CallDeferred( "OnBlowupTimerTimeout" );
		}

		private void OnDie( Entity source, Entity target ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			Flags |= ThinkerFlags.Dead;

			Health = 0.0f;

			HeadAnimations.Hide();
			ArmAnimations.Hide();
			BodyAnimations.Show();
			BodyAnimations.Play( "dead" );
			
			Enraged = false;
			BlowupTimer.Stop();
		}
		public override void Damage( Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );

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
				BlowupArea.SetDeferred( "monitoring", true );
				BlowupArea.GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", false );
				BlowupTimer.Start();
				PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_scream.ogg" ) );
			}
		}

		private void OnBlowupTimerTimeout() {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_blowup.ogg" ) );

			Godot.Collections.Array<Node2D> entities = BlowupArea.GetOverlappingBodies();
			for ( int i = 0; i < entities.Count; i++ ) {
				if ( entities[i] == this ) {
					continue;
				}
				if ( entities[i] is Entity entity && entity != null ) {
					float damage = BlowupDamage * BlowupDamageCurve.SampleBaked( entity.GlobalPosition.DistanceTo( GlobalPosition ) );
					entity.Damage( this, damage );
					if ( entity is Player player && player != null ) {
						player.ShakeCamera( damage );
					}
					if ( entity.GetHealth() > 0.0f ) {
						entity.AddStatusEffect( "status_burning" );
					}
				}
			}

			Health = 0.0f;
			OnDie( this, this );
		}

		public override void _Ready() {
			base._Ready();

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				AddToGroup( "Enemies" );
			}

			Die += OnDie;

			BlowupArea = GetNode<Area2D>( "BlowupArea" );
			BlowupArea.Monitoring = false;

			BlowupTimer = GetNode<Timer>( "BlowupTimer" );
			BlowupTimer.Connect( "timeout", Callable.From( OnBlowupTimerTimeout ) );

			BlowupDamageCurve = ResourceLoader.Load<Curve>( "res://resources/zurgut_grunt_blowup_damage_curve.tres" );

			HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
			ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );

			Hitbox HeadHitbox = HeadAnimations.GetNode<Hitbox>( "HeadHitbox" );
			HeadHitbox.Hit += OnHeadHit;

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
			case State.Hunting:
				break;
			case State.Idle:
				break;
			};
		}

		private void CheckSight() {
			/*
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
			*/
		}

		private void SwingHammer() {
			AngleTween = CreateTween();
			AngleTween.TweenProperty( ArmAnimations, "global_rotation", 80.0f, 2.5f );
		}
	};
};