using Godot;
using Steamworks;

public partial class LobbyRoom : Control {
	private VBoxContainer PlayerList;
	private Button StartGameButton;
	private Button ExitLobbyButton;

	private void OnStartGameButtonPressed() {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return;
		}

		SteamLobby.Instance.SendP2PPacket( CSteamID.Nil, new byte[]{ (byte)SteamLobby.MessageType.StartGame } );
	}
	private void OnExitLobbyButtonPressed() {
	}

	public override void _Ready() {
		base._Ready();

		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		PlayerList = GetNode<VBoxContainer>( "MarginContainer/PlayerList" );

		StartGameButton = GetNode<Button>( "StartGameButton" );
		StartGameButton.Connect( "pressed", Callable.From( OnStartGameButtonPressed ) );

		ExitLobbyButton = GetNode<Button>( "ExitLobbyButton" );
		ExitLobbyButton.Connect( "pressed", Callable.From( OnExitLobbyButtonPressed ) );
	}
};