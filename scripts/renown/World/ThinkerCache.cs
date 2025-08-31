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