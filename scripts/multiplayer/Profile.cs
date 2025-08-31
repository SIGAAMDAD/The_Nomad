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
using System.Collections.Generic;
using Steam;

namespace Multiplayer {
	/*
	===================================================================================
	
	Profile
	
	===================================================================================
	*/
	
	public class Profile {
		public string Name { get; private set; }
		public int SkillPoints { get; private set; } = 0;
		public int Gold { get; private set; } = 0;
		public HashSet<Badge> Badges { get; private set; }

		private Node DashTrail;
		private Resource PlayerSkin;
		private Resource BeardSkin;

		public Loadout Loadout { get; private set; }

		public static Profile Instance;

		public static void Load() {
			string path = ProjectSettings.GlobalizePath( "res://multiplayer_profile.dat" );
			System.IO.FileStream stream;
			System.IO.BinaryReader reader;

			Instance = new Profile();

			try {
				stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
				reader = new System.IO.BinaryReader( stream );
			} catch ( System.Exception ) {
				// not there, so don't mind
				return;
			}

			Instance.SkillPoints = reader.ReadInt32();

			int badgeCount = reader.ReadInt32();
			Instance.Badges.EnsureCapacity( badgeCount );
			for ( int i = 0; i < badgeCount; i++ ) {
				Instance.Badges.Add( (Badge)reader.ReadUInt32() );
			}
		}

		private Profile() {
			Name = SteamManager.GetSteamName();
			SkillPoints = 0;
			Badges = new HashSet<Badge>();
			Loadout = new Loadout();
		}
	};
};