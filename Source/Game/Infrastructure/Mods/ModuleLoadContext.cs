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
using System.Reflection;
using System.Runtime.Loader;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Infrastructure.Mods
{
	/*
	===================================================================================

	ModuleLoadContext

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class ModuleLoadContext : AssemblyLoadContext
	{
		private readonly AssemblyDependencyResolver _resolver;
		private readonly ModSecurityPolicy _policy;

		/*
		===============
		ModuleLoadContext
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="mainAssemblyPath"></param>
		/// <param name="policy"></param>
		public ModuleLoadContext( string mainAssemblyPath, ModSecurityPolicy policy )
			: base( isCollectible: true )
		{
			_resolver = new AssemblyDependencyResolver( mainAssemblyPath );
			_policy = policy;
		}

		/*
		===============
		Load
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		protected override Assembly? Load( AssemblyName assemblyName )
		{
			string name = assemblyName.Name ?? string.Empty;

			if ( IsForbiddenNomadAssembly( name ) ) {
				throw new InvalidOperationException(
					$"Mod attempted to load forbidden framework assembly '{name}'." +
					$"Mods must use Nomad.Modding or Nomad.Game.Sdk"
				);
			}

			if ( IsSharedAssembly( name ) ) {
				return null;
			}

			string? path = _resolver.ResolveAssemblyToPath( assemblyName );

			if ( path != null ) {
				return LoadFromAssemblyPath( path );
			}

			return null;
		}

		/*
		===============
		LoadUnmanagedDll
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="unmanagedDllName"></param>
		/// <returns></returns>
		protected override IntPtr LoadUnmanagedDll( string unmanagedDllName )
		{
			string? path = _resolver.ResolveUnmanagedDllToPath( unmanagedDllName );

			if ( path != null ) {
				return LoadUnmanagedDllFromPath( path );
			}

			return IntPtr.Zero;
		}

		/*
		===============
		IsSharedAssembly
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private bool IsSharedAssembly( string name )
		{
			return _policy.AllowedSharedAssemblies.Contains( name, StringComparer.Ordinal );
		}

		/*
		===============
		IsForbiddenNomadAssembly
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private bool IsForbiddenNomadAssembly( string name )
		{
			for ( int i = 0; i < _policy.ForbiddenAssemblyPrefixes.Length; i++ ) {
				if ( name.StartsWith( _policy.ForbiddenAssemblyPrefixes[i], StringComparison.Ordinal ) ) {
					return true;
				}
			}

			return false;
		}
	};
};
