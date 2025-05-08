using System.Collections.Generic;
using Godot;

public partial class ModsMenu : Control {
	private Dictionary<StringName, ModMetadata> ModList;
	private AudioStreamPlayer ThemeChannel;

	private HBoxContainer Cloner;

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

		Cloner = GetNode<HBoxContainer>( "MarginContainer/VScrollBar/Cloner" );

		ThemeChannel = GetNode<AudioStreamPlayer>( "Theme" );

		Console.PrintLine( "Loading mods..." );

		List<string> modList = GetModList( "user://Mods" );
		modList.Sort();

		ModList = new Dictionary<StringName, ModMetadata>( modList.Count );
		for ( int i = 0; i < modList.Count; i++ ) {
			ModMetadata mod = ResourceLoader.Load<ModMetadata>( modList[i] );
			mod.Load();
			ModList.Add( mod.Name, mod );

			HBoxContainer container = Cloner.Duplicate() as HBoxContainer;
			( container.GetChild( 0 ) as RichTextLabel ).ParseBbcode( mod.Name );
			( container.GetChild( 1 ) as Label ).Text = mod.Version;
			container.Show();

			Console.PrintLine( string.Format( "...loaded mod {0}", mod.Name ) );
		}
	}
};