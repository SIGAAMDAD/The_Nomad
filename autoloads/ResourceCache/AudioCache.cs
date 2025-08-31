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
using Godot;

namespace ResourceCache {
	/*
	===================================================================================
	
	AudioCache
	
	===================================================================================
	*/

	public partial class AudioCache : Node {
		private static ConcurrentDictionary<string, AudioStream>? DataCache = null;

		/*
		===============
		GetStream
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static AudioStream? GetStream( string path ) {
			ArgumentException.ThrowIfNullOrEmpty( path );
			ArgumentNullException.ThrowIfNull( DataCache );

			if ( DataCache.TryGetValue( path, out AudioStream? stream ) ) {
				return stream;
			}

			stream = ResourceLoader.Load<AudioStream>( path );
			if ( stream == null ) {
				Console.PrintError( $"AudioCache.Load: invalid audio file \"{path}\"" );
				return null;
			}
			DataCache.TryAdd( path, stream );
			return stream;
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

			Console.PrintLine( "Initializing AudioCache..." );

			DataCache = new ConcurrentDictionary<string, AudioStream>();
		}
	};
};