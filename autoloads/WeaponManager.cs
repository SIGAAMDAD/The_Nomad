using Godot;
using System.Collections.Generic;

public partial class WeaponManager : Node {
	private Dictionary<string, Resource> WeaponCache;

	private void LoadWeaponList( string path, List<string> list ) {
		DirAccess dir = DirAccess.Open( path );
		if ( dir != null ) {
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while ( fileName != "" ) {
				if ( dir.CurrentIsDir() ) {
					GD.Print( "Found directory: " + fileName );
					LoadWeaponList( fileName, list );
				} else {
					list.Add( dir.GetCurrentDir() + "/" + fileName );
				}
			}
		} else {
			GD.PushError( "An occurred when attempting to access directory \"" + path + "\"" );
		}
	}

	public void Init() {
		List<string> weaponList = new List<string>();
		LoadWeaponList( "res://resources/weapons", weaponList );

		GD.Print( "Loading weapons..." );
		for ( int i = 0; i < weaponList.Count; i++ ) {
			WeaponCache.Add( weaponList[i].GetBaseName(), ResourceLoader.Load( weaponList[i], "", ResourceLoader.CacheMode.Replace ) );
			if ( WeaponCache[ weaponList[i].GetBaseName() ] != null ) {
				GD.Print( "...loaded weapon data for \"" + weaponList[i] + "\"" );
			} else {
				GD.PushError( "..couldn't load weapon data for \"" + weaponList[i] + "\"" );
			}
		}
	}
}
