using Godot;

public partial class MultiplayerMenu : Control {
	private Control LobbyBrowser;
	private Control LobbyFactory;

	private void OnLobbyBrowserHostGamePressed() {
		LobbyBrowser.Hide();
		LobbyFactory.Show();
	}

	public override void _Ready() {
		LobbyBrowser = GetNode<LobbyBrowser>( "LobbyBrowser" );
		LobbyBrowser.Show();
		LobbyBrowser.Connect( "OnHostGame", Callable.From( OnLobbyBrowserHostGamePressed ) );

		LobbyFactory = GetNode<LobbyFactory>( "LobbyFactory" );
		LobbyFactory.Hide();
	}
}
