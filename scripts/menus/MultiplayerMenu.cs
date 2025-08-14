using Godot;

public partial class MultiplayerMenu : Control {
	private LobbyBrowser LobbyBrowser;
	private LobbyFactory LobbyFactory;
	private ProfileMenu ProfileMenu;

	private void OnLobbyBrowserHostGamePressed() {
		LobbyBrowser.Hide();
		LobbyFactory.Show();
	}

	private void OnQuickmatchButtonPressed() {
	}
	private void OnProfileButtonPressed() {
	}

	public override void _Ready() {
		Button QuickmatchButton = GetNode<Button>( "VBoxContainer/QuickmatchButton" );
		QuickmatchButton.Connect( Button.SignalName.FocusEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		QuickmatchButton.Connect( Button.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		QuickmatchButton.Connect( Button.SignalName.Pressed, Callable.From( OnQuickmatchButtonPressed ) );

		Button ProfileButton = GetNode<Button>( "VBoxContainer/ProfileButton" );
		ProfileButton.Connect( Button.SignalName.FocusEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		ProfileButton.Connect( Button.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
		ProfileButton.Connect( Button.SignalName.Pressed, Callable.From( OnProfileButtonPressed ) );

		LobbyBrowser = GetNode<LobbyBrowser>( "LobbyBrowser" );
		LobbyBrowser.Show();
		LobbyBrowser.Connect( "OnHostGame", Callable.From( OnLobbyBrowserHostGamePressed ) );

		LobbyFactory = GetNode<LobbyFactory>( "LobbyFactory" );
		LobbyFactory.Hide();
	}
}
