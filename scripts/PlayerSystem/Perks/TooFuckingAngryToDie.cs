using Godot;
using Renown;

namespace PlayerSystem.Perks {
	public partial class TooFuckingAngryToDie : Perk {
		private int Counter = 0;

		public TooFuckingAngryToDie( Player user )
			: base( user )
		{
			Name = TranslationServer.Translate( Name );
			Description = TranslationServer.Translate( Description );
		}

		private void OnUserDie( Entity source, Entity target ) {
			if ( User.Rage < 10.0f ) {
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