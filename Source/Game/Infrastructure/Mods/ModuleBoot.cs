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
using Godot;
using Nomad.Core.Abstractions;
using Nomad.Core.CVars;
using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Infrastructure.Mods
{
	internal sealed class ModuleBoot : IBootstrapper
	{
		private const string RUNTIME_API_VERSION = "1.0.0";

		private readonly List<DiscoveredModule> _modules = new();
		private ILoggerCategory? _category;
		private ModDiagnostics? _diagnostics;

		public IReadOnlyList<DiscoveredModule> Modules => _modules;

		public void Initialize( IServiceRegistry registry, IServiceLocator locator )
		{
			try {
				var fileSystem = locator.GetService<IFileSystem>();
				var logger = locator.GetService<ILoggerService>();
				var eventFactory = locator.GetService<IGameEventRegistryService>();
				var cvarSystem = locator.GetService<ICVarSystemService>();
				var contentCache = locator.GetService<IWorldContentCache>();
				var behaviorRegistry = locator.GetService<INomadBehaviorRegistry>();

				_category = logger.CreateCategory( nameof( ModuleBoot ), LogLevel.Info, true );
				_category.PrintLine( "Loading external modules..." );

				var diagnostics = new ModDiagnostics( new ModDiagnosticsPolicy() );
				_diagnostics = diagnostics;

				string executableDir = System.Environment.CurrentDirectory;

				var scanner = new ModuleScanner( RUNTIME_API_VERSION, fileSystem, _category );
				scanner.AddScanRoot( ProjectSettings.GlobalizePath( "res://Assets/Modules" ) );
				scanner.AddScanRoot( Path.Combine( System.Environment.CurrentDirectory, "Assets", "Modules" ) );
				scanner.AddScanRoot( Path.Combine( fileSystem.GetResourcePath(), "Modules" ) );
				scanner.AddScanRoot( Path.Combine( executableDir, "Modules" ) );
				scanner.AddScanRoot( Path.Combine( AppContext.BaseDirectory, "Modules" ) );

				string userMods = Path.Combine( fileSystem.GetUserDataPath(), "Modules" );
				scanner.AddScanRoot( userMods );

				foreach ( var module in scanner.ScanAndLoad() ) {
					_modules.Add(
						module with {
							Context = CreateContext(
								module.Manifest,
								fileSystem,
								logger,
								eventFactory,
								cvarSystem,
								contentCache,
								behaviorRegistry
							)
						}
					);
				}

				LoadResourcePacks();
				PreLoadModules();
				InitializeModules();

				registry.AddSingleton( this );
				registry.AddSingleton<IModDiagnostics>( diagnostics );
			} catch ( Exception ex ) {
				string message = $"Module bootstrap failed: {ex}";
				if ( _category != null ) {
					_category.PrintError( message );
				} else {
					GD.PushError( message );
				}
			} finally {
				_category?.PrintLine( $"Loaded {_modules.Count} module(s)." );
			}
		}

		public void Shutdown()
		{
			for ( int i = _modules.Count - 1; i >= 0; i-- ) {
				ShutdownModule( _modules[i] );
				_modules[i].LoadContext.Unload();
			}

			_modules.Clear();
			_diagnostics = null;
			_category?.Dispose();
			_category = null;
		}

		private void PreLoadModules()
		{
			foreach ( var module in _modules ) {
				try {
					_category?.PrintLine( $"Preloading module '{module.Manifest.Id}'." );
					module.Instance.OnPreLoad( GetContext( module ) );
				} catch ( Exception ex ) {
					_diagnostics?.ReportException(
						module.Manifest,
						ex,
						nameof( INomadModule.OnPreLoad )
					);

					_category?.PrintError(
						$"Module '{module.Manifest.Id}' failed during preload: {ex}"
					);
				}
			}
		}

		private void InitializeModules()
		{
			foreach ( var module in _modules ) {
				try {
					_category?.PrintLine( $"Initializing module '{module.Manifest.Id}'." );
					module.Instance.OnInitialized( GetContext( module ) );
				} catch ( Exception ex ) {
					_diagnostics?.ReportException(
						module.Manifest,
						ex,
						nameof( INomadModule.OnInitialized )
					);

					_category?.PrintError(
						$"Module '{module.Manifest.Id}' failed during initialization: {ex}"
					);
				}
			}
		}

		private void ShutdownModule( DiscoveredModule module )
		{
			try {
				module.Instance.OnShutdown( GetContext( module ) );
			} catch ( Exception ex ) {
				_diagnostics?.ReportException(
					module.Manifest,
					ex,
					nameof( INomadModule.OnShutdown )
				);

				_category?.PrintError(
					$"Module '{module.Manifest.Id}' failed during shutdown: {ex}"
				);
			} finally {
				if ( module.Context is IDisposable context ) {
					context.Dispose();
				}
			}
		}

		private void LoadResourcePacks()
		{
			foreach ( var module in _modules ) {
				if ( string.IsNullOrWhiteSpace( module.Manifest.Pck ) ) {
					continue;
				}

				string pckPath = Path.Combine( module.Manifest.DirectoryPath, module.Manifest.Pck );

				if ( !ProjectSettings.LoadResourcePack( pckPath ) ) {
					_category?.PrintWarning( $"Failed to load module resource pack '{pckPath}'." );
				}
			}
		}

		private ModuleContext CreateContext(
			ModuleManifest manifest,
			IFileSystem fileSystem,
			ILoggerService logger,
			IGameEventRegistryService eventFactory,
			ICVarSystemService cvarSystem,
			IWorldContentCache contentCache,
			INomadBehaviorRegistry behaviorRegistry
		)
		{
			if ( _diagnostics == null ) {
				throw new InvalidOperationException( "Module diagnostics must be initialized before module contexts are created." );
			}

			var policy = new ModSecurityPolicy();
			string safeId = ModPathSanitizer.NormalizeSegment( manifest.Id );
			var roots = new ModFileSystemRoots(
				Path.Combine( manifest.DirectoryPath, "Content" ),
				Path.Combine( fileSystem.GetUserDataPath(), "Modules", safeId )
			);

			return new ModuleContext(
				manifest,
				RUNTIME_API_VERSION,
				contentCache,
				behaviorRegistry,
				new ModEventRegistry(
					manifest,
					eventFactory,
					_diagnostics,
					policy,
					Array.Empty<string>()
				),
				new ModLogger(
					logger,
					_diagnostics,
					manifest,
					policy
				),
				new ModFileSystem(
					fileSystem,
					_diagnostics,
					manifest,
					roots
				),
				new ModCVarSystem(
					manifest,
					cvarSystem,
					_diagnostics,
					new ModCVarPolicy()
				)
			);
		}

		private static IModuleContext GetContext( DiscoveredModule module )
		{
			if ( module.Context == null ) {
				throw new InvalidOperationException(
					$"Module '{module.Manifest.Id}' has no runtime context."
				);
			}

			return module.Context;
		}
	};
};
