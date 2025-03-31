using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;

namespace Renown.World {
	public class DataCache<T> where T : Node {
		public Dictionary<int, T> Cache = null;

		public DataCache( Node nodeTree, StringName groupName ) {
			Godot.Collections.Array<Node> nodes = nodeTree.GetTree().GetNodesInGroup( groupName );

			Console.PrintLine( string.Format( "Initializing DataCache[{0}]...", typeof( T ).ToString() ) );

			Cache = new Dictionary<int, T>( nodes.Count );
			for ( int i = 0; i < nodes.Count; i++ ) {
				Console.PrintLine( string.Format( "...Added {0}", nodes[i].GetPath() ) );
				Cache.Add( nodes[i].GetPath().GetHashCode(), nodes[i] as T );
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
			foreach ( var cacheEntry in Cache ) {
				cacheEntry.Value?.Call( "Load" );
			}
		}
	};
};