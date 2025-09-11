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

using System;
using Godot;
using Multiplayer;
using Multiplayer.Modes;
using System.Threading;
using System.Linq;
using Steam;

namespace Menus {
	/*
	===================================================================================
	
	LobbyFactory
	
	===================================================================================
	*/
	
	public partial class LobbyFactory : Control {
		private LineEdit LobbyName;
		private HSlider MaxPlayers;
		private OptionButton MapList;
		private OptionButton GameModeList;
		private Label PlayerCountLabel;

		private void OnMapSelectionChanged( int selected ) {
			MultiplayerMapManager.MapData data = MultiplayerMapManager.MapCache.Values.ElementAt( selected );

			GameModeList.Clear();
			if ( data.ModeBloodbath ) {
				GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.Bloodbath ], (int)Mode.GameMode.Bloodbath );
			}
			if ( data.ModeCaptureTheFlag ) {
				GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.CaptureTheFlag ], (int)Mode.GameMode.CaptureTheFlag );
			}
			if ( data.ModeKingOfTheHill ) {
				GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.KingOfTheHill ], (int)Mode.GameMode.KingOfTheHill );
			}
			if ( data.ModeDuel ) {
				GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.Duel ], (int)Mode.GameMode.Duel );
			}
			if ( data.ModeExtraction ) {
				GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.Extraction ], (int)Mode.GameMode.Extraction );
			}
			if ( data.ModeBountyHunt ) {
				GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.BountyHunt ], (int)Mode.GameMode.BountyHunt );
			}
			if ( data.ModeHoldTheLine ) {
				GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.HoldTheLine ], (int)Mode.GameMode.HoldTheLine );
			}
			GameModeList.Selected = 0;
		}
		private void OnGameModeSelectionChanged( int selected ) {
			switch ( (Mode.GameMode)selected ) {
				case Mode.GameMode.Bloodbath:
					MaxPlayers.MinValue = Bloodbath.MIN_PLAYERS;
					MaxPlayers.MaxValue = Bloodbath.MAX_PLAYERS;
					break;
				case Mode.GameMode.CaptureTheFlag:
					break;
				case Mode.GameMode.KingOfTheHill:
					break;
				case Mode.GameMode.Duel:
					MaxPlayers.MinValue = Duel.MIN_PLAYERS;
					MaxPlayers.MaxValue = Duel.MAX_PLAYERS;
					break;
			}
		}

		public override void _Ready() {
			Theme = SettingsData.DyslexiaMode ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

			LobbyName = GetNode<LineEdit>( "MarginContainer/VBoxContainer/NameContainer/NameLineEdit" );

			MaxPlayers = GetNode<HSlider>( "MarginContainer/VBoxContainer/MaxPlayersContainer/HBoxContainer/MaxPlayersHSlider" );
			GameEventBus.ConnectSignal( MaxPlayers, HSlider.SignalName.ValueChanged, this, Callable.From<double>( ( value ) => { PlayerCountLabel.Text = Convert.ToString( (int)value ); } ) );

			PlayerCountLabel = GetNode<Label>( "MarginContainer/VBoxContainer/MaxPlayersContainer/HBoxContainer/PlayerCountLabel" );
			PlayerCountLabel.Text = Convert.ToString( MaxPlayers.Value );

			MapList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/MapContainer/MapOptionButton" );
			GameEventBus.ConnectSignal(  MapList, OptionButton.SignalName.ItemSelected, this, Callable.From<int>( OnMapSelectionChanged ) );

			GameModeList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/GameModeContainer/GameModeOptionButton" );

			GameEventBus.ConnectSignal( GetNode( "CreateButton" ), Button.SignalName.Pressed, this, OnCreateButtonPressed );

			foreach ( var map in MultiplayerMapManager.MapCache ) {
				MapList.AddItem( map.Value.Name );
			}
			for ( int i = 0; i < (int)Mode.GameMode.Count; i++ ) {
				GameModeList.AddItem( Mode.ModeNames[ (Mode.GameMode)i ] );
			}
		}

		private void OnCreateButtonPressed() {
			Console.PrintLine( "Creating lobby..." );

			string name = LobbyName.Text;
			if ( LobbyName.Text.Length == 0 ) {
				string username = SteamManager.GetSteamName();
				if ( username[ username.Length - 1 ] == 's' ) {
					name = string.Format( "{0}' Lobby", username );
				} else {
					name = string.Format( "{0}'s Lobby", username );
				}
			}

			SteamLobby.Instance.SetLobbyName( LobbyName.Text );
			SteamLobby.Instance.SetMaxMembers( (int)MaxPlayers.Value );
			SteamLobby.Instance.SetMap( MultiplayerMapManager.MapCache.Values.ElementAt( MapList.Selected ).Name );
			SteamLobby.Instance.SetGameMode( (uint)GameModeList.GetItemId( GameModeList.Selected ) );
			SteamLobby.Instance.SetHostStatus( true );
			GameConfiguration.SetGameMode( GameMode.Multiplayer );

			SteamLobby.Instance.CreateLobby();

			Console.PrintLine( string.Format( "Starting game [map: {0}, gamemode: {1}]...", MultiplayerMapManager.MapCache.Values.ElementAt( MapList.Selected ).Name, GameModeList.Selected ) );

			Hide();
			GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( "res://scenes/multiplayer/lobby_room.tscn" );
		}
	};
};