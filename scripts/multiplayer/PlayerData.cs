using Godot;
using Steamworks;

public partial class PlayerData : HBoxContainer {
	public Label NameLabel;
	public Button KickButton;
	public Button BanButton;
	private CSteamID UserID;

	public void SetUserID( CSteamID playerId ) {
		UserID = playerId;
	}

	public override void _Ready() {
		base._Ready();

		NameLabel = GetNode<Label>( "NameLabel" );
		NameLabel.Text = SteamFriends.GetFriendPersonaName( UserID );
		if ( SteamLobby.Instance.IsOwner() ) {
			NameLabel.Connect( "gui_input", Callable.From<InputEvent>( ( @event ) => {
				if ( @event is InputEventMouseButton mouseButton && mouseButton != null ) {
					if ( mouseButton.ButtonIndex == MouseButton.Left ) {
						UIAudioManager.OnButtonPressed();
						LobbyRoom.Instance.FocusPlayer( this );
					}
				}
			} ) );
		}

		KickButton = GetNode<Button>( "KickButton" );
		KickButton.Connect( "pressed", Callable.From( () => {
			if ( UserID == SteamManager.GetSteamID() ) {
				return; // can't kick yourself
			}
			LobbyRoom.Instance.KickPlayer( UserID );
		} ) );
	}
};