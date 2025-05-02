using Godot;

public partial class MultiplayerMenu : Control {
	private Control LobbyBrowser;
	private Control LobbyFactory;

	private void OnLobbyBrowserHostGamePressed() {
		LobbyBrowser.Hide();
		LobbyFactory.Show();
	}

	public override void _Ready() {
		LobbyBrowser = GetNode<Control>( "LobbyBrowser" );
		LobbyBrowser.Show();
		LobbyBrowser.Connect( "OnHostGame", Callable.From( OnLobbyBrowserHostGamePressed ) );

		LobbyFactory = GetNode<Control>( "LobbyFactory" );
		LobbyFactory.Hide();
	}
}
