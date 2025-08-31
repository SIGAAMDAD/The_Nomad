using Godot;

namespace SaveSystem {
	public class Slot {
		public class MemoryMetadata {
			public uint Year = 0;
			public uint Month = 0;
			public uint Day = 0;

			public MemoryMetadata( uint year, uint month, uint day ) {
				Year = year;
				Month = month;
				Day = day;
			}
		};

		private const uint MAGIC = 0x387AFFA3;
		private string Version;
		private uint MemoryCount = 0;
		private uint LastMemory = 0;

		private int Index = 0;
		private bool Loaded = false;

		private string Dir;
		private string Path;

		private System.Collections.Generic.List<MemoryMetadata> MemoryList = new System.Collections.Generic.List<MemoryMetadata>();

		public Slot( int nIndex ) {
			Index = nIndex;
			Loaded = LoadMetadata();
		}

		public string GetPath() {
			return Dir;
		}
		public bool IsValid() {
			return Loaded;
		}
		public uint GetCurrentMemory() {
			return LastMemory;
		}
		public System.Collections.Generic.List<MemoryMetadata> GetMemoryList() {
			return MemoryList;
		}

		private bool LoadMetadata() {
			Dir = "user://SaveData/SLOT_" + Index.ToString();
			Path = Dir + "/MetaData.dat";
			if ( !DirAccess.DirExistsAbsolute( Dir ) ) {
				return false;
			}

			FileAccess file = FileAccess.Open( Path, FileAccess.ModeFlags.Read );

			uint Magic = file.Get32();
			if ( Magic != MAGIC ) {
				GD.PushError( "Slot::Load: invalid magic in save slot " + Index.ToString() );
				return false;
			}

			MemoryCount = file.Get32();
			LastMemory = file.Get32();

			for ( uint i = 0; i < MemoryCount; i++ ) {
				uint year = file.Get32();
				uint month = file.Get32();
				uint day = file.Get32();

				MemoryList.Add( new MemoryMetadata( year, month, day ) );
			}

			return true;
		}

		private void SaveMetadata() {
			FileAccess file = FileAccess.Open( Path, FileAccess.ModeFlags.Write );
			if ( file == null ) {
				GD.PushError( "Failed to open file metadata for save slot!" );
				return;
			}

			file.Store32( MAGIC );
			file.Store32( MemoryCount );
			file.Store32( LastMemory );
			
			for ( int i = 0; i < MemoryList.Count; i++ ) {
				uint year, month, day;
				if ( i == LastMemory ) {
					year = (uint)System.DateTime.Now.Year;
					month = (uint)System.DateTime.Now.Month;
					day = (uint)System.DateTime.Now.Day;
				} else {
					year = MemoryList[i].Year;
					month = MemoryList[i].Month;
					day = MemoryList[i].Day;
				}
				file.Store32( year );
				file.Store32( month );
				file.Store32( day );
			}
		}

		public bool Load( Godot.Collections.Array<Node> nodes, uint memoryIndex ) {
			if ( !Loaded ) {
				return false;
			}

			FileAccess file = FileAccess.Open( Dir + "/Memories/GameData_" + memoryIndex + ".ngd", FileAccess.ModeFlags.Read );
			LastMemory = memoryIndex;

			for ( int i = 0; i < nodes.Count; i++ ) {
				nodes[i].Call( "Load", file );
			}

			return true;
		}
		public void Save( Image screenshot, Godot.Collections.Array<Node> nodes, uint memoryIndex ) {
			DirAccess.MakeDirRecursiveAbsolute( Dir );
			DirAccess.MakeDirRecursiveAbsolute( Dir + "/Memories" );

			LastMemory = memoryIndex;

			string path = Dir + "/Memories/GameData_" + memoryIndex + ".ngd";
			if ( !FileAccess.FileExists( path ) ) {
				MemoryCount++;
				SaveMetadata();
			}

//			screenshot.SavePng( "user://SaveData/SLOT_" + ArchiveSystem.Instance.GetCurrentSlot() + "/Memories/Screenshot_" + LastMemory + ".png" );

			FileAccess file = FileAccess.Open( Dir + "/Memories/GameData_" + memoryIndex + ".ngd", FileAccess.ModeFlags.Write );
			if ( file == null ) {
				GD.PushError( "Error creating memory file!" );
				return;
			}

			for ( int i = 0; i < nodes.Count; i++ ) {
				nodes[i].Call( "Save", file );
			}
		}
	};
};