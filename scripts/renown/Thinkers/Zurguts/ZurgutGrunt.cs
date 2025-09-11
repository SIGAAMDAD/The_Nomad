/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using ResourceCache;
using Items;

namespace Renown.Thinkers {
	public partial class ZurgutGrunt : MobBase {
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
			}
			DetectionMeter.SetDeferred( Line2D.PropertyName.DefaultColor, DetectionColor );
		}

		// TODO: make the "valid target" thing for the grunt a lot looser
		private bool IsValidTarget( GodotObject target ) {
			return target is Entity entity && entity != null && entity.Faction != Faction;
		}

		private void SetAlert() {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( Awareness == MobAwareness.Alert ) {
				SetNavigationTarget( LastTargetPosition );
				return;
			}

			// NOTE: this sound might be a little bit annoying to the sane mind
			PlaySound( null, AudioCache.GetStream( "res://sounds/mobs/zurgut_grunt_alert.ogg" ) );
			Awareness = MobAwareness.Alert;
			//			SetNavigationTarget( LastTargetPosition );
		}

		public void OnHeadHit( Entity source, float amount ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			CallDeferred( MethodName.OnBlowupTimerTimeout );
			HitHead = true;
		}

		protected override void OnDie( Entity source, Entity target ) {
			base.OnDie( source, target );

			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( source is Player ) {
				if ( GameConfiguration.GameMode == GameMode.ChallengeMode || GameConfiguration.GameMode == GameMode.JohnWick ) {
					if ( BodyAnimations.Animation == "die_high" ) {
						FreeFlow.AddKill( KillType.Headshot, ChallengeMode_Score );
					} else if ( BodyAnimations.Animation == "die_low" ) {
						FreeFlow.AddKill( KillType.Bodyshot, ChallengeMode_Score );
					}
				}
			}

			Flags |= ThinkerFlags.Dead;

			Health = 0.0f;

			DetectionMeter.CallDeferred( MethodName.Hide );

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

				GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );
				SetDeferred( PropertyName.CollisionLayer, 0 );
				SetDeferred( PropertyName.CollisionMask, 0 );

				GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).SetDeferred( Area2D.PropertyName.Monitoring, false );
				GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, true );

				DetectionMeter.CallDeferred( "hide" );

				Velocity = Godot.Vector2.Zero;
				NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );

				GotoPosition = Godot.Vector2.Zero;
				Flags |= ThinkerFlags.Dead;
				HeadAnimations.Hide();
				ArmAnimations.Hide();
				CallDeferred( MethodName.PlaySound, AudioChannel, AudioCache.GetStream( "res://sounds/mobs/die_high.ogg" ) );
				BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "dead" );
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
				PlaySound( null, AudioCache.GetStream( "res://sounds/mobs/zurgut_grunt_scream.ogg" ) );
			}
		}

		private void OnBlowupTimerTimeout() {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			PlaySound( null, AudioCache.GetStream( "res://sounds/mobs/zurgut_grunt_blowup.ogg" ) );

			Explosion explosion = SceneCache.GetScene( "res://scenes/effects/big_explosion.tscn" ).Instantiate<Explosion>();
			explosion.Radius = 126.0f;
			explosion.Damage = BlowupDamage;
			explosion.DamageCurve = BlowupDamageCurve;
			AddChild( explosion );

			Health = 0.0f;
			OnDie( this, this );
		}

		private void OnHammerSwingFinished() {
			Explosion explosion = SceneCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>();
			explosion.Radius = ( HammerShape.Shape as CircleShape2D ).Radius;
			explosion.Damage = BlowupDamage;
			explosion.DamageCurve = BlowupDamageCurve;
			explosion.Effects = ExtraAmmoEffects.Incendiary;
			AddChild( explosion );

			AreaOfEffect.SetDeferred( Area2D.PropertyName.Monitoring, false );
			HammerShape.SetDeferred( CollisionShape2D.PropertyName.Disabled, true );
			ArmAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
		}

		private void OnPlayerRespawn() {
			GlobalPosition = StartPosition;
			Health = StartHealth;

			DetectionMeter.CallDeferred( MethodName.Show );
			ArmAnimations.CallDeferred( MethodName.Show );
			HeadAnimations.CallDeferred( MethodName.Show );

			NavAgent.AvoidanceEnabled = true;

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, false );
			GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetChild<CollisionShape2D>( 0 ).SetDeferred( CollisionShape2D.PropertyName.Disabled, false );

			SetDeferred( PropertyName.CollisionLayer, (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );
			SetDeferred( PropertyName.CollisionMask, (uint)( PhysicsLayer.SpriteEntity | PhysicsLayer.Player ) );

			Flags &= ~ThinkerFlags.Dead;

			GetNode<CollisionShape2D>( "CollisionShape2D" ).SetDeferred( CollisionShape2D.PropertyName.Disabled, false );
			GetNode<Hitbox>( "Animations/HeadAnimations/HeadHitbox" ).GetChild<CollisionShape2D>( 0 ).SetDeferred( CollisionShape2D.PropertyName.Disabled, false );

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
			}
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
			BlowupTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnBlowupTimerTimeout ) );

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
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;
		}

		protected override void ProcessAnimations() {
			if ( SightTarget != null ) {
				LookAtTarget();
			}
			base.ProcessAnimations();
		}
		protected override void Think() {
			CheckSight();
		}

		public void LookAtTarget() {
			if ( SightTarget == null ) {
				return;
			}
			LookDir = GlobalPosition.DirectionTo( SightTarget.GlobalPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;
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