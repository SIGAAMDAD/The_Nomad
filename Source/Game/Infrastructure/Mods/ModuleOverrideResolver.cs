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
using System.Linq;
using System.Collections.Generic;
using Nomad.Game.Domain.Data.Mods;

namespace Nomad.Game.Infrastructure.Mods
{
	public static class ModuleOverrideResolver
	{
		public static List<DiscoveredModule> Resolve( List<DiscoveredModule> discovered )
		{
			var byId = discovered.ToDictionary( x => x.Manifest.Id, StringComparer.Ordinal );
			var replacedBy = new Dictionary<string, DiscoveredModule>( StringComparer.Ordinal );

			foreach ( var module in discovered ) {
				foreach ( var target in module.Manifest.Replaces ) {
					if ( !byId.ContainsKey( target ) ) {
						throw new InvalidOperationException(
							$"{module.Manifest.Id} replaces missing module '{target}'."
						);
					}

					if ( replacedBy.TryGetValue( target, out DiscoveredModule? value ) ) {
						throw new InvalidOperationException(
							$"Multiple modules replace '{target}': " +
							$"{value.Manifest.Id}, {module.Manifest.Id}"
						);
					}

					replacedBy[target] = module;
				}
			}

			var active = new List<DiscoveredModule>( discovered.Count );

			foreach ( var module in discovered ) {
				if ( replacedBy.ContainsKey( module.Manifest.Id ) ) {
					continue;
				}
				active.Add( module );
			}

			return active;
		}
	}
};
