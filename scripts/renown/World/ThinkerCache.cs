using System.Collections.Generic;
using Godot;
using Renown.Thinkers;

namespace Renown.World {
	public partial class ThinkerCache : Node {
		private Dictionary<int, Thinker> Thinkers = null;
		private Dictionary<int, Family> Families = null;

		public static int ThinkerCount = 0;

		private static ThinkerCache Instance = null;

		public void Load() {
			using ( var reader = ArchiveSystem.GetSection( "FamilyCache" ) ) {
				int cacheSize = reader.LoadInt( "CacheSize" );
				Families = new Dictionary<int, Family>( cacheSize );

				for ( int i = 0; i < cacheSize; i++ ) {
					Family family = new Family();
					family.Load( reader, i );
					Families.Add( family.GetHashCode(), family );
				}
			}
			using ( var reader = ArchiveSystem.GetSection( "ThinkerCache" ) ) {
				int cacheSize = reader.LoadInt( "CacheSize" );
				Thinkers = new Dictionary<int, Thinker>( cacheSize );

				for ( int i = 0; i < cacheSize; i++ ) {
					string key = string.Format( "Thinker{0}", i );

					bool premade = reader.LoadBoolean( key + "IsPremade" );

					Thinker thinker;
					if ( premade ) {
						thinker = GetTree().Root.GetNode<Thinker>( reader.LoadString( key + "InitialPath" ) );
					} else {
						thinker = new Thinker();
					}
					thinker.Load( reader, i );

					Thinkers.Add( thinker.GetHashCode(), thinker );
				}
			}
		}
		public void Save() {
			using ( var writer = new SaveSystem.SaveSectionWriter( "FamilyCache" ) ) {
				writer.SaveInt( "CacheSize", Families.Count );

				int index = 0;
				foreach ( var family in Families ) {
					family.Value.Save( writer, index );
					index++;
				}
			}
			using ( var writer = new SaveSystem.SaveSectionWriter( "ThinkerCache" ) ) {
				writer.SaveInt( "CacheSize", Thinkers.Count );

				int index = 0;
				foreach ( var thinker in Thinkers ) {
					thinker.Value.Save( writer, index );
					index++;
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			Instance = this;

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}

			if ( ArchiveSystem.Instance.IsLoaded() ) {
				Load();
			} else {
				Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Thinkers" );
				
				Thinkers = new Dictionary<int, Thinker>( nodes.Count );
				for ( int i = 0; i < nodes.Count; i++ ) {
					Thinker thinker = nodes[i] as Thinker;
					Thinkers.Add( thinker.GetHashCode(), thinker );
				}

				Families = new Dictionary<int, Family>();
			}
		}

		public static Family GetFamily( Settlement settlement ) {
			foreach ( var family in Instance.Families ) {
				if ( family.Value.CanAddMember() ) {
					return family.Value;
				}
			}
			Family instance = Family.Create( settlement );
			Instance.Families.Add( instance.GetHashCode(), instance );
			return instance;
		}
		public static void AddThinker( Thinker thinker ) {
			Instance.Thinkers.TryAdd( thinker.GetHashCode(), thinker );
			Instance.CallDeferred( "add_child", thinker );
			System.Threading.Interlocked.Increment( ref ThinkerCount );
		}
	};
};