using Godot;

public partial class MultiplayerMenu : Control {
	private Control LobbyBrowser;
	private Control LobbyFactory;

	private void OnLobbyBrowserHostGamePressed() {
		LobbyBrowser.Hide();
		LobbyBrowser.SetProcess( false );
		LobbyBrowser.SetProcessInternal( false );

		LobbyFactory.Show();
		LobbyFactory.SetProcess( false );
		LobbyFactory.SetProcessInternal( false );
	}

	public override void _Ready() {
		LobbyBrowser = GetNode<Control>( "LobbyBrowser" );
		LobbyBrowser.Connect( "OnHostGame", Callable.From( OnLobbyBrowserHostGamePressed ) );

		LobbyFactory = GetNode<Control>( "LobbyFactory" );
	}
}
