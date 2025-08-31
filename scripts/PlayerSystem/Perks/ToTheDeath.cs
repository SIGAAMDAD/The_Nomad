using Godot;
using Renown;
using ResourceCache;

namespace PlayerSystem.Perks {
	public partial class Perk2 : Perk {
		public Perk2( Player user )
			: base( user )
		{
			Icon = TextureCache.GetTexture( "res://textures/icons/perk0.jpg" );
			Name = TranslationServer.Translate( "PERK_TOO_FUCKING_ANGRY_TO_DIE_NAME" );
			Description = TranslationServer.Translate( "PERK_TOO_FUCKING_ANGRY_TO_DIE_DESCRIPTION" );
		}

		public override void Connect() {
			
		}
		public override void Disconnect() {
		}
	};
};