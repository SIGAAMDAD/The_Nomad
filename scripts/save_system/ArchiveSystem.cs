using Godot;
using SaveSystem;

namespace SaveSystem {
	public enum FieldType : uint {
		Int,
		UInt,
		Float,
		Boolean,
		String,
		IntList,
		UIntList,
		FloatList,
		StringList,
		Vector2,

		Count
	};
};

public partial class ArchiveSystem : Node {
	private static ArchiveSystem _Instance;
	public static ArchiveSystem Instance => _Instance;

	private string SaveDirectory;
	private uint CurrentMemory = 0;

	private bool Loaded = false;

	private static System.Collections.Generic.Dictionary<string, SaveSectionReader> SectionCache = new System.Collections.Generic.Dictionary<string, SaveSectionReader>();

	public static int SectionCount = 0;
	public static System.IO.BinaryReader SaveReader = null;
	public static System.IO.BinaryWriter SaveWriter = null;

	/*
	* A memory is less of an extra save file.
	* To put it simply, its a temporary save file that the player can create,
	* But once its loaded, it will delete au tomatically
	* To be implemented...
	*/
	public class MemoryData {
		public int Year = 0;
		public int Month = 0;
		public int Day = 0;
		public Texture2D Screenshot;

		public MemoryData( int year, int month, int day ) {
			Year = year;
			Month = month;
			Day = day;
		}
	};

	[Signal]
	public delegate void SaveGameBeginEventHandler();
	[Signal]
	public delegate void SaveGameEndEventHandler();
	[Signal]
	public delegate void LoadGameBeginEventHandler();
	[Signal]
	public delegate void LoadGameEndEventHandler();

	public bool IsLoaded() {
		return Loaded;
	}
	public string GetSaveDirectory() {
		return SaveDirectory;
	}

	private void Save( Image screenshot, Godot.Collections.Array<Node> nodes, uint memoryIndex ) {
		DirAccess.MakeDirRecursiveAbsolute( SaveDirectory );

		string path = ProjectSettings.GlobalizePath( SaveDirectory + "GameData.ngd" );
		System.IO.FileStream stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
		SaveWriter = new System.IO.BinaryWriter( stream );

		SectionCount = 0;

		// header
		SaveWriter.Write( SectionCount );

		/*
		CurrentMemory = memoryIndex;

		string path = SaveDirectory + "/Memories/GameData_" + memoryIndex + ".ngd";
		if ( !FileAccess.FileExists( path ) ) {
			MemoryCount++;
			SaveMetadata();
		}

		screenshot.SavePng( "user://SaveData/SLOT_" + ArchiveSystem.Instance.GetCurrentSlot() + "/Memories/Screenshot_" + LastMemory + ".png" );

		FileAccess file = FileAccess.Open( Dir + "/Memories/GameData_" + memoryIndex + ".ngd", FileAccess.ModeFlags.Write );
		if ( file == null ) {
			GD.PushError( "Error creating memory file!" );
			return;
		}
		*/

		for ( int i = 0; i < nodes.Count; i++ ) {
			nodes[i].Call( "Save" );
		}

		stream.Seek( 0, System.IO.SeekOrigin.Begin );
		SaveWriter.Write( SectionCount );
	}

	public static void SaveGame( Image screenshot, uint memoryIndex ) {
		Instance.EmitSignal( "SaveGameBegin" );

		Godot.Collections.Array<Node> nodes = Instance.GetTree().GetNodesInGroup( "Archive" );
		Instance.Save( screenshot, nodes, memoryIndex );
		
		Instance.EmitSignal( "SaveGameEnd" );
	}
	public static void LoadGame() {
		string path = ProjectSettings.GlobalizePath( Instance.SaveDirectory + "GameData.ngd" );
		System.IO.FileStream stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
		SaveReader = new System.IO.BinaryReader( stream );

		int sectionCount = SaveReader.ReadInt32();
		if ( sectionCount == 0 ) {
			GD.PushError( "Save file with no sections" );
			return;
		}

		for ( int i = 0; i < sectionCount; i++ ) {
			string name = SaveReader.ReadString();
			SectionCache.Add( name, new SaveSectionReader() );
		}
	}
	public static SaveSectionReader GetSection( string name ) {
		SaveSectionReader reader;
		if ( SectionCache.TryGetValue( name, out reader ) ) {
			return reader;
		}
		return null;
	}

	/*
	public static bool LoadGame( uint memoryIndex ) {
		Instance.EmitSignal( "LoadGameBegin" );

		Godot.Collections.Array<Node> nodes = Instance.GetTree().GetNodesInGroup( "Archive" );
		
		for ( int i = 0; i < nodes.Count; i++ ) {
			nodes[i].Call( "Load" );
		}

		Instance.EmitSignal( "LoadGameEnd" );

		return true;
	}
	*/

	public override void _EnterTree() {
		base._EnterTree();
		if ( _Instance != null ) {
			this.QueueFree();
		}
		_Instance = this;
	}
	public override void _Ready() {
		base._Ready();

		Loaded = DirAccess.DirExistsAbsolute( "user://SaveData/" );
		SaveDirectory = "user://SaveData/";
	}
};