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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using Multiplayer;
using Steamworks;
using Steam;
using System;

namespace Menus {
	/*
	===================================================================================
	
	LobbyBrowser
	
	===================================================================================
	*/
	
	public partial class LobbyBrowser : Control {
		private partial class LobbyData : Button {
			private CSteamID LobbyId;

			private Mode.GameMode GameMode = Mode.GameMode.Bloodbath;
			private GameMode GameType = global::GameMode.Multiplayer;
			private string MapName = "";
			private string LobbyName = "";

			public LobbyData( CSteamID lobbyId ) {
				LobbyId = lobbyId;
				Name = LobbyId.ToString();

				Size = new Godot.Vector2( 240, 20 );
				CustomMinimumSize = Size;
				Pressed += () => Instance.OnLobbySelected( lobbyId );

				LoadMetadata();
			}

			public bool IsValid() {
				Refresh();

				if ( !MultiplayerMapManager.MapCache.ContainsKey( MapName ) ) {
					return false;
				}
				if ( GameMode == Mode.GameMode.Count ) {
					return false;
				}
				if ( GameType != global::GameMode.Multiplayer && GameType != global::GameMode.Online ) {
					return false;
				}
				return true;
			}

			private void LoadMetadata() {
				LobbyName = SteamMatchmaking.GetLobbyData( LobbyId, "name" );
				Text = LobbyName;

				string gameMode = SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" );
				if ( gameMode.IsValidInt() ) {
					GameMode = (Mode.GameMode)System.Convert.ToUInt32( gameMode );
				} else {
					GameMode = Mode.GameMode.Count;
				}
				GameType = global::GameMode.Multiplayer;
				MapName = SteamMatchmaking.GetLobbyData( LobbyId, "map" );
			}
			public bool Refresh() {
				LoadMetadata();
				return true;
			}
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public int GetMaxMembers() => SteamMatchmaking.GetLobbyMemberLimit( LobbyId );
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public int GetNumMembers() => SteamMatchmaking.GetNumLobbyMembers( LobbyId );
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public GameMode GetGameType() => GameType;
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public Mode.GameMode GetGameMode() => GameMode;
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public string GetMapName() => MapName;
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public string GetMapFileName() => MultiplayerMapManager.MapCache[ MapName ].FileName;
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public string GetLobbyName() => LobbyName;
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			public CSteamID GetLobbyID() => LobbyId;
		};

		private Dictionary<CSteamID, LobbyData> LobbyList = new Dictionary<CSteamID, LobbyData>();
		private Dictionary<int, bool> MapFilterList = new Dictionary<int, bool>();

		private Callable LobbyJoinedCallback;
		private Callable LobbyConnectionStatusChangedCallback;
		private Callable LobbyListUpdatedCallback;

		private Button HostGame;
		private Button RefreshLobbies;
		private Button Matchmake;
		private Button CancelMatchmake;

		private HBoxContainer JoinGame;
		private HBoxContainer LobbyManager;

		private CSteamID SelectedLobby = CSteamID.Nil;

		private Label MapNameLabel;
		private Label PlayerCountLabel;
		private Label GameModeLabel;

		private Label JoiningLobbyLabel;
		private Godot.Range JoiningLobbySpinner;

		private HBoxContainer JoiningLobbyContainer;

		private Control MatchmakingSpinner;
		private Label MatchmakingLabel;
		private Timer MatchmakingTimer;

		private VBoxContainer LobbyTable;

		private CheckBox ShowFullServers;
		private CheckBox GameFilter_Bloodbath;
		private CheckBox GameFilter_KingOfTheHill;
		private CheckBox GameFilter_CaptureTheFlag;
		private CheckBox GameFilter_Duel;

		private int MatchmakingPhase = 0;

		private System.Threading.Thread MatchmakingThread = null;
		private object MatchmakingLobbyListReady = new object();

		private string LoadedScenePath = "";

		public static LobbyBrowser Instance;

		[Signal]
		public delegate void OnHostGameEventHandler();
		[Signal]
		public delegate void MatchmakingFinishedEventHandler();

