using System;
using System.Collections.Generic;
using System.Linq;
using Nomad.Game.Domain.Data.Mods;

namespace Nomad.Game.Infrastructure.Mods {
	public static class ModuleOrderer {
		public static List<DiscoveredModule> TopologicalSort( List<DiscoveredModule> modules ) {
			var byId = modules.ToDictionary( x => x.Manifest.Id, StringComparer.Ordinal );
			var visited = new Dictionary<string, int>( StringComparer.Ordinal );
			var ordered = new List<DiscoveredModule>( modules.Count );

			foreach ( var module in modules ) {
				Visit( module );
			}

			return ordered;

			void Visit( DiscoveredModule module ) {
				if ( visited.TryGetValue( module.Manifest.Id, out var state ) ) {
					if ( state == 1 ) {
						throw new InvalidOperationException( $"Cycle detected at {module.Manifest.Id}" );
					}
					if ( state == 2 ) {
						return;
					}
				}

				visited[module.Manifest.Id] = 1;

				foreach ( var dep in module.Manifest.Dependencies ) {
					if ( !byId.TryGetValue( dep, out var depModule ) ) {
						throw new InvalidOperationException(
							$"{module.Manifest.Id} depends on missing module '{dep}'." );
					}
					Visit( depModule );
				}

				foreach ( var after in module.Manifest.LoadAfter ) {
					if ( byId.TryGetValue( after, out var afterModule ) ) {
						Visit( afterModule );
					}
				}

				visited[module.Manifest.Id] = 2;
				ordered.Add( module );
			}
		}
	};
};