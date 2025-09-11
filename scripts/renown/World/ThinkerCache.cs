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

using System.Collections.Generic;
using Godot;
using Renown.Thinkers;

namespace Renown.World {
	public partial class ThinkerCache : Node {
		public static int ThinkerCount = 0;
		private static ThinkerCache Instance = null;

		private Dictionary<int, Thinker> Thinkers = null;

		public void Load() {
			using var reader = ArchiveSystem.GetSection( "ThinkerCache" );
			int cacheSize = reader.LoadInt( "ThinkerCacheSize" );
			Thinkers = new Dictionary<int, Thinker>( cacheSize );

			System.Threading.Tasks.Parallel.For( 0, cacheSize, ( index ) => {
				string key = string.Format( "Thinker{0}", index );

				bool premade = reader.LoadBoolean( key + "IsPremade" );

				Thinker thinker = new Thinker();
				thinker.Load( reader, index );

				if ( !premade ) {
					AddThinker( thinker );
				}
			} );
		}
		public void Save() {
			using var writer = new SaveSystem.SaveSectionWriter( "ThinkerCache", ArchiveSystem.SaveWriter );
			writer.SaveInt( "ThinkerCacheSize", Thinkers.Count );

			int index = 0;
			foreach ( var thinker in Thinkers ) {
				thinker.Value.Save( writer, index );
				index++;
			}
		}
		
		public static void AddThinker( Thinker thinker ) {
			Instance.Thinkers.TryAdd( thinker.GetHashCode(), thinker );
			Instance.CallDeferred( "add_child", thinker );
			System.Threading.Interlocked.Increment( ref ThinkerCount );
		}

		public override void _Ready() {
			base._Ready();

			Instance = this;

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}

			if ( ArchiveSystem.IsLoaded() ) {
				Load();
			} else {
				Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Thinkers" );

				Thinkers = new Dictionary<int, Thinker>( nodes.Count );
				for ( int i = 0; i < nodes.Count; i++ ) {
					Thinker thinker = nodes[ i ] as Thinker;
					Thinkers.Add( thinker.GetHashCode(), thinker );
				}
			}
		}
	};
};