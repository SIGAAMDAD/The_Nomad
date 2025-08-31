/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

namespace ResourceCache {
	/*
	===================================================================================
	
	PreLoader
	
	===================================================================================
	*/

	public partial class PreLoader : Node {
		private static ConcurrentDictionary<string, Resource>? DataCache = null;

		/*
		===============
		GetResource
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Resource? GetResource( string path ) {
			ArgumentException.ThrowIfNullOrEmpty( path );
			ArgumentNullException.ThrowIfNull( DataCache );

			if ( DataCache.TryGetValue( path, out Resource? resource ) ) {
				return resource;
			}

			resource = ResourceLoader.Load( path );
			if ( resource == null ) {
				Console.PrintError( $"ResourceCache.Load: invalid resource \"{path}\"" );
				return null;
			}
			DataCache.TryAdd( path, resource );
			return resource;
		}

		/*
		===============
		Clear
		===============
		*/
		/// <summary>
		/// Clears the cache
		/// </summary>
		public static void Clear() {
			ArgumentNullException.ThrowIfNull( DataCache );
			DataCache.Clear();
		}

		/*
		===============
		_EnterTree
		===============
		*/
		public override void _EnterTree() {
			base._EnterTree();
			ProcessMode = ProcessModeEnum.Disabled;

			Console.PrintLine( "Initializing PreLoader..." );

			DataCache = new ConcurrentDictionary<string, Resource>();
		}
	};
};