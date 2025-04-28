using Godot;
using Renown;

namespace PlayerSystem.Perks {
	public class TooFuckingAngryToDie : Perk {
		private int Counter = 0;

		public TooFuckingAngryToDie( Player user )
			: base( user )
		{
			Icon = ResourceCache.GetTexture( "res://textures/icons/perk0.jpg" );
			Name = TranslationServer.Translate( "PERK_TOO_FUCKING_ANGRY_TO_DIE_NAME" );
			Description = TranslationServer.Translate( "PERK_TOO_FUCKING_ANGRY_TO_DIE_DESCRIPTION" );
		}

		private void OnUserDie( Entity source, Entity target ) {
			if ( User.GetRage() < 25.0f ) {
				return;
			}

			//TODO: play big 'ol BOOM!
			User.SetRage( 0.0f );
			User.SetHealth( 100.0f );
		}

		public override void Connect() {
			User.Die += OnUserDie;
		}
		public override void Disconnect() {
			User.Die -= OnUserDie;
		}
	};
};