		private void OnFinishedLoading() {
			GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeOut();
			Console.PrintLine( "...Finished loading game" );
		}
		public void OnLobbyConnected() {
			/*
			if ( SettingsData.GetNetworkingEnabled() ) {
				Console.PrintLine( "Networking enabled, creating co-op lobby..." );

				GameConfiguration.GameMode = GameMode.Online;

				SteamLobby.Instance.SetMaxMembers( 4 );
				string name = SteamManager.GetSteamName();
				if ( name[ name.Length - 1 ] == 's' ) {
					SteamLobby.Instance.SetLobbyName( string.Format( "{0}' Lobby", name ) );
				} else {
					SteamLobby.Instance.SetLobbyName( string.Format( "{0}'s Lobby", name ) );
				}

				SteamLobby.Instance.CreateLobby();
			} else {
				GameConfiguration.GameMode = GameMode.SinglePlayer;
			}
			*/

			if ( SteamLobby.Instance.IsHost ) {
				return;
			}

			string gameType = SteamMatchmaking.GetLobbyData( SteamLobby.Instance.LobbyId, "gametype" );
			Console.PrintLine( string.Format( "Loading game [{0}]...", gameType ) );

			switch ( gameType ) {
				case "Multiplayer":
					// loading a multiplayer game instead a co-op world
					GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( "res://scenes/multiplayer/lobby_room.tscn" );
					Console.PrintLine( "...Finished loading game" );
					break;
			}

			ServerCommandManager.SendCommand( ServerCommandType.ConnectedToLobby );
		}
		private static void OnLobbyJoined( ulong lobbyId ) {
		}

		private void OnConnectionStatusChanged( int status ) {
			switch ( (ESteamNetworkingConnectionState)status ) {
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					UIAudioManager.FadeMusic();

					JoiningLobbyLabel.Text = "JOINED";
					JoiningLobbySpinner.Set( "status", 3 );
					OnLobbyConnected();
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
					JoiningLobbyLabel.Text = "CONNECTING...";
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Dead:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
					JoiningLobbyLabel.Text = "FAILED TO CONNECT";
					JoiningLobbySpinner.Set( "status", 5 );
					break;
			}
		}

		private void MatchmakingLoop() {
			// apply filters
			SteamMatchmaking.AddRequestLobbyListStringFilter( "gametype", "", ELobbyComparison.k_ELobbyComparisonEqual );

			while ( MatchmakingPhase < 4 ) {
				lock ( MatchmakingLobbyListReady ) {
					System.Threading.Monitor.Wait( MatchmakingLobbyListReady );
				}

				MatchmakingPhase++;
			}
			Console.PrintLine( "...no open contracts found" );
		}
		private void OnJoinGame( CSteamID lobbyId ) {
			LobbyData data = LobbyList[ lobbyId ];

			// sanity check
			if ( !data.IsValid() ) {
				return;
			}

			JoinGame.Hide();

			JoiningLobbyContainer.Show();
			JoiningLobbyLabel.Text = "CONNECTING...";

			UIAudioManager.OnActivate();

			Console.PrintLine( string.Format( "Joining lobby {0}...", lobbyId.ToString() ) );
			SteamLobby.Instance.JoinLobby( lobbyId );
		}
		private void OnLobbySelected( CSteamID lobbyId ) {
			UIAudioManager.OnButtonPressed();

			JoiningLobbyContainer.Hide();

			LobbyData lobby = LobbyList[ lobbyId ];
			lobby.Refresh();

			LobbyManager.Hide();
			JoinGame.Show();

			SelectedLobby = lobbyId;
			PlayerCountLabel.Text = lobby.GetNumMembers() + "/" + lobby.GetMaxMembers();

			switch ( lobby.GetGameType() ) {
				case GameMode.Online:
					GameModeLabel.Text = "Cooperative (LOCAL WORLD)";
					MapNameLabel.Text = "The Fever Dream";
					break;
				case GameMode.Multiplayer: {
						Mode.GameMode mode = lobby.GetGameMode();
						GameModeLabel.Text = Mode.ModeNames[ mode ];

						MapNameLabel.Text = lobby.GetMapName();
						break;
					}
			}
		}

