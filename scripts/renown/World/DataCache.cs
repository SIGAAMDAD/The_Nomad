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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

namespace Renown.World {
	public class DataCache<T> where T : Node {
		public Dictionary<int, T>? Cache = null;

		public DataCache( Node nodeTree, StringName groupName ) {
			Godot.Collections.Array<Node> nodes = nodeTree.GetTree().GetNodesInGroup( groupName );

			Console.PrintLine( string.Format( "Initializing DataCache[{0}]...", typeof( T ).ToString() ) );

			Cache = new Dictionary<int, T>( nodes.Count );
			for ( int i = 0; i < nodes.Count; i++ ) {
				Console.PrintLine( string.Format( "...Added {0}", nodes[ i ].GetPath() ) );
				Cache.Add( nodes[ i ].GetPath().GetHashCode(), nodes[ i ] as T );
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SearchCache( NodePath name ) {
			if ( Cache.TryGetValue( name.GetHashCode(), out T value ) ) {
				return value;
			}
			return null;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SearchCache( int hash ) {
			if ( Cache.TryGetValue( hash, out T value ) ) {
				return value;
			}
			return null;
		}
		public void ClearCache() {
			foreach ( var entry in Cache ) {
				entry.Value.QueueFree();
			}
			Cache.Clear();
		}
		public void Load() {
			Parallel.ForEach( Cache, ( source ) => source.Value.Call( "Load" ) );
		}
		public T FindNearest( in Godot.Vector2 position ) {
			T best = null;
			float bestDistance = float.MaxValue;
			foreach ( var node in Cache ) {
				float distance = ( node.Value as Node2D ).GlobalPosition.DistanceTo( position );
				if ( distance < bestDistance ) {
					bestDistance = distance;
					best = node.Value;
				}
			}
			return best;
		}
	};
};