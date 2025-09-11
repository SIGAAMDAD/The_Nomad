using Godot;
using ResourceCache;
using System;

namespace PlayerSystem.Perks {
	public partial class ToTheDeath : Perk {
		public ToTheDeath( Player user )
			: base( user )
		{
			Icon = TextureCache.GetTexture( "res://textures/icons/perk0.jpg" );
			ArgumentNullException.ThrowIfNull( Icon );

			Name = TranslationServer.Translate( Name );
			Description = TranslationServer.Translate( Description );
		}

		public override void Connect() {
			
		}
		public override void Disconnect() {
		}
	};
};