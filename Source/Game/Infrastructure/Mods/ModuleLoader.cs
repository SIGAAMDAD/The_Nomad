using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Nomad.Core.ServiceRegistry.Services;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Domain.Data.Mods;
using Nomad.Game.Domain.Interfaces.Mods;

namespace Nomad.Game.Infrastructure.Mods {
	public sealed class ModuleLoader {
		private readonly IServiceRegistry _serviceRegistry;
		private readonly Dictionary<string, LoadedModule> _loaded = new( StringComparer.Ordinal );

		public ModuleLoader( IServiceRegistry serviceRegistry ) {
			_serviceRegistry = serviceRegistry;
		}

		public IReadOnlyDictionary<string, LoadedModule> LoadedModules => _loaded;

		public void LoadAll( string modulesRoot ) {
			var discovered = ModuleDiscovery.Discover( modulesRoot );
			discovered = ModuleOverrideResolver.Resolve( discovered );
			var ordered = ModuleOrderer.TopologicalSort( discovered );

			LoadModules( ordered );
			RegisterServices();
			var services = new ServiceLocator( _serviceRegistry as ServiceCollection );
			InitializeModules( services );
			StartModules();
		}

		private void LoadModules( List<DiscoveredModule> ordered ) {
			foreach ( var module in ordered ) {
				var assemblyPath = Path.Combine( module.Directory, module.Manifest.EntryAssembly );
				if ( !File.Exists( assemblyPath ) ) {
					throw new InvalidOperationException(
						$"{module.Manifest.Id}: missing assembly '{assemblyPath}'" );
				}

				var alc = new ModuleLoadContext( assemblyPath );
				var asm = alc.LoadFromAssemblyPath( assemblyPath );

				var type = asm.GetType( module.Manifest.EntryType, throwOnError: true )
					?? throw new InvalidOperationException(
						$"{module.Manifest.Id}: entry type '{module.Manifest.EntryType}' not found" );

				if ( !typeof( IGameModule ).IsAssignableFrom( type ) ) {
					throw new InvalidOperationException(
						$"{module.Manifest.Id}: entry type must implement IGameModule" );
				}

				var ctor = ModuleActivatorCache.GetOrCreate( type );
				var instance = ctor();

				_loaded.Add( module.Manifest.Id, new LoadedModule {
					Directory = module.Directory,
					Manifest = module.Manifest,
					LoadContext = alc,
					Assembly = asm,
					Instance = instance,
					State = ModuleState.Loaded
				} );
			}
		}

		private void RegisterServices() {
			foreach ( var module in _loaded.Values ) {
				module.Instance.RegisterServices( _serviceRegistry );
				module.State = ModuleState.Registered;
			}
		}

		private void InitializeModules( IServiceLocator services ) {
			foreach ( var module in _loaded.Values ) {
				var host = new ModuleHost( services, module.Directory, module.Manifest );
				module.Instance.Initialize( host );
				module.State = ModuleState.Initialized;
			}
		}

		private void StartModules() {
			foreach ( var module in _loaded.Values ) {
				module.Instance.Start();
				module.State = ModuleState.Started;
			}
		}

		public void UnloadAll() {
			foreach ( var module in _loaded.Values.Reverse() ) {
				try { module.Instance.Stop(); } catch { }
				try { module.Instance.Dispose(); } catch { }

				module.State = ModuleState.Stopped;
				module.LoadContext.Unload();
			}

			_loaded.Clear();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
	};
};