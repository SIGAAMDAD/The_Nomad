using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Nomad.Game.Domain.Data.Mods;

namespace Nomad.Game.Infrastructure.Mods
{
	public static class ModuleDiscovery
	{
		public static List<DiscoveredModule> Discover( string modulesRoot )
		{
			var results = new List<DiscoveredModule>();

			if ( !Directory.Exists( modulesRoot ) ) {
				return results;
			}

			foreach ( var dir in Directory.EnumerateDirectories( modulesRoot ) ) {
				var manifestPath = Path.Combine( dir, "module.json" );
				if ( !File.Exists( manifestPath ) ) {
					continue;
				}

				var json = File.ReadAllText( manifestPath );
				var manifest = JsonSerializer.Deserialize<ModuleManifest>( json )
					?? throw new InvalidOperationException( $"Failed to parse {manifestPath}" );

				ValidateManifest( manifest, manifestPath );
				results.Add( new DiscoveredModule { Directory = dir, Manifest = manifest } );
			}

			return results;
		}

		private static void ValidateManifest( ModuleManifest manifest, string manifestPath )
		{
			if ( string.IsNullOrWhiteSpace( manifest.Id ) ) {
				throw new InvalidOperationException( $"{manifestPath}: id is required" );
			}
			if ( string.IsNullOrWhiteSpace( manifest.EntryAssembly ) ) {
				throw new InvalidOperationException( $"{manifestPath}: entryAssembly is required" );
			}
			if ( string.IsNullOrWhiteSpace( manifest.EntryType ) ) {
				throw new InvalidOperationException( $"{manifestPath}: entryType is required" );
			}
		}
	};
};
