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
using Steamworks;
using Multiplayer;
using System.Threading;
using System.Collections.Generic;
using System;
using Steam;
using ResourceCache;
using Utils;

namespace Menus {
	/*
	===================================================================================
	
	LobbyRoom
		
	===================================================================================
	*/
	
	public partial class LobbyRoom : Control {
		private static readonly Color GoodPing = new Color( 0.0f, 1.0f, 0.0f, 1.0f );
		private static readonly Color DecentPing = new Color( 1.0f, 1.0f, 0.0f, 1.0f );
		private static readonly Color BadPing = new Color( 1.0f, 0.0f, 0.0f, 1.0f );

		private readonly Color FocusColor = new Color( 0.0f, 1.0f, 0.0f, 1.0f );
		private readonly Color DefaultColor = new Color( 0.0f, 0.0f, 0.0f, 1.0f );

		private readonly Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
		private readonly Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		private VBoxContainer PlayerList;
		private Button StartGameButton;
		private Button ExitLobbyButton;

		private HBoxContainer CurrentFocus = null;

		private Label VoteLabel;

		private HBoxContainer ClonerContainer;

		private Dictionary<CSteamID, bool> StartGameVotes = null;

		private Callable PlayerLeaveCallback;

		public static LobbyRoom Instance;

		/*
		===============
		FocusPlayer
		===============
		*/
		public void FocusPlayer( HBoxContainer focus ) {
			if ( CurrentFocus != focus ) {
				UnfocusPlayer( CurrentFocus );
			}
			( focus.GetChild( 0 ) as Label ).Modulate = FocusColor;
			( focus.GetChild( 1 ) as Button ).Show();
			CurrentFocus = focus;
		}

		/*
		===============
		UnfocusPlayer
		===============
		*/
		public void UnfocusPlayer( HBoxContainer focus ) {
			( focus.GetChild( 0 ) as Label ).Modulate = DefaultColor;
			( focus.GetChild( 1 ) as Button ).Hide();
		}

		/*
		===============
		KickPlayer
		===============
		*/
		public void KickPlayer( CSteamID steamId ) {
		}

		/*
		===============
		VoteStart
		===============
		*/
		private void VoteStart( CSteamID senderId ) {
			if ( !SteamLobby.Instance.IsHost ) {
				return;
			}
			Console.PrintLine( "Received lobby vote..." );
			if ( !StartGameVotes.ContainsKey( senderId ) ) {
				StartGameVotes.Add( senderId, true );
			}
			StartGameVotes[ senderId ] = true;
		}

		/*
		===============
		CancelVote
		===============
		*/
		private void CancelVote( CSteamID senderId ) {
			if ( !SteamLobby.Instance.IsHost ) {
				return;
			}
			if ( !StartGameVotes.ContainsKey( senderId ) ) {
				StartGameVotes.Add( senderId, false );
			} else {
				StartGameVotes[ senderId ] = false;
			}
		}

		/*
		===============
		LoadGame
		===============
		*/
		private void LoadGame() {
			UIAudioManager.OnActivate();

			string modeName = (Mode.GameMode)SteamLobby.Instance.LobbyGameMode switch {
				Mode.GameMode.Bloodbath => "ffa",
				Mode.GameMode.CaptureTheFlag => "ctf",
				Mode.GameMode.KingOfTheHill => "koth",
				Mode.GameMode.Duel => "duel",
				Mode.GameMode.Extraction => "extraction",
				Mode.GameMode.HoldTheLine => "htl",
				Mode.GameMode.BountyHunt => "bh",
				_ => ""
			};

			GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( "res://levels/" +
				MultiplayerMapManager.MapCache[ SteamLobby.Instance.LobbyMap ].FileName
				+ "_mp_" + modeName + ".tscn"
			);
		}

		/*
		===============
		OnStartGameButtonPressed
		===============
		*/
		private void OnStartGameButtonPressed() {
			if ( !SteamLobby.Instance.IsHost ) {
				// if we're not the host, send a vote to start command
				ServerCommandManager.SendCommand( ServerCommandType.VoteStart );
				return;
			}

			LoadGame();

			ServerCommandManager.SendCommand( ServerCommandType.StartGame );
		}

		/*
		===============
		OnExitLobbyButtonPressed
		===============
		*/
		private void OnExitLobbyButtonPressed() {
			UIAudioManager.OnButtonPressed();

			SteamLobby.Instance.LeaveLobby();

			Hide();
			GetTree().ChangeSceneToPacked( SceneCache.GetScene( "res://scenes/main_menu.tscn" ) );
		}

		/*
		===============
		OnPlayerJoined
		===============
		*/
		private void OnPlayerJoined( ulong steamId ) {
			Console.PrintLine( string.Format( "Adding {0} to game...", steamId ) );

			SteamLobby.Instance.GetLobbyMembers();

			CSteamID userId = (CSteamID)steamId;

			HBoxContainer container = ClonerContainer.Duplicate() as HBoxContainer;
			container.Show();
			( container.GetChild( 3 ) as Label ).Text = SteamFriends.GetFriendPersonaName( userId );
			( container.GetChild( 4 ) as Button ).Hide();
			PlayerList.AddChild( container );
		}

