/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using Steam;
using Steamworks;
using Menus;

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
		if ( SteamLobby.Instance.IsHost ) {
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
				return; // can't kick yourselfs
			}
			LobbyRoom.Instance.KickPlayer( UserID );
		} ) );
	}
};