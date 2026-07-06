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
using System.Reflection;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Content.Mods
{
	internal sealed class ModuleBehaviorDiscoveryService<TBehavior>
		where TBehavior : class
	{
		private readonly ModuleBehaviorRegistry<TBehavior> _registry;
		private readonly ModuleBehaviorLoader<TBehavior> _loader = new();

		public ModuleBehaviorDiscoveryService( ModuleBehaviorRegistry<TBehavior> registry )
		{
			_registry = registry ?? throw new ArgumentNullException( nameof( registry ) );
		}

		public void LoadModuleAssembly( string assemblyPath )
		{
			string fullPath = Path.GetFullPath( assemblyPath );

			var loadContext = new ModuleLoadContext(
				fullPath,
				new ModSecurityPolicy()
			);
			Assembly assembly = loadContext.LoadFromAssemblyPath( fullPath );

			IReadOnlyList<ModuleBehaviorDescriptor> descriptors = _loader.LoadFromAssembly( assembly );

			foreach ( var descriptor in descriptors ) {
				_registry.Register(
					descriptor.Id,
					() => _loader.Create( descriptor.Type )
				);
			}
		}
	};
};
