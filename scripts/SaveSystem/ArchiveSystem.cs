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
using SaveSystem;
using Steamworks;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using Menus;
using System;

namespace SaveSystem {
	public enum FieldType : uint {
		Int8,
		Int16,
		Int32,
		Int64,
		UInt8,
		UInt16,
		UInt32,
		UInt64,
		Float,
		Double,
		Boolean,
		String,
		Vector2,
		Vector2I,

		IntList,
		UIntList,
		FloatList,
		StringList,
		ByteArray,

		// internal godot types
		Array,
		Dictionary,

		Count
	};
};

/*
===================================================================================

ArchiveSystem

Made this way to avoid save file corruption between mods & versions.

Fuck U M&B save files!

===================================================================================
*/
/// <summary>
/// the main singleton for managing game save data
/// </summary>

public partial class ArchiveSystem : Node {
	private static readonly ulong MAGIC = 0xffead4546B727449;
	public static readonly string SaveDirectory = "user://SaveData/";

	private bool Loaded = false;

	private static ConcurrentDictionary<string, SaveSectionReader> SectionCache = new ConcurrentDictionary<string, SaveSectionReader>();

	public static int SectionCount { get; private set; } = 0;
	public static System.IO.BinaryReader? SaveReader { get; private set; } = null;
	public static System.IO.BinaryWriter? SaveWriter { get; private set; } = null;

	[Signal]
	public delegate void SaveGameBeginEventHandler();
	[Signal]
	public delegate void SaveGameEndEventHandler();
	[Signal]
	public delegate void LoadGameBeginEventHandler();
	[Signal]
	public delegate void LoadGameEndEventHandler();

	public static ArchiveSystem? Instance;

	/*
	===============
	IsLoaded
	===============
	*/
	public static bool IsLoaded() {
		return SlotExists( SettingsData.LastSaveSlot )
			&& GameConfiguration.GameMode != GameMode.Multiplayer
			&& GameConfiguration.GameMode != GameMode.ChallengeMode;
	}

	private static long GetFreeDiskSpace( string path ) {
		try {
			System.IO.DriveInfo driveInfo = new System.IO.DriveInfo( path );
			if ( driveInfo.IsReady ) {
				return driveInfo.AvailableFreeSpace;
			}
		} catch ( System.Exception ) {
			Console.PrintError( $"ArchiveSystem.GetFreeDiskSpace: error getting remaining disk space in drive {path}" );
		}
		return -1;
	}

	/*
	===============
	Save
	===============
	*/
	private static void Save( Godot.Collections.Array<Node> nodes, int slot ) {
		Error result = DirAccess.MakeDirRecursiveAbsolute( SaveDirectory );
		if ( result != Error.Ok ) {
			throw new System.IO.IOException( $"ArchiveSystem.Save: couldn't create directory {SaveDirectory} - godot error \"{result}\"" );
		}

		string path = ProjectSettings.GlobalizePath( $"{SaveDirectory}GameData_{slot}.ngd" );

		using var stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
		using ( SaveWriter = new System.IO.BinaryWriter( stream ) ) {
			SaveWriter.Write( MAGIC );
			SaveWriter.Write( ProjectSettings.GetSetting( "application/config/version" ).AsString() );

			SectionCount = 0;
			SaveWriter.Write( SectionCount );

			for ( int i = 0; i < nodes.Count; i++ ) {
				nodes[ i ].Call( "Save" );
			}

			stream.Seek( 0, System.IO.SeekOrigin.Begin );
			SaveWriter.Write( MAGIC );
			SaveWriter.Write( ProjectSettings.GetSetting( "application/config/version" ).AsString() );

			// write the section count after we've saved all the sections
			SaveWriter.Write( SectionCount );

			SaveWriter.Flush();
		}
	}

