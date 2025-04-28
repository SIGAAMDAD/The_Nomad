using Godot;

namespace Renown.Thinkers {
	public partial class ZurgutGrunt : Thinker {
		private static readonly float BlowupDamage = 80.0f;

		private Curve BlowupDamageCurve;

		private bool Enraged = false;
		private Entity Target = null;

		private Area2D BlowupArea = null;
		private Timer BlowupTimer = null;

		private void OnDie( Entity source, Entity target ) {
			Enraged = false;
		}
		public override void Damage( Entity source, float nAmount ) {
			base.Damage( source, nAmount );

			Target = source;

			if ( !Enraged && Health < Health * 0.25f ) {
				BlowupArea.SetDeferred( "monitoring", true );
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
					entity.Damage( this, BlowupDamage * BlowupDamageCurve.SampleBaked( entity.GlobalPosition.DistanceTo( GlobalPosition ) ) );
					if ( entity.GetHealth() > 0.0f ) {
						
					}
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			Die += OnDie;

			BlowupArea = GetNode<Area2D>( "BlowupArea" );
			BlowupArea.Monitoring = false;

			BlowupTimer = GetNode<Timer>( "BlowupTimer" );
			BlowupTimer.Connect( "timeout", Callable.From( OnBlowupTimerTimeout ) );

			BlowupDamageCurve = ResourceLoader.Load<Curve>( "res://resources/zurgut_grunt_blowup_damage_curve.tres" );
		}

		protected override void Think() {
			if ( Enraged ) {

			}
		}
	};
};