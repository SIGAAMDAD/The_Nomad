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
using System.Collections.Concurrent;
using System.Collections.Generic;

/*
===================================================================================

MultiplayerMapManager

===================================================================================
*/
/// <summary>
/// loads and manages all the metadata associated with map data
/// found in res://resources/multiplayer_maps
/// </summary>

public static class MultiplayerMapManager {
	public readonly struct MapData {
		public readonly string Name;
		public readonly string FileName;
		public readonly bool ModeBloodbath;
		public readonly bool ModeCaptureTheFlag;
		public readonly bool ModeKingOfTheHill;
		public readonly bool ModeDuel;
		public readonly bool ModeBountyHunt;
		public readonly bool ModeExtraction;
		public readonly bool ModeHoldTheLine;

		/*
		===============
		MapData
		===============
		*/
		public MapData( Resource map ) {
			Name = map.Get( "_name" ).AsString();
			FileName = map.Get( "_filename" ).AsString();

			ModeBloodbath = map.Get( "_mode_bloodbath" ).AsBool();
			ModeCaptureTheFlag = map.Get( "_mode_capture_the_flag" ).AsBool();
			ModeKingOfTheHill = map.Get( "_mode_king_of_the_hill" ).AsBool();
			ModeDuel = map.Get( "_mode_duel" ).AsBool();
			ModeBountyHunt = map.Get( "_mode_bounty_hunt" ).AsBool();
			ModeExtraction = map.Get( "_mode_extraction" ).AsBool();
			ModeHoldTheLine = map.Get( "_mode_hold_the_line" ).AsBool();
		}
	};

	public static ConcurrentDictionary<string, MapData> MapCache { get; private set; }

	/*
	===============
	IsMapValid
	===============
	*/
	public static bool IsMapValid( string name ) {
		return MapCache.ContainsKey( name );
	}

	/*
	===============
	LoadMapList
	===============
	*/
	private static void LoadMapList( string path, List<string> list ) {
		DirAccess dir = DirAccess.Open( path );
		if ( dir != null ) {
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while ( fileName.Length > 0 ) {
				list.Add( $"{dir.GetCurrentDir()}/{fileName}" );
				fileName = dir.GetNext();
			}
		} else {
			Console.PrintError( $"MultiplayerMapManager.LoadMapList: an error occurred when trying to access path \"{path}\"" );
		}
	}

	/*
	===============
	Init
	===============
	*/
	public static void Init() {
		List<string> mapList = new List<string>();
		LoadMapList( "res://resources/multiplayer_maps", mapList );

		Console.PrintLine( "MultiplayerMapManager.Init: Loading maps..." );

		MapCache = new ConcurrentDictionary<string, MapData>();
		for ( int i = 0; i < mapList.Count; i++ ) {
			Console.PrintLine( $"MultiplayerMapManager.Init: ...found map {mapList[ i ]}" );
			Resource map = ResourceLoader.Load( mapList[ i ], "", ResourceLoader.CacheMode.Replace );
			if ( map != null ) {
				MapCache.TryAdd( map.Get( "_name" ).AsString(), new MapData( map ) );
			} else {
				Console.PrintError( $"MultiplayerMapManager.Init: error loading multiplayer map {mapList[ i ]}!" );
			}
		}
	}
};