	/*
	===============
	PushSection
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void PushSection() {
		SectionCount++;
	}

	/*
	===============
	CreateSlot
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CreateSlot() {
		Instance.Loaded = false;
	}

	/*
	===============
	SlotExists
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool SlotExists( int slot ) {
		return Godot.FileAccess.FileExists( $"user://SaveData/GameData_{slot}.ngd" );
	}

	/*
	===============
	DeleteSave
	===============
	*/
	/// <summary>
	/// Deletes the given save slot from both local storage and steam cloud storage
	/// </summary>
	/// <param name="slot"></param>
	/// <exception cref="System.Exception"></exception>
	public static void DeleteSave( int slot ) {
		Error result = DirAccess.RemoveAbsolute( $"user://SaveData/GameData_{slot}.ngd" );
		if ( result != Error.Ok ) {
			throw new System.Exception( $"ArchiveSystem.DeleteSave: DirAccess.RemoveAbsolute returned \"{result}\"" );
		}
		if ( !SteamRemoteStorage.FileDelete( $"SaveData/GameData_{slot}.ngd" ) ) {
			Console.PrintError( $"ArchiveSystem.DeleteSave: SteamRemoteStorage.FileDelete returned false" );
		}
	}

	/*
	===============
	SaveGame
	===============
	*/
	/// <summary>
	/// Archives current game state, called when exiting game, activating a firelink, or resting at a meliora
	/// </summary>
	/// <param name="slot">The slot to save into</param>
	public static void SaveGame( int slot ) {
		if ( slot < 0 ) {
			Console.PrintError( $"ArchiveSystem.SaveGame: slot index cannot be less than 0" );
			return;
		}

		Instance.EmitSignalSaveGameBegin();
		string slotPath = $"SaveData/GameData_{slot}.ngd";

		try {
			Godot.Collections.Array<Node> nodes = Instance.GetTree().GetNodesInGroup( "Archive" );
			Save( nodes, slot );

			Steam.SteamManager.SaveCloudFile( slotPath );
		} catch ( System.Exception e ) {
			Console.PrintError( $"ArchiveSystem.SaveGame: exception thrown while saving game, deleting corrupt files\n{e}" );

			// remove possibly corrupt save data from cloud and local storage
			DeleteSave( slot );
		}

		Instance.EmitSignalSaveGameEnd();
	}

	/*
	===============
	LoadGame
	===============
	*/
	/// <summary>
	/// Loads and initializes game state from the provided save slot file
	/// </summary>
	/// <param name="slot">The save slot to load</param>
	public static void LoadGame( int slot ) {
		Instance.EmitSignalLoadGameBegin();

		string path = ProjectSettings.GlobalizePath( $"{SaveDirectory}GameData_{slot}.ngd" );

		try {
			using var stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
			using ( SaveReader = new System.IO.BinaryReader( stream ) ) {
				ulong magic = SaveReader.ReadUInt64();
				if ( magic != MAGIC ) {
					Console.PrintError( "ArchiveSystem.LoadGame: Save data has invalid magic in header!" );
					Instance.Loaded = false;
					return;
				}

				string version = SaveReader.ReadString();
				if ( version != ProjectSettings.GetSetting( "application/config/version" ).AsString() ) {
					Console.PrintWarning( $"ArchiveSystem.LoadGame: savefile has different version \"{version}\" from the current executable" );
				}

				Instance.Loaded = true;

				int sectionCount = SaveReader.ReadInt32();

				for ( int i = 0; i < sectionCount; i++ ) {
					string name = SaveReader.ReadString();
					Console.PrintLine( $"Loading save section \"{name}\"..." );
					SectionCache.TryAdd( name, new SaveSectionReader( SaveReader ) );
					Console.PrintLine( "...Done" );
				}
			}
		} catch ( System.Exception e ) {
			Console.PrintError( $"ArchiveSystem.LoadGame: exception thrown while loading save game\n{e}" );

			Instance.Loaded = false;
			SectionCache.Clear();
		}
		Instance.EmitSignalLoadGameEnd();
	}

	/*
	===============
	Clear
	===============
	*/
	/// <summary>
	/// Clears the section cache when loading a new save
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void Clear() {
		SectionCache.Clear();
	}

	/*
	===============
	GetSection
	===============
	*/
	/// <summary>
	/// Performs a lookup for a section based on the name
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static SaveSectionReader? GetSection( string name ) {
		ArgumentException.ThrowIfNullOrEmpty( name );

		if ( SectionCache.TryGetValue( name, out SaveSectionReader? value ) ) {
			return value;
		}
		Console.PrintError( $"ArchiveSystem.GetSection: Failed to find save section \"{name}\"!" );
		return null;
	}

	public override void _EnterTree() {
		base._EnterTree();
		Instance = this;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CheckSaveData() {
		Instance.Loaded = Godot.FileAccess.FileExists( $"user://SaveData/GameData_{SettingsData.LastSaveSlot}.ngd" );
	}
};