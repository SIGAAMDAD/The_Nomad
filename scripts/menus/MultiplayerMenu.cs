using Godot;

public partial class MultiplayerMenu : Control {
	private Control LobbyBrowser;
	private Control LobbyFactory;

	private void OnLobbyBrowserHostGamePressed() {
		LobbyBrowser.Hide();
		LobbyFactory.Show();
	}

	public override void _Ready() {
		LobbyBrowser = ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/lobby_browser.tscn" ).Instantiate<LobbyBrowser>();
		AddChild( LobbyBrowser );
		LobbyBrowser.Show();
		LobbyBrowser.Connect( "OnHostGame", Callable.From( OnLobbyBrowserHostGamePressed ) );

		LobbyFactory = ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/lobby_factory.tscn" ).Instantiate<LobbyFactory>();
		AddChild( LobbyFactory );
		LobbyFactory.Hide();
	}
}
