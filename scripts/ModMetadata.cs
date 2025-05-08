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
		} catch ( Exception ) {
		}

		bool bSuccess = ProjectSettings.LoadResourcePack( PckPath );
		if ( !bSuccess ) {
		}
	}
};