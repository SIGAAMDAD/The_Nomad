using System.Collections.Generic;
using Godot;

namespace PlayerSystem {
	public class Skin {
		private SpriteFrames BodyAnimations;
		private SpriteFrames ArmAnimations;
		private SpriteFrames LegAnimations;
		private StringName Description;
		private StringName Name;

		private static Dictionary<StringName, Skin> SkinCache = null;
		
		public static void LoadSkins() {
			List<string> skinPaths = new List<string>();

			SkinCache = new Dictionary<StringName, Skin>();
		}

		public Skin( string path ) {
			BodyAnimations = ResourceLoader.Load<SpriteFrames>( path + "/" );
		}
	};
};