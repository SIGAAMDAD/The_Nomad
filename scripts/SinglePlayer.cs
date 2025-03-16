using Godot;

public partial class SinglePlayer : Node2D {
	[Export]
	private Player Player1 = null;
	private Node2D Hellbreaker = null;
	private Node2D SettingsData = null;

	[Export]
	public Node2D LevelData = null;

//	public ThinkerPoolManager GetThinkerPool() {
//		return ThinkerPool;
//	}
	
	public void ToggleHellbreaker() {
		LevelData.Hide();
		LevelData.SetProcess( false );
		LevelData.SetProcessInput( false );
		LevelData.SetProcessInternal( false );
		LevelData.SetPhysicsProcess( false );
		LevelData.SetProcessUnhandledInput( false );

		Hellbreaker = ResourceLoader.Load<PackedScene>( "res://levels/hellbreaker" ).Instantiate<Node2D>();
		Hellbreaker.Show();
		Hellbreaker.SetProcess( true );
		Hellbreaker.SetProcessInput( true );
		Hellbreaker.SetProcessInternal( true );
		Hellbreaker.SetPhysicsProcess( true );
		Hellbreaker.SetProcessUnhandledInput( true );
		
		AddChild( Hellbreaker );
	}

	public override void _ExitTree() {
		Player1.QueueFree();
		if ( Hellbreaker != null ) {
			Hellbreaker.QueueFree();
		}
	}

	public override void _Ready() {
		GetTree().CurrentScene = this;
		
		if ( Input.GetConnectedJoypads().Count > 0 ) {
			Player1.SetupSplitScreen( 0 );
		}

//		_ = new MountainGoapLogging.DefaultLogger(
//			true
//			"goap.log"
//		);
	}
};