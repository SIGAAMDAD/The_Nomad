using System;
using ChallengeMode;
using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class GatlingGunner : Thinker {
		private enum State : uint {
			Guarding,
			Attacking,
			Investigating,

			Count
		};

		private static readonly float ExplosionDamage = 80.0f;
		private static readonly float BulletDamage = 1.5f;

		private static readonly float AngleBetweenRays = Mathf.DegToRad( 8.0f );
		private static readonly float ViewAngleAmount = Mathf.DegToRad( 60.0f );
		private static readonly float MaxViewDistance = 220.0f;

		private static readonly int ChallengeMode_Score = 50;

		[Export]
		private float LoseInterestTime = 0.0f;
		[Export]
		private float SightDetectionTime = 0.0f;
		[Export]
		private float SightDetectionSpeed = 0.0f;
		[Export]
		private AINodeCache NodeCache;

		private float SightDetectionAmount = 0.0f;

		private State CurrentState;

		private Godot.Vector2 StartPosition = Godot.Vector2.Zero;

		// combat variables
		private Timer RevTimer;
		private RayCast2D AimLine;
		private bool Revved = false;
		private bool Shooting = false;
		private AudioStreamPlayer2D GunChannel;

//		private HashSet<Entity> SightTargets = new HashSet<Entity>();
		private Entity SightTarget = null;

		private Timer LoseInterestTimer;
		private Timer ChangeInvestigationAngleTimer;

		private MobAwareness Awareness = MobAwareness.Relaxed;

		private Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;
		private RayCast2D[] SightLines = null;
		private bool CanSeeTarget = false;

		private Entity Target;

		private Line2D DetectionMeter;

		private bool HitHead = false;

		private AnimatedSprite2D HeadAnimations;
		private AnimatedSprite2D ArmAnimations;

		// if we have fear, move slower
		private float SpeedDegrade = 1.0f;

		private int Fear = 0;

		private Color DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

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

		public override void Alert( Entity source ) {
			return; // deaf, but they have a mix track playing inside that helmet
		}
		private float RandomFloat( float min, float max ) {
			return (float)( min + Random.NextDouble() * ( min - max ) );
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

		public override void Damage( Entity source, float nAmount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( HitHead ) {
				base.Damage( source, nAmount );
				PlaySound( AudioChannel, ResourceCache.Pain[ Random.Next( 0, ResourceCache.Pain.Length - 1 ) ] );
			}

			if ( Health <= 0.0f ) {
				DetectionMeter.CallDeferred( "hide" );
				
				GunChannel.Stop();
				GunChannel.Set( "parameters/looping", false );

				AudioChannel.Stop();
				AudioChannel.Set( "parameters/looping", false );

				Velocity = Godot.Vector2.Zero;
				NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );
				GotoPosition = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;
				HeadAnimations.CallDeferred( "hide" );
				ArmAnimations.CallDeferred( "hide" );
//				if ( BodyAnimations.Animation != "die_high" ) {
//					PlaySound( AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_low.ogg" ) );
					BodyAnimations.CallDeferred( "play", "die" );
//				}

				GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", true );
				GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", true );
				SetDeferred( "collision_layer", 0 );
				SetDeferred( "collision_mask", 0 );
				return;
			}

			if ( Awareness == MobAwareness.Alert ) {

			} else {
				SetAlert();
			}

			float angle = RandomFloat( 0, 360.0f );
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
		}
		private void SetAlert() {
			Target = SightTarget;
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

		public void OnHeadHit( Entity source ) {
//			CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/die_high.ogg" ) );
//			BodyAnimations.Play( "die_high" );
			HitHead = true;
			Damage( source, Health );
		}
		public void OnBackpackHit( Entity source ) {
			CallDeferred( "BlowupBackpack" );
		}

		private void BlowupBackpack() {
			Explosion explosion = ResourceCache.GetScene( "res://scenes/effects/big_explosion.tscn" ).Instantiate<Explosion>();
			explosion.Radius = 72.0f;
			explosion.Damage = ExplosionDamage;
			explosion.Effects = AmmoEntity.ExtraEffects.Incendiary;
			AddChild( explosion );

			base.Damage( this, Health );
			DetectionMeter.CallDeferred( "hide" );
			
			GunChannel.Stop();
			GunChannel.Set( "parameters/looping", false );

			AudioChannel.Stop();
			AudioChannel.Set( "parameters/looping", false );

			GotoPosition = Godot.Vector2.Zero;
			Flags |= ThinkerFlags.Dead;
			HeadAnimations.Hide();
			ArmAnimations.Hide();
			BodyAnimations.CallDeferred( "play", "die" );

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", true );
		}

		private void OnRevTimerTimeout() {
			Revved = !Revved;
		}
		private void OnLoseInterestTimerTimeout() {
			GunChannel.CallDeferred( "stop" );
			GunChannel.SetDeferred( "parameters/looping", false );

			AudioChannel.CallDeferred( "stop" );
			AudioChannel.SetDeferred( "parameters/looping", false );

			ArmAnimations.CallDeferred( "play", "idle" );

			CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( "res://sounds/mobs/gatling_dissapointed.ogg" ) );
			if ( Shooting || Revved ) {
				CallDeferred( "PlaySound", GunChannel, ResourceCache.GetSound( "res://sounds/mobs/gatling_revdown.ogg" ) );
				RevTimer.CallDeferred( "start" );
			}
			Shooting = false;
			Revved = false;

			CurrentState = State.Investigating;
		}

		private void OnPlayerRestart() {
			GlobalPosition = StartPosition;

			DetectionMeter.CallDeferred( "show" );
			ArmAnimations.CallDeferred( "show" );
			HeadAnimations.CallDeferred( "show" );

			SetDeferred( "collision_layer", (uint)( PhysicsLayer.SpriteEntity ) );
			SetDeferred( "collision_mask", (uint)( PhysicsLayer.SpriteEntity ) );

			Flags &= ~ThinkerFlags.Dead;

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( "disabled", false );
			GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", false );
		}

		protected override void OnDie( Entity source, Entity target ) {
			base.OnDie( source, target );
			
			if ( source is Player && GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				if ( HitHead ) {
					ChallengeLevel.IncreaseScore( ChallengeMode_Score * ChallengeCache.ScoreMultiplier_HeadShot * Player.ComboCounter );
					System.Threading.Interlocked.Increment( ref ChallengeLevel.HeadshotCounter );
				} else {
					ChallengeLevel.IncreaseScore( ChallengeMode_Score * Player.ComboCounter );
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				AddToGroup( "Enemies" );

				LevelData.Instance.PlayerRespawn += OnPlayerRestart;
			}

			GunChannel = new AudioStreamPlayer2D();
			GunChannel.Name = "GunChannel";
			GunChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AddChild( GunChannel );

			HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );

			ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );
//			ArmAnimations.AnimationFinished += OnArmAnimationFinished;

			Hitbox HeadHitbox = HeadAnimations.GetNode<Hitbox>( "HeadHitbox" );
			HeadHitbox.Hit += OnHeadHit;

			// the massive oxygen tank on their back, highly dangerous and explosive
			Hitbox BackpackHitbox = BodyAnimations.GetNode<Hitbox>( "BackpackHitbox" );
			BackpackHitbox.Hit += OnBackpackHit;

			CurrentState = State.Guarding;

			NodeCache ??= Location.GetNodeCache();

			RevTimer = new Timer();
			RevTimer.WaitTime = 2.5f;
			RevTimer.Connect( "timeout", Callable.From( OnRevTimerTimeout ) );
			AddChild( RevTimer );

			AimLine = new RayCast2D();
			AimLine.Name = "AimLine";
			AimLine.TargetPosition = Godot.Vector2.Right;
			AimLine.CollideWithAreas = true;
			AimLine.CollideWithBodies = true;
			AimLine.CollisionMask = (uint)( PhysicsLayer.Player | PhysicsLayer.SpecialHitboxes | PhysicsLayer.SpriteEntity );
			ArmAnimations.AddChild( AimLine );

			LoseInterestTimer = new Timer();
			LoseInterestTimer.WaitTime = LoseInterestTime;
			LoseInterestTimer.OneShot = true;
			LoseInterestTimer.Connect( "timeout", Callable.From( OnLoseInterestTimerTimeout ) );
			AddChild( LoseInterestTimer );

			DetectionMeter = GetNode<Line2D>( "DetectionMeter" );

			ChangeInvestigationAngleTimer = new Timer();
			ChangeInvestigationAngleTimer.Name = "ChangeInvestigationAngleTimer";
			ChangeInvestigationAngleTimer.OneShot = true;
			ChangeInvestigationAngleTimer.WaitTime = 1.0f;
			ChangeInvestigationAngleTimer.Connect( "timeout", Callable.From( OnChangeInvestigationAngleTimerTimeout ) );
			AddChild( ChangeInvestigationAngleTimer );

			StartPosition = GlobalPosition;

			GenerateRayCasts();

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
			};
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;
		}

		protected override void ProcessAnimations() {
			if ( !Visible || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
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
		private void CalcShots() {
			const int numShots = 24;
			
			for ( int i = 0; i < numShots; i++ ) {
				AimLine.TargetPosition = Godot.Vector2.Right.Rotated( Mathf.DegToRad( RandomFloat( 0.0f, 60.0f ) ) ) * 1024.0f;
				AimLine.ForceRaycastUpdate();

				GodotObject collision = AimLine.GetCollider();
				if ( collision != null ) {
					if ( collision is Entity entity && entity != null ) {
						entity.Damage( this, BulletDamage );
					} else {
						DebrisFactory.Create( AimLine.GetCollisionPoint() );
					}
				}
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
				if ( CanSeeTarget && Awareness == MobAwareness.Alert ) {
					CurrentState = State.Attacking;
					ChangeInvestigationAngleTimer.Stop();
				} else {
//					CurrentState = State.Investigating;
				}
			}

			switch ( CurrentState ) {
			case State.Investigating:
				if ( ChangeInvestigationAngleTimer.IsStopped() ) {
					ChangeInvestigationAngleTimer.CallDeferred( "start" );
				}
				if ( Target != null && CanSeeTarget ) {
					CurrentState = State.Attacking;
				}
				break;
			case State.Attacking:
				if ( !CanSeeTarget && LoseInterestTimer.IsStopped() ) {
					LoseInterestTimer.CallDeferred( "start" );
				}
				if ( !Shooting && CanSeeTarget ) {
					CallDeferred( "PlaySound", GunChannel, ResourceCache.GetSound( "res://sounds/mobs/gatling_aiming.ogg" ) );
					GunChannel.SetDeferred( "parameters/looping", false );
					AudioChannel.SetDeferred( "parameters/looping", false );
					Shooting = true;
					ArmAnimations.CallDeferred( "play", "aim" );

					SetNavigationTarget( LastTargetPosition );

					LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
					LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
					AimAngle = LookAngle;
				} else if ( Revved && Shooting ) {
					if ( ArmAnimations.Animation != "attack" ) {
						CallDeferred( "PlaySound", GunChannel, ResourceCache.GetSound( "res://sounds/mobs/gatling_shooting.ogg" ) );
						GunChannel.SetDeferred( "parameters/looping", true );
						CallDeferred( "PlaySound", AudioChannel, ResourceCache.GetSound( string.Format( "res://sounds/mobs/gatling_laughter{0}.ogg", Random.Next( 0, 7 ) ) ) );
						AudioChannel.SetDeferred( "parameters/looping", true );
					}
					ArmAnimations.CallDeferred( "play", "attack" );
					CallDeferred( "CalcShots" );
				}
				if ( !Revved && CanSeeTarget ) {
					if ( RevTimer.IsStopped() ) {
						PlaySound( GunChannel, ResourceCache.GetSound( "res://sounds/mobs/gatling_revup.ogg" ) );
						RevTimer.CallDeferred( "start" );
					}
					ArmAnimations.CallDeferred( "play", "aim" );
				}
				break;
			case State.Guarding:
				if ( Awareness > MobAwareness.Relaxed ) {
					CurrentState = State.Investigating;
				}
				break;
			};
		}

		protected override bool MoveAlongPath() {
			float movespeed = MovementSpeed;
			if ( Shooting ) {
				MovementSpeed = 50.0f;
			} else {
				MovementSpeed = movespeed;
			}
			base.MoveAlongPath();
			MovementSpeed = movespeed;
			return true;
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