using System;
using System.IO;
using System.Reflection;
using Godot;

public partial class ModMetadata : Resource {
	[Export]
	public string? Version { get; private set; }
	[Export]
	public StringName? Name { get; private set; }
	[Export]
	public string? DllPath { get; private set; }
	[Export]
	public string? PckPath { get; private set; }

	public Assembly Assembly;

	public void Load() {
		if ( Name == null || Name.IsEmpty ) {
			throw new ModLoadingException( "ModMetadata.Load: Mod name is null or empty!" );
		}
		if ( DllPath != null && DllPath.Length > 0 ) {
			try {
				Assembly = Assembly.LoadFile( DllPath );
			} catch ( FileNotFoundException fileNotFound ) {
				throw new ModLoadingException( $"ModMetadata.Load: Dynamic Link Library {DllPath} couldn't be found" );
			} catch ( BadImageFormatException badImage ) {
				throw new ModLoadingException( $"ModMetadata.Load: Dynamic Link Library {DllPath} has an incompatible or corrupt image" );
			}
		}

		bool bSuccess = ProjectSettings.LoadResourcePack( PckPath, true );
		if ( !bSuccess ) {
			Console.PrintError( string.Format( "ModMetadata.Load: couldn't load mod resource pack \"{0}\"!", PckPath ) );
		}
	}
};