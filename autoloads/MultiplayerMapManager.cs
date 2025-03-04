using System.Linq;
using Godot;

public static class MultiplayerMapManager {
	public class MapData {
		public string Name;
		public string FileName;
		public System.Collections.Generic.List<string> Modes;

		public MapData( Resource map ) {
			Name = (string)map.Get( "_name" );
			FileName = (string)map.Get( "_filename" );
		}
	};

	public static System.Collections.Generic.List<MapData> MapCache;

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

		GD.Print( "Loading maps..." );

		MapCache = new System.Collections.Generic.List<MapData>();
		for ( int i = 0; i < mapList.Count; i++ ) {
			MapCache.Add( new MapData( ResourceLoader.Load( mapList[i], "", ResourceLoader.CacheMode.Replace ) ) );
			if ( MapCache.Last() != null ) {
				GD.Print( "...Loaded multiplayer map data for \"" + mapList[i] + "\"" );
			} else {
				GD.PushError( "...Couldn't load map data for \"" + mapList[i] + "\"" );
			}
		}
	}
};