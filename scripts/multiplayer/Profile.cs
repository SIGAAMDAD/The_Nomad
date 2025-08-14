using Godot;
using System.Collections.Generic;

namespace Multiplayer {
	public class Profile {
		private string Name;
		private int SkillPoints = 0;
		private int Gold = 0;
		private HashSet<Badge> Badges;

		private Node DashTrail;
		private Resource PlayerSkin;
		private Resource BeardSkin;

		public static Profile Instance;

		public static void Load() {
			string path = ProjectSettings.GlobalizePath( "res://multiplayer_profile.dat" );
			System.IO.FileStream stream;
			System.IO.BinaryReader reader;

			try {
				stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
				reader = new System.IO.BinaryReader( stream );
			} catch ( System.Exception ) {
				// not there, so don't mind
				return;
			}

			Instance = new Profile();

			Instance.Name = SteamManager.GetSteamName();
			Instance.SkillPoints = reader.ReadInt32();

			int badgeCount = reader.ReadInt32();
			Instance.Badges = new HashSet<Badge>( badgeCount );
			for ( int i = 0; i < badgeCount; i++ ) {
				Instance.Badges.Add( (Badge)reader.ReadUInt32() );
			}
		}
	};
};