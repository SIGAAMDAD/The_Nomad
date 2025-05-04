using System.Collections.Generic;
using Godot;

public partial class Hellbreaker : Node2D {
	private List<HellbreakerExit> Exits;

	public override void _Ready() {
		base._Ready();

		Exits = new List<HellbreakerExit>();

		Godot.Collections.Array<Node> exits = GetTree().GetNodesInGroup( "HellbreakerExits" );
		for ( int i = 0; i < exits.Count; i++ ) {
			Exits.Add( exits[i] as HellbreakerExit );
		}
	}
};
