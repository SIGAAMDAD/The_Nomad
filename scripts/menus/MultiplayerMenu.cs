using Godot;
using System;

public partial class MultiplayerMenu : Control {
	private Control LobbyBrowser;
	private Control LobbyFactory;

	private void OnLobbyBrowserHostGamePressed() {
		LobbyBrowser.Hide();
		LobbyFactory.Show();
	}

	public override void _Ready() {
		LobbyBrowser = GetNode<Control>( "LobbyBrowser" );
		LobbyBrowser.Connect( "OnHostGame", Callable.From( OnLobbyBrowserHostGamePressed ) );

		LobbyFactory = GetNode<Control>( "LobbyFactory" );
	}
}
