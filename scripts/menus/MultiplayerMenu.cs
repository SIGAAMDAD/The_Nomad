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
using Multiplayer;
using ResourceCache;

namespace Menus {
	/*
	===================================================================================

	MultiplayerMenu

	===================================================================================
	*/

	public partial class MultiplayerMenu : Control {
		private LobbyBrowser LobbyBrowser;
		private LobbyFactory LobbyFactory;

		/*
		================
		OnLobbyBrowserHostGamePressed
		===============
		*/
		private void OnLobbyBrowserHostGamePressed() {
			LobbyBrowser.Hide();
			LobbyFactory.Show();
		}

		/*
		================
		OnQuickmatchButtonPressed
		===============
		*/
		private void OnQuickmatchButtonPressed() {
		}

		/*
		================
		InitProfile
		===============
		*/
		private void InitProfile() {
			Label Name = GetNode<Label>( "HBoxContainer/ProfileContainer/NameLabel" );
			Name.Text = Profile.Instance.Name;

			Label SkillPoints = GetNode<Label>( "HBoxContainer/ProfileContainer/SkillPointsLabel" );
			SkillPoints.Text = string.Format( "NOTORIETY: {0}", Profile.Instance.SkillPoints );

			Resource PrimaryWeapon = ItemCache.GetItem( Profile.Instance.Loadout.PrimaryWeaponId );
			Resource SecondaryWeapon = ItemCache.GetItem( Profile.Instance.Loadout.SecondaryWeaponId );

			TextureRect PrimaryWeaponIcon = GetNode<TextureRect>( "HBoxContainer/ProfileContainer/PrimaryWeaponIcon" );
			PrimaryWeaponIcon.Texture = PrimaryWeapon.Get( "icon" ).AsGodotObject() as Texture2D;

			TextureRect SecondaryWeaponIcon = GetNode<TextureRect>( "HBoxContainer/ProfileContainer/SecondaryWeaponIcon" );
			SecondaryWeaponIcon.Texture = SecondaryWeapon.Get( "icon" ).AsGodotObject() as Texture2D;
		}

		/*
		================
		_Ready

		godot initialization override
		===============
		*/
		public override void _Ready() {
			Profile.Load();

			Button QuickmatchButton = GetNode<Button>( "HBoxContainer/VBoxContainer/QuickmatchButton" );
			QuickmatchButton.Connect( Button.SignalName.FocusEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
			QuickmatchButton.Connect( Button.SignalName.MouseEntered, Callable.From( UIAudioManager.OnButtonFocused ) );
			QuickmatchButton.Connect( Button.SignalName.Pressed, Callable.From( OnQuickmatchButtonPressed ) );

			LobbyBrowser = GetNode<LobbyBrowser>( "LobbyBrowser" );
			LobbyBrowser.Hide();
			LobbyBrowser.Connect( LobbyBrowser.SignalName.OnHostGame, Callable.From( OnLobbyBrowserHostGamePressed ) );

			LobbyFactory = GetNode<LobbyFactory>( "LobbyFactory" );
			LobbyFactory.Hide();

			InitProfile();
		}
	};
};