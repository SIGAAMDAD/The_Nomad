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
using System.Reflection;
using System.Runtime.Loader;

namespace Nomad.Game.Infrastructure.Mods
{
	public sealed class ModuleLoadContext : AssemblyLoadContext
	{
		private static readonly HashSet<string> SharedAssemblies = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) {
			"System",
			"System.Runtime",
			"GodotSharp",
			"GodotSharpEditor",
			"TheNomad",
		};

		private readonly AssemblyDependencyResolver _resolver;

		public ModuleLoadContext( string mainAssemblyPath )
			: base( isCollectible: true )
		{
			_resolver = new AssemblyDependencyResolver( mainAssemblyPath );
		}

		protected override Assembly? Load( AssemblyName assemblyName )
		{
			if ( assemblyName.Name != null && SharedAssemblies.Contains( assemblyName.Name ) ) {
				return null; // let default context provide shared contracts.
			}

			string? path = _resolver.ResolveAssemblyToPath( assemblyName );

			if ( path != null ) {
				return LoadFromAssemblyPath( path );
			}

			return null;
		}

		protected override IntPtr LoadUnmanagedDll( string unmanagedDllName )
		{
			string? path = _resolver.ResolveUnmanagedDllToPath( unmanagedDllName );

			if ( path != null ) {
				return LoadUnmanagedDllFromPath( path );
			}

			return IntPtr.Zero;
		}
	};
};
