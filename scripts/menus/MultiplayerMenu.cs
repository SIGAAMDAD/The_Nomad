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
using System;

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
			Label name = GetNode<Label>( "HBoxContainer/ProfileContainer/NameLabel" );
			name.Text = Profile.Instance.Name;

			Label skillPoints = GetNode<Label>( "HBoxContainer/ProfileContainer/SkillPointsLabel" );
			skillPoints.Text = string.Format( "NOTORIETY: {0}", Profile.Instance.SkillPoints );

			Resource? primaryWeapon = ItemCache.GetItem( Profile.Instance.Loadout.PrimaryWeaponId );
			ArgumentNullException.ThrowIfNull( primaryWeapon );

			Resource? secondaryWeapon = ItemCache.GetItem( Profile.Instance.Loadout.SecondaryWeaponId );
			ArgumentNullException.ThrowIfNull( secondaryWeapon );

			TextureRect primaryWeaponIcon = GetNode<TextureRect>( "HBoxContainer/ProfileContainer/PrimaryWeaponIcon" );
			primaryWeaponIcon.Texture = primaryWeapon.Get( "icon" ).AsGodotObject() as Texture2D;

			TextureRect secondaryWeaponIcon = GetNode<TextureRect>( "HBoxContainer/ProfileContainer/SecondaryWeaponIcon" );
			secondaryWeaponIcon.Texture = secondaryWeapon.Get( "icon" ).AsGodotObject() as Texture2D;
		}

		/*
		================
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			Profile.Load();

			SelectionNodes.Button quickmatchButton = GetNode<SelectionNodes.Button>( "HBoxContainer/VBoxContainer/QuickmatchButton" );
			quickmatchButton.Pressed += OnQuickmatchButtonPressed;

			LobbyBrowser = GetNode<LobbyBrowser>( "LobbyBrowser" );
			LobbyBrowser.Hide();
			GameEventBus.ConnectSignal( LobbyBrowser, LobbyBrowser.SignalName.OnHostGame, this, OnLobbyBrowserHostGamePressed );

			LobbyFactory = GetNode<LobbyFactory>( "LobbyFactory" );
			LobbyFactory.Hide();

			InitProfile();
		}
	};
};