		/*
		===============
		OnPlayerLeft
		===============
		*/
		private void OnPlayerLeft( ulong steamId ) {
			SteamLobby.Instance.GetLobbyMembers();

			CSteamID userId = (CSteamID)steamId;
			if ( userId == SteamUser.GetSteamID() ) {
				return;
			}

			string username = SteamFriends.GetFriendPersonaName( userId );
			for ( int i = 0; i < PlayerList.GetChildCount(); i++ ) {
				if ( ( ( PlayerList.GetChild( i ) as HBoxContainer ).GetChild( 3 ) as Label ).Text == username ) {
					PlayerList.GetChild( i ).QueueFree();
					PlayerList.RemoveChild( PlayerList.GetChild( i ) );
					break;
				}
			}

			Console.PrintLine( string.Format( "{0} has faded away...", username ) );
			SteamLobby.Instance.RemovePlayer( userId );
		}

		/*
		===============
		CheckAutoStart
		===============
		*/
		/// <summary>
		/// if the host is currently AFK, then check to see if all the
		/// requirements are met to automatically start the game
		/// </summary>
		public void CheckAutoStart() {
			if ( !SteamLobby.Instance.IsHost ) {
				return;
			}

			int numStartVotes = 0;
			int requiredVotes = SteamLobby.Instance.LobbyMemberCount / 2;

			foreach ( var vote in StartGameVotes ) {
				if ( vote.Value ) {
					numStartVotes++;
				}
			}
			if ( numStartVotes >= requiredVotes ) {
				ServerCommandManager.SendCommand( ServerCommandType.StartGame );
			}
		}

		/*
		===============
		PlayerIsInQueue
		===============
		*/
		private bool PlayerIsInQueue( CSteamID userId ) {
			if ( userId == SteamManager.GetSteamID() ) {
				return true;
			}
			for ( int i = 0; i < PlayerList.GetChildCount(); i++ ) {
				string username = SteamFriends.GetFriendPersonaName( userId );
				if ( ( ( PlayerList.GetChild( i ) as HBoxContainer ).GetChild( 3 ) as Label ).Text == username ) {
					PlayerList.GetChild( i ).QueueFree();
					PlayerList.RemoveChild( PlayerList.GetChild( i ) );
					return true;
				}
			}
			return false;
		}

		/*
		===============
		PlayerKicked
		===============
		*/
		private void PlayerKicked( CSteamID senderId ) {
		}

		/*
		===============
		OnVoteKickResponseYes
		===============
		*/
		private void OnVoteKickResponseYes( CSteamID senderId ) {
		}

		/*
		===============
		OnVoteKickResponseNo
		===============
		*/
		private void OnVoteKickResponseNo( CSteamID senderId ) {
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			GetTree().CurrentScene = this;

			GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( LoadingScreen.MethodName.FadeOut );

			Theme = SettingsData.DyslexiaMode ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

			PlayerList = GetNode<VBoxContainer>( "MarginContainer/PlayerList" );

			StartGameButton = GetNode<Button>( "StartGameButton" );
			Methods.ConnectMenuButton( StartGameButton, this, OnStartGameButtonPressed );

			ExitLobbyButton = GetNode<Button>( "ExitLobbyButton" );
			Methods.ConnectMenuButton( ExitLobbyButton, this, OnExitLobbyButtonPressed );

			VoteLabel = GetNode<Label>( "VoteLabel" );

			ClonerContainer = GetNode<HBoxContainer>( "MarginContainer/PlayerList/ClonerContainer" );

			//		SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
			PlayerLeaveCallback = Callable.From<ulong>( OnPlayerLeft );
			GameEventBus.ConnectSignal( SteamLobby.Instance, SteamLobby.SignalName.ClientLeftLobby, this, PlayerLeaveCallback );

			ServerCommandManager.RegisterCommandCallback( ServerCommandType.StartGame, ( senderId ) => { LoadGame(); } );
			ServerCommandManager.RegisterCommandCallback( ServerCommandType.VoteStart, VoteStart );
			ServerCommandManager.RegisterCommandCallback( ServerCommandType.KickPlayer, PlayerKicked );
			ServerCommandManager.RegisterCommandCallback( ServerCommandType.VoteKickResponse_Yes, OnVoteKickResponseYes );
			ServerCommandManager.RegisterCommandCallback( ServerCommandType.VoteKickResponse_Yes, OnVoteKickResponseNo );
			ServerCommandManager.RegisterCommandCallback( ServerCommandType.ConnectedToLobby, ( senderId ) => OnPlayerJoined( (ulong)senderId ) );

			HBoxContainer container = ClonerContainer.Duplicate() as HBoxContainer;
			container.Show();
			( container.GetChild( 3 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamManager.GetSteamID() );
			( container.GetChild( 4 ) as Button ).Hide();
			PlayerList.AddChild( container );

			SteamLobby.Instance.GetLobbyMembers();

			if ( SteamLobby.Instance.IsHost ) {
				StartGameVotes = new Dictionary<CSteamID, bool>( SteamLobby.MAX_LOBBY_MEMBERS );
			} else {
				for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
					if ( PlayerIsInQueue( SteamLobby.Instance.LobbyMembers[ i ] ) ) {
						continue;
					}
					container = ClonerContainer.Duplicate() as HBoxContainer;
					container.Show();
					( container.GetChild( 3 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[ i ] );
					( container.GetChild( 4 ) as Button ).Hide();
					PlayerList.AddChild( container );
				}
			}

			Instance = this;
		}
	};
};