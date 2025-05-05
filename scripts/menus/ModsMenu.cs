using System.Collections.Generic;
using Godot;

public partial class ModsMenu : Control {
	private Dictionary<StringName, ModMetadata> ModList;

	private List<string> GetModList( string directory ) {
		List<string> modList = new List<string>();

		DirAccess dir = DirAccess.Open( directory );
		if ( dir != null ) {
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while ( fileName.Length > 0 ) {
				if ( fileName.GetExtension() != ".mod" ) {
					fileName = dir.GetNext();
					continue;
				}
				modList.Add( dir.GetCurrentDir() + "/" + fileName );
				fileName = dir.GetNext();
			}
		} else {
			Console.PrintError( string.Format( "An error occurred when trying to access path \"{0}\"", directory ) );
		}

		return modList;
	}

	public override void _Ready() {
		base._Ready();

		List<string> modList = GetModList( "user://mods" );
		modList.Sort();

		ModList = new Dictionary<StringName, ModMetadata>( modList.Count );
		for ( int i = 0; i < modList.Count; i++ ) {
			ModMetadata mod = ResourceLoader.Load<ModMetadata>( modList[i] );
			ModList.Add( mod.Name, mod );
			Console.PrintLine( string.Format( "...loaded mod {0}", mod.Name ) );
		}
	}
};