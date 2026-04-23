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

using System.Runtime.Loader;
using System.Reflection;
using System;
using System.IO;
using GodotPlugins.Game;

namespace Nomad.Game.Infrastructure.Mods {
	/*
	===================================================================================
	
	ModuleLoadContext
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class ModuleLoadContext : AssemblyLoadContext {
		private readonly AssemblyDependencyResolver _resolver;

		public string MainAssemblyPath { get; }

		public ModuleLoadContext( string mainAssemblyPath )
			: base( name: Path.GetFileNameWithoutExtension( mainAssemblyPath ), isCollectible: true )
		{
			MainAssemblyPath = mainAssemblyPath;
			_resolver = new AssemblyDependencyResolver( mainAssemblyPath );
		}

		protected override Assembly? Load( AssemblyName assemblyName ) {
			string? path = _resolver.ResolveAssemblyToPath( assemblyName );
			return path == null ? null : LoadFromAssemblyPath( path );
		}

		protected override IntPtr LoadUnmanagedDll( string unmanagedDllName ) {
			string? path = _resolver.ResolveUnmanagedDllToPath( unmanagedDllName );
			return path == null ? IntPtr.Zero : LoadUnmanagedDllFromPath( path );
		}
	};
};