		private void GetLobbyList() {
			List<CSteamID> lobbyList = SteamLobby.Instance.LobbyList;

			for ( int i = 0; i < lobbyList.Count; i++ ) {
				//
				// filter 'em
				//
				string gametype = SteamMatchmaking.GetLobbyData( lobbyList[ i ], "gamemode" );
				int memberCount = SteamMatchmaking.GetNumLobbyMembers( lobbyList[ i ] );

				if ( memberCount == SteamMatchmaking.GetLobbyMemberLimit( lobbyList[ i ] ) && !ShowFullServers.ButtonPressed ) {
					continue;
				}

				for ( int j = 0; j < LobbyTable.GetChildCount(); j++ ) {
					if ( LobbyList.TryGetValue( lobbyList[ i ], out LobbyData lobby ) ) {
						if ( LobbyTable.FindChild( lobby.Name ) != null ) {
							LobbyTable.RemoveChild( lobby );
						}
						LobbyList.Remove( lobbyList[ i ] );
						lobby.QueueFree();
					}
				}
				LobbyData data = new LobbyData( lobbyList[ i ] );
				LobbyList.TryAdd( lobbyList[ i ], data );
				LobbyTable.CallDeferred( MethodName.AddChild, data );
			}

			if ( MatchmakingThread.IsAlive ) {
				lock ( MatchmakingLobbyListReady ) {
					System.Threading.Monitor.Pulse( MatchmakingLobbyListReady );
				}
			}
		}
		private void OnRefreshButtonPressed() {
			if ( GameFilter_Bloodbath.ButtonPressed ) {
				SteamMatchmaking.AddRequestLobbyListStringFilter( "gamemode", "Bloodbath", ELobbyComparison.k_ELobbyComparisonEqual );
			}
			if ( GameFilter_CaptureTheFlag.ButtonPressed ) {
				SteamMatchmaking.AddRequestLobbyListStringFilter( "gamemode", "CaptureTheFlag", ELobbyComparison.k_ELobbyComparisonEqual );
			}
			if ( GameFilter_KingOfTheHill.ButtonPressed ) {
				SteamMatchmaking.AddRequestLobbyListStringFilter( "gamemode", "KingOfTheHill", ELobbyComparison.k_ELobbyComparisonEqual );
			}
			if ( GameFilter_Duel.ButtonPressed ) {
				SteamMatchmaking.AddRequestLobbyListStringFilter( "gamemode", "Duel", ELobbyComparison.k_ELobbyComparisonEqual );
			}
			SteamLobby.Instance.OpenLobbyList();
		}
		private void OnHostGameButtonPressed() {
			EmitSignalOnHostGame();
		}
		private void OnMatchmakeButtonPressed() {
			MatchmakingSpinner.Show();
			MatchmakingLabel.Show();

			CancelMatchmake.Show();

			MatchmakingLabel.Text = "FINDING_MULTIPLAYER_GAME";
			MatchmakingTimer.Start();

			MatchmakingThread.Start();

			MatchmakingPhase = 0;
		}
		private void OnJoinButtonPressed() {
			if ( MatchmakingLabel.Visible ) {
				return; // matchmaking, can't join game
			}

			JoiningLobbyContainer.Show();
			JoiningLobbySpinner.Set( "status", 1 );
			OnJoinGame( SelectedLobby );
		}

		private void OnMatchmakingLabelTimerTimeout() {
			string text = MatchmakingLabel.Text;

			int position = 0;
			int numDots = 0;
			while ( text.Find( '.', position, false ) != -1 ) {
				numDots++;
			}

			if ( numDots < 3 ) {
				text += ".";
			} else {
				text = "FINDING_MULTIPLAYER_GAME";
			}

			MatchmakingLabel.Text = text;
			MatchmakingTimer.Start();
		}

		public void ResetBrowser() {
			MatchmakingSpinner.Hide();
			MatchmakingLabel.Hide();
			CancelMatchmake.Hide();

			RefreshLobbies.Show();
			HostGame.Show();
			Matchmake.Show();
		}

		private void ConnectButton( Button button, Action pressedCallback ) {
			GameEventBus.ConnectSignal( button, Button.SignalName.MouseEntered, this, UIAudioManager.OnButtonFocusedCallable );
			GameEventBus.ConnectSignal( button, Button.SignalName.FocusEntered, this, UIAudioManager.OnButtonFocusedCallable );
			GameEventBus.ConnectSignal( button, Button.SignalName.Pressed, this, pressedCallback );
		}

