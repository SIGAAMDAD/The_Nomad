using Godot;

// TODO: support more than 2 players

public partial class SplitScreenCoop : Node2D {
	private SubViewport Viewport1;
	private SubViewport Viewport2;

	private Player Player1;
	private Player Player2;

	private System.Threading.Thread ResourceLoadThread;
	private System.Threading.Thread SceneLoadThread;

	[Signal]
	public delegate void ResourcesLoadingFinishedEventHandler();

	private void OnResourcesFinishedLoading() {
		SetProcess( true );

		ResourceLoadThread.Join();

		ResourceCache.Initialized = true;

		if ( SettingsData.GetNetworkingEnabled() ) {
			SteamLobby.Instance.SetProcess( true );
			SteamLobby.Instance.SetPhysicsProcess( true );
		}

		Console.PrintLine( "...Finished loading game" );
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
	}

	public override void _Ready() {
		GetTree().CurrentScene = this;

		Viewport1 = GetNode<SubViewport>( "ScreenData/Player1/SubViewport" );
		Viewport2 = GetNode<SubViewport>( "ScreenData/Player2/SubViewport" );

		Player1 = GetNode<Player>( "ScreenData/Player1/SubViewport/Network/Player1" );
		Player1.SetupSplitScreen( 0 );
		Player1.SwitchInputMode( ResourceCache.GamepadInputMappings );
		Player1.RemoveChild( Player1.GetNode<Camera2D>( "Camera2D" ) );

		Player2 = GetNode<Player>( "ScreenData/Player1/SubViewport/Network/Player2" );
		Player2.SetupSplitScreen( 1 );
		Player2.SwitchInputMode( ResourceCache.GamepadInputMappings );
		Player2.RemoveChild( Player2.GetNode<Camera2D>( "Camera2D" ) );

		Viewport2.World2D = Viewport1.World2D;
		
		RemoteTransform2D transform = new RemoteTransform2D();
		transform.RemotePath = Viewport1.GetNode<Camera2D>( "Camera2D" ).GetPath();
		Player1.AddChild( transform );

		RemoteTransform2D transform2 = new RemoteTransform2D();
		transform2.RemotePath = Viewport2.GetNode<Camera2D>( "Camera2D" ).GetPath();

		Viewport1.GetNode<Camera2D>( "Camera2D" ).MakeCurrent();
		Player2.AddChild( transform2 );

		ResourceLoadThread = new System.Threading.Thread( () => { ResourceCache.Cache( this, null ); } );
		ResourceLoadThread.Start();

		ResourcesLoadingFinished += OnResourcesFinishedLoading;

		PhysicsServer2D.SetActive( true );

		SetProcess( false );
		SetProcessInternal( false );
	}
};
