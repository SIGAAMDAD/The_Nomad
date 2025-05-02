using Godot;

namespace Renown.Thinkers {
	public partial class ZurgutGrunt : Thinker {
		private static readonly float BlowupDamage = 80.0f;

		[Export]
		private float SightDetectionTime = 5.0f;
		[Export]
		private float SightDetectionSpeed = 0.75f;

		// deaf, but their eyesight is incredible
		private RayCast2D[] SightLines;
		private MobAwareness Awareness = MobAwareness.Relaxed;
		private bool CanSeeTarget = false;
		private Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;

		private Curve BlowupDamageCurve;

		private bool Enraged = false;
		private Entity Target = null;

		private Area2D BlowupArea = null;
		private Timer BlowupTimer = null;

		private AnimatedSprite2D ArmAnimations;
		private AnimatedSprite2D HeadAnimations;

		private float SightDetectionAmount = 0.0f;

		private Tween AngleTween;
		private Tween ChangeInvestigationAngleTween;

		// TODO: make the "valid target" thing for the grunt a lot looser
		private bool IsValidTarget( GodotObject target ) => target is Entity entity && entity != null && entity.GetFaction() != Faction;

		private void SetAlert() {
			// NOTE: this sound might be a little bit annoying to the sane mind
			PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_alert.ogg" ) );
			Awareness = MobAwareness.Alert;
			SetNavigationTarget( LastTargetPosition );
		}

		private void OnDie( Entity source, Entity target ) {
			Enraged = false;
			BlowupTimer.Stop();
		}
		public override void Damage( Entity source, float nAmount ) {
			base.Damage( source, nAmount );

			Target = source;

			if ( !Enraged && Health < Health * 0.25f ) {
				BlowupArea.SetDeferred( "monitoring", true );
				BlowupArea.GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", false );
				PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_scream.ogg" ) );
			}
		}

		private void OnBlowupTimerTimeout() {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			PlaySound( null, ResourceCache.GetSound( "res://sounds/mobs/zurgut_grunt_blowup.ogg" ) );
			Damage( this, 1000.0f );

			Godot.Collections.Array<Node2D> entities = BlowupArea.GetOverlappingBodies();
			for ( int i = 0; i < entities.Count; i++ ) {
				if ( entities[i] == this ) {
					continue;
				}
				if ( entities[i] is Entity entity && entity != null ) {
					entity.Damage( this, BlowupDamage * BlowupDamageCurve.SampleBaked( entity.GlobalPosition.DistanceTo( GlobalPosition ) ) );
					if ( entity.GetHealth() > 0.0f ) {
						entity.AddStatusEffect( new StatusBurning( entity ) );
					}
				}
			}
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

			Area2D HeadHitbox = GetNode<Area2D>( "Animations/HeadAnimations/HeadHitbox" );
			HeadHitbox.SetMeta( "IsHeadHitbox", true );
		}

		protected override void Think() {
			if ( Enraged ) {
				return;
			}
		}

		private void CheckSight() {
			Entity sightTarget = null;
			for ( int i = 0; i < SightLines.Length; i++ ) {
				sightTarget = SightLines[i].GetCollider() as Entity;
				if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
					break;
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
				};
				CanSeeTarget = false;
				return;
			}

			Target = sightTarget;
			LastTargetPosition = Target.GlobalPosition;
			CanSeeTarget = true;
		}

		private void SwingHammer() {
			AngleTween = CreateTween();
			AngleTween.TweenProperty( ArmAnimations, "global_rotation", 80.0f, 2.5f );
		}
	};
};