		public override void _Ready() {
			base._Ready();

			Theme = SettingsData.DyslexiaMode ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

			HostGame = GetNode<Button>( "ControlBar/HostButton" );
			ConnectButton( HostGame, OnHostGameButtonPressed );

			RefreshLobbies = GetNode<Button>( "ControlBar/RefreshButton" );
			ConnectButton( RefreshLobbies, OnRefreshButtonPressed );

			JoiningLobbyContainer = GetNode<HBoxContainer>( "JoiningLobbyContainer" );
			JoiningLobbyLabel = GetNode<Label>( "JoiningLobbyContainer/JoiningLobbyLabel" );
			JoiningLobbySpinner = GetNode<Godot.Range>( "JoiningLobbyContainer/JoiningLobbySpinner" );

			Matchmake = GetNode<Button>( "ControlBar/MatchmakeButton" );
			ConnectButton( Matchmake, OnMatchmakeButtonPressed );

			CancelMatchmake = GetNode<Button>( "ControlBar/CancelMatchmakeButton" );
			MatchmakingSpinner = GetNode<Control>( "MatchMakingSpinner" );
			MatchmakingLabel = GetNode<Label>( "MatchMakingLabel" );

			MatchmakingTimer = GetNode<Timer>( "MatchMakingLabel/MatchMakingLabelTimer" );
			GameEventBus.ConnectSignal( MatchmakingTimer, Timer.SignalName.Timeout, this, OnMatchmakingLabelTimerTimeout );

			LobbyTable = GetNode<VBoxContainer>( "LobbyList/Lobbies" );

			Label mapName = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/MapNameContainer/MapNameLabel" );
			MapNameLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/MapNameContainer/Label" );

			Label playerCount = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/PlayerCountContainer/PlayerCountLabel" );
			PlayerCountLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/PlayerCountContainer/Label" );

			Label gameMode = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/GameModeContainer/GameModeLabel" );
			GameModeLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/GameModeContainer/Label" );

			Button joinButton = GetNode<Button>( "ControlBar2/JoinButton" );
			ConnectButton( joinButton, OnJoinButtonPressed );

			LobbyManager = GetNode<HBoxContainer>( "ControlBar" );
			JoinGame = GetNode<HBoxContainer>( "ControlBar2" );

			ShowFullServers = GetNode<CheckBox>( "FilterList/VBoxContainer/FullserversCheckBox" );
			GameEventBus.ConnectSignal( ShowFullServers, CheckBox.SignalName.MouseEntered, this, UIAudioManager.OnButtonFocusedCallable );
			GameEventBus.ConnectSignal( ShowFullServers, CheckBox.SignalName.FocusEntered, this, UIAudioManager.OnButtonFocusedCallable );
			GameEventBus.ConnectSignal( ShowFullServers, CheckBox.SignalName.Pressed, this, UIAudioManager.OnButtonPressedCallable );

			MatchmakingThread = new System.Threading.Thread( MatchmakingLoop );

			Timer refreshTimer = new Timer() {
				Name = nameof( refreshTimer ),
				WaitTime = 0.5f,
				OneShot = false,
				Autostart = true,
			};
			GameEventBus.ConnectSignal( refreshTimer, Timer.SignalName.Timeout, this, OnRefreshButtonPressed );
			AddChild( refreshTimer );

			LobbyJoinedCallback = Callable.From<ulong>( OnLobbyJoined );
			LobbyConnectionStatusChangedCallback = Callable.From<int>( OnConnectionStatusChanged );
			LobbyListUpdatedCallback = Callable.From( GetLobbyList );

			GameEventBus.ConnectSignal( SteamLobby.Instance, SteamLobby.SignalName.LobbyJoined, this, LobbyJoinedCallback );
			GameEventBus.ConnectSignal( SteamLobby.Instance, SteamLobby.SignalName.LobbyConnectionStatusChanged, this, LobbyConnectionStatusChangedCallback );
			GameEventBus.ConnectSignal( SteamLobby.Instance, SteamLobby.SignalName.LobbyListUpdated, this, LobbyListUpdatedCallback );

			SteamLobby.Instance.SetPhysicsProcess( true );

			GameFilter_Bloodbath = GetNode<CheckBox>( "FilterList/VBoxContainer/BloodbathCheckBox" );
			GameFilter_CaptureTheFlag = GetNode<CheckBox>( "FilterList/VBoxContainer/CaptureTheFlagCheckBox" );
			GameFilter_KingOfTheHill = GetNode<CheckBox>( "FilterList/VBoxContainer/KingOfTheHillCheckBox" );
			GameFilter_Duel = GetNode<CheckBox>( "FilterList/VBoxContainer/DuelCheckBox" );

			Instance = this;
		}
	};
};