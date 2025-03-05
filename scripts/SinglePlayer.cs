using System.Threading.Tasks;
using Godot;

public partial class SinglePlayer : Node2D {
	[Export]
	private Player Player1 = null;
	private Node2D Hellbreaker = null;
	private Node2D SettingsData = null;

	[Export]
	public Node2D LevelData = null;
	
	public void ToggleHellbreaker() {
		LevelData.Hide();
		LevelData.SetProcess( false );
		LevelData.SetProcessInput( false );
		LevelData.SetProcessInternal( false );
		LevelData.SetPhysicsProcess( false );
		LevelData.SetProcessUnhandledInput( false );

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

		MobSfxCache.Instance.Cache();
		Control SettingsData = GetNode<Control>( "/root/SettingsData" );
		if ( (bool)SettingsData.Get( "_hellbreaker" ) ) {
			Hellbreaker = ResourceLoader.Load<PackedScene>( "res://levels/hellbreaker.tscn" ).Instantiate<Node2D>();

			Hellbreaker.Hide();
			Hellbreaker.SetProcess( false );
			Hellbreaker.SetProcessInput( false );
			Hellbreaker.SetProcessInternal( false );
			Hellbreaker.SetPhysicsProcess( false );
			Hellbreaker.SetProcessUnhandledInput( false );
		}
	}
};