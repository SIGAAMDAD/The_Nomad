using Godot;

public static class MultiplayerMapManager {
	public class MapData {
		public readonly string Name;
		public readonly string FileName;
		public readonly bool ModeBloodbath;
		public readonly bool ModeTeamBrawl;
		public readonly bool ModeCaptureTheFlag;
		public readonly bool ModeKingOfTheHill;
		public readonly bool ModeDuel;

		public MapData( Resource map ) {
			Name = (string)map.Get( "_name" );
			FileName = (string)map.Get( "_filename" );

			ModeBloodbath = (bool)map.Get( "_mode_bloodbath" );
			ModeTeamBrawl = (bool)map.Get( "_mode_team_brawl" );
			ModeCaptureTheFlag = (bool)map.Get( "_mode_capture_the_flag" );
			ModeKingOfTheHill = (bool)map.Get( "_mode_king_of_the_hill" );
			ModeDuel = (bool)map.Get( "_mode_duel" );
		}
	};

	public static System.Collections.Generic.Dictionary<string, MapData> MapCache;

	private static void LoadMapList( string path, System.Collections.Generic.List<string> list ) {
		DirAccess dir = DirAccess.Open( path );
		if ( dir != null ) {
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while ( fileName.Length > 0 ) {
				list.Add( dir.GetCurrentDir() + "/" + fileName );
				fileName = dir.GetNext();
			}
		} else {
			GD.PushError( "An error occurred when trying to access path \"" + path + "\"" );
		}
	}

	public static void Init() {
		System.Collections.Generic.List<string> mapList = new System.Collections.Generic.List<string>();
		LoadMapList( "res://resources/multiplayer_maps", mapList );

		Console.PrintLine( "Loading maps..." );

		MapCache = new System.Collections.Generic.Dictionary<string, MapData>();
		for ( int i = 0; i < mapList.Count; i++ ) {
			Resource map = ResourceLoader.Load( mapList[i], "", ResourceLoader.CacheMode.Replace );
			if ( map != null ) {
				MapCache.Add( (string)map.Get( "_name" ), new MapData( map ) );
			} else {
				Console.PrintError( string.Format( "Error loading multiplayer map {0}!", mapList[i] ) );
			}
		}
	}
};