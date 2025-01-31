using Godot;
using System;

public partial class SinglePlayer : Node2D {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Node ArchiveSystem = GetNode( "/root/ArchiveSystem" );

		PackedScene level = GD.Load<PackedScene>( "res://scenes/level"
			+ Convert.ToString( (int)ArchiveSystem.Call( "GetPart" ) )
			+ Convert.ToString( (int)ArchiveSystem.Call( "GetChapter" ) )
			+ ".tscn" );
		Node child = level.Instantiate();
		AddChild( child );

		Player player1 = child.GetChild<Player>( 3 );
		Player player2 = child.GetChild<Player>( 4 );

		player2.Hide();
		if ( Input.GetConnectedJoypads().Count > 0 ) {
			player1.SetupSplitScreen( 0 );
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta ) {
	}
}
