using System.Collections.Generic;
using Godot;
using Renown.Thinkers;

namespace Renown.World {
	public partial class FamilyCache : Node {
		private Dictionary<int, Family> Families = null;

		public static int FamilyCount = 0;

		private static FamilyCache Instance = null;

		public void Load() {
			using ( var reader = ArchiveSystem.GetSection( "FamilyCache" ) ) {
				int cacheSize = reader.LoadInt( "FamilyCacheSize" );
				Families = new Dictionary<int, Family>( cacheSize );

				for ( int i = 0; i < cacheSize; i++ ) {
					Family family = new Family();
					AddChild( family );
					family.Load( reader, i );
					Families.Add( family.GetHashCode(), family );
				}
			}
		}
		public void Save() {
			using ( var writer = new SaveSystem.SaveSectionWriter( "FamilyCache" ) ) {
				writer.SaveInt( "FamilyCacheSize", Families.Count );

				int index = 0;
				foreach ( var family in Families ) {
					family.Value.Save( writer, index );
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
				Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Families" );
				
				Families = new Dictionary<int, Family>( nodes.Count );
				for ( int i = 0; i < nodes.Count; i++ ) {
					Family thinker = nodes[i] as Family;
					Families.Add( thinker.GetHashCode(), thinker );
				}
			}
		}

		public static Family GetFamily( string name ) {
			foreach ( var family in Instance.Families ) {
				if ( family.Value.GetFamilyName() == name ) {
					return family.Value;
				}
			}
			return null;
		}
		public static Family GetFamily( Settlement settlement ) {
			foreach ( var family in Instance.Families ) {
				if ( family.Value.CanAddMember() ) {
					return family.Value;
				}
			}
			Family instance = Family.Create( settlement );
			Instance.Families.Add( instance.GetHashCode(), instance );
			Instance.GetTree().Root.GetNode( "World/FamilyTrees" ).CallDeferred( "add_child", instance );
			System.Threading.Interlocked.Increment( ref FamilyCount );
			return instance;
		}
	};
};