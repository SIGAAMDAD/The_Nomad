/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Nomad.Core.FileSystem;
using Nomad.Game.Domain.Data.Mods;
using Nomad.Game.Domain.Interfaces.Mods;

namespace Nomad.Game.Infrastructure.Mods
{
	internal sealed class ModuleScanner
	{
		private readonly string _apiVersion;
		private readonly List<string> _scanRoots = new();

		private readonly IFileSystem _fileSystem;

		private readonly ModAssemblyValidator _validator;
		private readonly ModSecurityPolicy _policy;

		public ModuleScanner( string apiVersion, IFileSystem fileSystem )
		{
			_apiVersion = apiVersion;
			_fileSystem = fileSystem ?? throw new ArgumentNullException( nameof( fileSystem ) );

			_policy = new ModSecurityPolicy();
			_validator = new ModAssemblyValidator( _policy );
		}

		public void AddScanRoot( string path )
		{
			if ( !string.IsNullOrWhiteSpace( path ) ) {
				_scanRoots.Add( path );
			}
		}

		public IReadOnlyList<DiscoveredModule> ScanAndLoad()
		{
			var manifests = ScanManifests();
			ValidateManifests( manifests );

			var sorted = SortByDependencies( manifests );
			var loaded = new List<DiscoveredModule>();

			foreach ( var manifest in sorted ) {
				var module = LoadModuleAssembly( manifest );
				loaded.Add( module );
			}

			return loaded;
		}

		private List<ModuleManifest> ScanManifests()
		{
			var manifests = new List<ModuleManifest>();

			foreach ( var root in _scanRoots ) {
				if ( !_fileSystem.DirectoryExists( root ) ) {
					continue;
				}

				foreach ( var directory in _fileSystem.GetDirectories( root ) ) {
					string manifestPath = Path.Combine( directory, "module.json" );

					if ( !_fileSystem.FileExists( manifestPath ) ) {
						continue;
					}

					try {
						using var fileBuffer = _fileSystem.LoadFile( manifestPath );

						var manifest = JsonSerializer.Deserialize<ModuleManifest>(
							fileBuffer.AsStream(),
							new JsonSerializerOptions {
								PropertyNameCaseInsensitive = true,
								ReadCommentHandling = JsonCommentHandling.Skip,
								AllowTrailingCommas = true
							}
						);

						if ( manifest == null ) {
							// TODO: warning
						}

						manifest.DirectoryPath = directory;

						manifests.Add( manifest );
					}
					catch ( Exception ex ) {

					}
				}
			}
			return manifests;
		}

		private void ValidateManifests( List<ModuleManifest> manifests )
		{
			var ids = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

			foreach ( var manifest in manifests ) {
				if ( string.IsNullOrWhiteSpace( manifest.Id ) ) {
					throw new InvalidOperationException( $"Module in '{manifest.DirectoryPath}' has no ID." );
				}
				if ( !ids.Add( manifest.Id ) ) {
					throw new InvalidOperationException( $"Duplicate module ID: {manifest.Id}" );
				}
				if ( manifest.ApiVersion != _apiVersion ) {
					throw new InvalidOperationException(
						$"Module '{manifest.Id}' targets mod API {manifest.ApiVersion}, but runtime uses {_apiVersion}."
					);
				}

				string assemblyPath = Path.Combine( manifest.DirectoryPath, manifest.Assembly );
				if ( !_fileSystem.FileExists( assemblyPath ) ) {
					throw new FileNotFoundException( $"Module assembly missing for '{manifest.Id}'", assemblyPath );
				}

				if ( !string.IsNullOrWhiteSpace( manifest.Pck ) ) {
					string pckPath = Path.Combine( manifest.DirectoryPath, manifest.Pck );
					if ( !_fileSystem.FileExists( pckPath ) ) {
						throw new FileNotFoundException( $"Module PCK missing for '{manifest.Id}'", pckPath );
					}
				}

				foreach ( var dependency in manifest.Dependencies ) {
					bool found = manifests.Any( m => string.Equals( m.Id, dependency.Id, StringComparison.OrdinalIgnoreCase ) );

					if ( !found && !dependency.Optional ) {
						throw new InvalidOperationException(
							$"Module '{manifest.Id}' requires missing dependency '{dependency.Id}'."
						);
					}
				}

				foreach ( var incompatibleId in manifest.Incompatibilities ) {
					bool found = manifests.Any( m => string.Equals( m.Id, incompatibleId, StringComparison.OrdinalIgnoreCase ) );

					if ( found ) {
						throw new InvalidOperationException(
							$"Module '{manifest.Id}' is incompatible with '{incompatibleId}'"
						);
					}
				}
			}
		}

		private List<ModuleManifest> SortByDependencies( List<ModuleManifest> manifests )
		{
			var byId = manifests.ToDictionary( m => m.Id, StringComparer.OrdinalIgnoreCase );
			var visited = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
			var visiting = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
			var result = new List<ModuleManifest>();

			foreach ( var manifest in manifests.OrderBy( m => m.LoadPriority ) ) {
				Visit( manifest );
			}

			return result;

			void Visit( ModuleManifest manifest )
			{
				if ( visited.Contains( manifest.Id ) ) {
					return;
				}
				if ( !visiting.Add( manifest.Id ) ) {
					throw new InvalidOperationException( $"Circular module dependency involving '{manifest.Id}'" );
				}

				foreach ( var dependency in manifest.Dependencies ) {
					if ( byId.TryGetValue( dependency.Id, out var dependencyManifest ) ) {
						Visit( dependencyManifest );
					}
				}

				visiting.Remove( manifest.Id );
				visited.Add( manifest.Id );
				result.Add( manifest );
			}
		}

		private DiscoveredModule LoadModuleAssembly( ModuleManifest manifest )
		{
			string assemblyPath = Path.Combine( manifest.DirectoryPath, manifest.Assembly );
			string fullAssemblyPath = Path.Combine( assemblyPath );

			var report = _validator.Validate( assemblyPath );
			if ( !report.IsAllowed ) {
				throw new Exception();
			}

			var loadContext = new ModuleLoadContext( fullAssemblyPath, _policy );
			Assembly assembly = loadContext.LoadFromAssemblyPath( fullAssemblyPath );

			Type? entryType = assembly.GetType( manifest.EntryType, throwOnError: false );

			if ( entryType == null ) {
				throw new InvalidOperationException(
					$"Module '{manifest.Id}' entr type not found: {manifest.EntryType}"
				);
			}

			if ( !typeof( INomadModule ).IsAssignableFrom( entryType ) ) {
				throw new InvalidOperationException(
					$"Module '{manifest.Id}' entr type does not implement INomadModule: {manifest.EntryType}"
				);
			}

			if ( Activator.CreateInstance( entryType ) is not INomadModule instance ) {
				throw new InvalidOperationException(
					$"Could not instantiate module entry type: {manifest.EntryType}"
				);
			}

			if ( !string.Equals( instance.Id, manifest.Id, StringComparison.OrdinalIgnoreCase ) ) {
				throw new InvalidOperationException(
					$"Module ID mismatch. Manifest says '{manifest.Id}', instance says '{instance.Id}'."
				);
			}

			return new DiscoveredModule {
				Manifest = manifest,
				LoadContext = loadContext,
				Assembly = assembly,
				Instance = instance
			};
		}
	};
};
