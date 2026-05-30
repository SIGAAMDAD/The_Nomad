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
using Nomad.Core.Logger;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Infrastructure.Mods
{
	/*
	===================================================================================

	ModuleScanner

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class ModuleScanner
	{
		private readonly string _apiVersion;
		private readonly List<string> _scanRoots = new();

		private readonly IFileSystem _fileSystem;

		private readonly ModAssemblyValidator _validator;
		private readonly ModSecurityPolicy _policy;
		private readonly ILoggerCategory? _category;

		/*
		===============
		ModuleScanner
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="apiVersion"></param>
		/// <param name="fileSystem"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ModuleScanner( string apiVersion, IFileSystem fileSystem, ILoggerCategory? category = null )
		{
			_apiVersion = apiVersion;
			_fileSystem = fileSystem ?? throw new ArgumentNullException( nameof( fileSystem ) );
			_category = category;

			_policy = new ModSecurityPolicy();
			_validator = new ModAssemblyValidator( _policy );
		}

		/*
		===============
		AddScanRoot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="path"></param>
		public void AddScanRoot( string path )
		{
			if ( string.IsNullOrWhiteSpace( path ) ) {
				return;
			}

			string normalizedPath = Path.GetFullPath( path );

			if ( !_scanRoots.Contains( normalizedPath, StringComparer.OrdinalIgnoreCase ) ) {
				_scanRoots.Add( normalizedPath );
			}
		}

		/*
		===============
		ScanAndLoad
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public IReadOnlyList<DiscoveredModule> ScanAndLoad()
		{
			var manifests = ScanManifests();
			if ( manifests.Count == 0 ) {
				_category?.PrintWarning( "No module manifests were discovered." );
				return Array.Empty<DiscoveredModule>();
			}

			ValidateManifests( manifests );

			var sorted = SortByDependencies( manifests );
			var loaded = new List<DiscoveredModule>();

			foreach ( var manifest in sorted ) {
				try {
					var module = LoadModuleAssembly( manifest );
					loaded.Add( module );
				} catch ( Exception ex ) {
					_category?.PrintError(
						$"Failed to load module '{manifest.Id}' from '{manifest.DirectoryPath}': {ex}"
					);
				}
			}

			return loaded;
		}

		/*
		===============
		ScanManifests
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private List<ModuleManifest> ScanManifests()
		{
			var manifests = new List<ModuleManifest>();

			foreach ( var root in _scanRoots ) {
				if ( !_fileSystem.DirectoryExists( root ) ) {
					_category?.PrintDebug( $"Module scan root does not exist: '{root}'." );
					continue;
				}

				_category?.PrintDebug( $"Scanning module root: '{root}'." );

				foreach ( var directory in _fileSystem.GetDirectories( root ) ) {
					string manifestPath = Path.Combine( directory, "module.json" );

					if ( !_fileSystem.FileExists( manifestPath ) ) {
						_category?.PrintDebug( $"Skipping module directory without manifest: '{directory}'." );
						continue;
					}

					try {
						_category?.PrintLine( $"Found module manifest: '{manifestPath}'." );

						using var fileBuffer = _fileSystem.LoadFile( manifestPath );

						var manifest = JsonSerializer.Deserialize<ModuleManifest>(
							fileBuffer.ToArray(),
							new JsonSerializerOptions {
								PropertyNameCaseInsensitive = true,
								ReadCommentHandling = JsonCommentHandling.Skip,
								AllowTrailingCommas = true
							}
						);

						if ( manifest == null ) {
							_category?.PrintWarning( $"Module manifest '{manifestPath}' did not deserialize." );
							continue;
						}

						manifest.DirectoryPath = directory;

						manifests.Add( manifest );
					}
					catch ( Exception ex ) {
						_category?.PrintError(
							$"Failed to read module manifest '{manifestPath}': {ex}"
						);
					}
				}
			}
			return manifests;
		}

		/*
		===============
		ValidateManifests
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="manifests"></param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
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

		/*
		===============
		SortByDependencies
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="manifests"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
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

		/*
		===============
		LoadModuleAssembly
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="manifest"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		private DiscoveredModule LoadModuleAssembly( ModuleManifest manifest )
		{
			string assemblyPath = Path.Combine( manifest.DirectoryPath, manifest.Assembly );
			string fullAssemblyPath = Path.GetFullPath( assemblyPath );

			_category?.PrintLine( $"Loading module '{manifest.Id}' from '{fullAssemblyPath}'." );

			var report = _validator.Validate( fullAssemblyPath );
			if ( !report.IsAllowed ) {
				string issues = string.Join(
					Environment.NewLine,
					report.Issues.Select(
						issue => $"{issue.Code}: {issue.Message}" +
							(string.IsNullOrWhiteSpace( issue.Location ) ? string.Empty : $" ({issue.Location})")
					)
				);

				throw new InvalidOperationException(
					$"Module '{manifest.Id}' failed assembly validation:{Environment.NewLine}{issues}"
				);
			}

			var loadContext = new ModuleLoadContext( fullAssemblyPath, _policy );
			Assembly assembly = loadContext.LoadFromAssemblyPath( fullAssemblyPath );

			_category.PrintLine( $"Assembly Info:" );
			_category.PrintLine( $"[Name] {assembly.FullName}" );
			_category.PrintLine( $"[Defined Types] {assembly.DefinedTypes}" );
			_category.PrintLine( $"[Runtime Version] {assembly.ImageRuntimeVersion}" );
			_category.PrintLine( $"[Exported Types] {assembly.ExportedTypes}" );

			Type? entryType = assembly.GetType( manifest.EntryType, throwOnError: false );

			if ( entryType == null ) {
				throw new InvalidOperationException(
					$"Module '{manifest.Id}' entry type not found: {manifest.EntryType}"
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
