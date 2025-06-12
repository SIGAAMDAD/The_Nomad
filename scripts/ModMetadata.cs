using System;
using System.Reflection;
using Godot;

public partial class ModMetadata : Resource {
	[Export]
	public string Version {
		get;
		private set;
	}
	[Export]
	public StringName Name {
		get;
		private set;
	}
	[Export]
	public string DllPath {
		get;
		private set;
	}
	[Export]
	public string PckPath {
		get;
		private set;
	}

	public Assembly Assembly;

	public void Load() {
		try {
			Assembly = Assembly.LoadFile( DllPath );
		} catch ( Exception e ) {
			Console.PrintError( string.Format( "ModMetadata.Load: couldn't load mod assembly .dll \"{0}\"!", DllPath ) );
			Console.PrintError( string.Format( "StackDump: {0}", e.StackTrace ) );
		}

		bool bSuccess = ProjectSettings.LoadResourcePack( PckPath );
		if ( !bSuccess ) {
			Console.PrintError( string.Format( "ModMetadata.Load: couldn't load mod resource pack \"{0}\"!", PckPath ) );
		}
	}
};