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
using System.Linq;
using Multiplayer;

namespace Menus {
	/*
	===================================================================================

	CoopMenu

	===================================================================================
	*/

	public partial class CoopMenu : Control {
		private OptionButton MapList;
		private OptionButton GameModeList;

		private AudioStreamPlayer UIChannel;

		private bool Loaded = false;

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

		private void OnTransitionFinished() {
			Hide();

			Console.PrintLine( "Loading game..." );
			string modeName = (Mode.GameMode)GameModeList.GetItemId( GameModeList.Selected ) switch {
				Mode.GameMode.Duel => "duel",
				Mode.GameMode.Bloodbath => "ffa",
				Mode.GameMode.CaptureTheFlag => "ctf",
				Mode.GameMode.KingOfTheHill => "koth",
				Mode.GameMode.Extraction => "extraction",
				Mode.GameMode.BountyHunt => "bh",
				_ => "",
			};
			string mapName = "res://levels/" + MultiplayerMapManager.MapCache.Values.ElementAt( MapList.Selected ).FileName + "_" + modeName + "_2p.tscn";
			GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( mapName );
		}
		private void OnStartButtonPressed() {
			if ( Loaded ) {
				return;
			}
			Loaded = true;

			UIAudioManager.OnActivate();

			TransitionScreen.TransitionFinished += OnTransitionFinished;
			TransitionScreen.Transition();
			/*
			TODO: split screen with online
			string name = LobbyName.Text;
			if ( LobbyName.Text.Length == 0 ) {
				string username = SteamManager.GetSteamName();
				if ( username[ username.Length - 1 ] == 's' ) {
					name = string.Format( "{0}' Lobby", username );
				} else {
					name = string.Format( "{0}'s Lobby", username );
				}
			}
			*/

			GameConfiguration.SetGameMode( GameMode.LocalCoop2 );

			Console.PrintLine( string.Format( "Starting game [map: {0}, gamemode: {1}]...", MultiplayerMapManager.MapCache.Values.ElementAt( MapList.Selected ).Name, GameModeList.Selected ) );
		}

		public override void _Ready() {
			base._Ready();

			MapList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/MapContainer/MapOptionButton" );
			GameEventBus.ConnectSignal( MapList, OptionButton.SignalName.ItemSelected, this, Callable.From<int>( OnMapSelectionChanged ) );

			GameModeList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/GameModeContainer/GameModeOptionButton" );

			UIChannel = GetNode<AudioStreamPlayer>( "../../UIChannel" );

			GameEventBus.ConnectSignal( GetNode<Button>( "StartButton" ), Button.SignalName.Pressed, this, OnStartButtonPressed );

			foreach ( var map in MultiplayerMapManager.MapCache ) {
				MapList.AddItem( map.Value.Name );
			}
			for ( int i = 0; i < (int)Mode.GameMode.Count; i++ ) {
				GameModeList.AddItem( Mode.ModeNames[ (Mode.GameMode)i ] );
			}
		}
	};
};