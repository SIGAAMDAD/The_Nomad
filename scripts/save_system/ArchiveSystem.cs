using System;
using System.ComponentModel.Design.Serialization;
using System.Text.RegularExpressions;
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

	private const ulong MAGIC = 0xffead4546B727449;

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
		return Loaded && GameConfiguration.GameMode != GameMode.Multiplayer;
	}
	public string GetSaveDirectory() {
		return SaveDirectory;
	}

	private void Save( Image screenshot, Godot.Collections.Array<Node> nodes, uint memoryIndex ) {
		DirAccess.MakeDirRecursiveAbsolute( SaveDirectory );

		string path = ProjectSettings.GlobalizePath( SaveDirectory + "GameData.ngd" );
		System.IO.FileStream stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
		SaveWriter = new System.IO.BinaryWriter( stream );

		SaveWriter.Write( MAGIC );
		SaveWriter.Write( (string)ProjectSettings.GetSetting( "application/config/version" ) );

		SectionCount = 0;
		SaveWriter.Write( SectionCount );

		for ( int i = 0; i < nodes.Count; i++ ) {
			nodes[i].Call( "Save" );
		}

		stream.Seek( 0, System.IO.SeekOrigin.Begin );
		SaveWriter.Write( MAGIC );
		SaveWriter.Write( (string)ProjectSettings.GetSetting( "application/config/version" ) );
		SaveWriter.Write( SectionCount );
		SaveWriter.Flush();
		SaveWriter.Close();
	}

	public static void SaveGame( Image screenshot, uint memoryIndex ) {
		Instance.EmitSignal( "SaveGameBegin" );

		Godot.Collections.Array<Node> nodes = Instance.GetTree().GetNodesInGroup( "Archive" );
		Instance.Save( screenshot, nodes, memoryIndex );

		SteamManager.SaveCloudFile( "SaveData/GameData.ngd" );
		
		Instance.EmitSignal( "SaveGameEnd" );
	}
	public static void LoadGame() {
		string path = ProjectSettings.GlobalizePath( Instance.SaveDirectory + "GameData.ngd" );
		System.IO.FileStream stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
		SaveReader = new System.IO.BinaryReader( stream );

		ulong magic = SaveReader.ReadUInt64();
		if ( magic != MAGIC ) {
			Console.PrintError( "Save data has invalid magic in header!" );
			Instance.Loaded = false;
			return;
		}

		string version = SaveReader.ReadString();
		// TODO: version compatibility conversion

		Instance.Loaded = true;

		int sectionCount = SaveReader.ReadInt32();

		for ( int i = 0; i < sectionCount; i++ ) {
			string name = SaveReader.ReadString();
			GD.Print( "Loading save section \"" + name + "\"..." );
			SectionCache.Add( name, new SaveSectionReader() );
			GD.Print( "...Done" );
		}

		SaveReader.Close();
	}
	public static void Clear() {
		SectionCache.Clear();
	}

	public static byte LoadByte() => SaveReader.ReadByte();
	public static ushort LoadUShort() => SaveReader.ReadUInt16();
	public static uint LoadUInt() => SaveReader.ReadUInt32();
	public static ulong LoadULong() => SaveReader.ReadUInt64();
	public static sbyte LoadSByte() => SaveReader.ReadSByte();
	public static short LoadShort() => SaveReader.ReadInt16();
	public static int LoadInt() => SaveReader.ReadInt32();
	public static long LoadLong() => SaveReader.ReadInt64();
	public static float LoadFloat() => (float)SaveReader.ReadDouble();
	public static string LoadString() => SaveReader.ReadString();
	public static Godot.Vector2 LoadVector2() {
		Godot.Vector2 value = Godot.Vector2.Zero;
		value.X = LoadFloat();
		value.Y = LoadFloat();
		return value;
	}
	public static bool LoadBoolean() => SaveReader.ReadBoolean();

	public static void SaveByte( byte value ) => SaveWriter.Write( value );
	public static void SaveUShort( ushort value ) => SaveWriter.Write( value );
	public static void SaveUInt( uint value ) => SaveWriter.Write( value );
	public static void SaveULong( ulong value ) => SaveWriter.Write( value );
	public static void SaveSByte( sbyte value ) => SaveWriter.Write( value );
	public static void SaveShort( short value ) => SaveWriter.Write( value );
	public static void SaveInt( int value ) => SaveWriter.Write( value );
	public static void SaveLong( long value ) => SaveWriter.Write( value );
	public static void SaveFloat( float value ) => SaveWriter.Write( (double)value );
	public static void SaveString( string value ) => SaveWriter.Write( value );
	public static void SaveVector2( Godot.Vector2 value ) {
		SaveFloat( value.X );
		SaveFloat( value.Y );
	}
	public static void SaveBoolean( bool value ) => SaveWriter.Write( value );

	public static SaveSectionReader GetSection( string name ) {
		if ( SectionCache.ContainsKey( name ) ) {
			return SectionCache[ name ];
		}
		GD.PushError( "Failed to find save section \"" + name + "\"!" );
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