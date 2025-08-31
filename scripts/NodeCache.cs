using Godot;
using System.Collections.Generic;
using System.Dynamic;

/// <summary>
/// a singleton to both save and load nodes created during runtime
/// </summary>
public partial class NodeCache : Node {
	private Dictionary<NodePath, Node> Nodes;
	private Dictionary<string, Node> Groups;

	private static NodeCache Instance;

	private void Load() {
		using var reader = ArchiveSystem.GetSection( "NodeCache" );

		int count = reader.LoadInt( "Count" );
		Nodes = new Dictionary<NodePath, Node>( count );

		for ( int i = 0; i < count; i++ ) {
			string path = reader.LoadString( string.Format( "CachedNodePath{0}", i ) );
			if ( Nodes.ContainsKey( path ) ) {
				Console.PrintError( string.Format( "NodeCache.Load: duplicate key found in save data \"{0}\"", path ) );
			} else {
				string group = reader.LoadString( string.Format( "CachedNodeGroup{0}", i ) );
			}
		}
	}
	public override void _Ready() {
		base._Ready();

		Instance = this;

		Godot.Collections.Array<Node> groups = GetTree().GetNodesInGroup( "CachedGroup" );
		Groups = new Dictionary<string, Node>( groups.Count );
		for ( int i = 0; i < groups.Count; i++ ) {
			Groups.Add( groups[ i ].Name, groups[ i ] );
		}

		if ( ArchiveSystem.IsLoaded() ) {
			Load();
		} else {
		}
	}

	public static void AddNode( Node node ) {
		if ( node.IsInGroup( "Thinkers" ) ) {
		} else if ( node.IsInGroup( "ItemPickups" ) ) {

		} else if ( node.IsInGroup( "Families" ) ) {

		}
	}
};