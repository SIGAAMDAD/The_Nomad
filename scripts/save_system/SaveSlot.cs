using Godot;

namespace SaveSystem {
	public class Slot {
		private const uint MAGIC = 0x387AFFA3;
		private string Version;

		private int Index = 0;
		private bool Loaded = false;

		private string Dir;
		private string Path;

		public Slot( int nIndex ) {
			Index = nIndex;
			Loaded = LoadMetadata();
		}

		public bool IsValid() {
			return Loaded;
		}

		private bool LoadMetadata() {
			Dir = "user://SaveData/SLOT_" + Index.ToString();
			Path = Dir + "/GameData.ngd";
			if ( !System.IO.Directory.Exists( Dir ) ) {
				return false;
			}

			FileAccess file = FileAccess.Open( Path, FileAccess.ModeFlags.Read );

			uint Magic = file.Get32();
			if ( Magic != MAGIC ) {
				GD.PushError( "Slot::Load: invalid magic in save slot " + Index.ToString() );
				return false;
			}

			return true;
		}

		public bool Load( Godot.Collections.Array<Node> nodes ) {
			if ( !Loaded ) {
				return false;
			}

			FileAccess file = FileAccess.Open( Path, FileAccess.ModeFlags.Read );
			
			// skip past the header
			file.Seek( sizeof( uint ) );

			for ( int i = 0; i < nodes.Count; i++ ) {
				nodes[i].Call( "Load", file );
			}

			return true;
		}
		public void Save( Godot.Collections.Array<Node> nodes ) {
			DirAccess.MakeDirRecursiveAbsolute( Dir );
			FileAccess file = FileAccess.Open( Path, FileAccess.ModeFlags.Write );

			file.Store32( MAGIC );

			for ( int i = 0; i < nodes.Count; i++ ) {
				nodes[i].Call( "Save", file );
			}
		}
	};
};