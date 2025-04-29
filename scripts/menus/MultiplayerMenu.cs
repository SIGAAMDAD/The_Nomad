using Godot;

public partial class MultiplayerMenu : Control {
	private Control LobbyBrowser;
	private Control LobbyFactory;

	private void OnLobbyBrowserHostGamePressed() {
		LobbyBrowser.Hide();
		LobbyBrowser.ProcessMode = ProcessModeEnum.Disabled;

		LobbyFactory.Show();
		LobbyFactory.ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Ready() {
		LobbyBrowser = GetNode<Control>( "LobbyBrowser" );
		LobbyBrowser.ProcessMode = ProcessModeEnum.Always;
		LobbyBrowser.Show();
		LobbyBrowser.Connect( "OnHostGame", Callable.From( OnLobbyBrowserHostGamePressed ) );

		LobbyFactory = GetNode<Control>( "LobbyFactory" );
		LobbyBrowser.ProcessMode = ProcessModeEnum.Disabled;
		LobbyFactory.Hide();
	}
}
