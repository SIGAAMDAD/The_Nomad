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
using Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Multiplayer {
	public partial class Chat : CanvasLayer {
		private MarginContainer ExpandedContainer;
		private MarginContainer MinimizedContainer;
		private RichTextLabel FullText;
		private RichTextLabel RecentText;
		private LineEdit Message;

		private static readonly HashSet<string> BlockedTerms = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) {
			"nigger",
			"gook",
			"kike",
			"spic",
			"chink",

			"cunt",
			"bitch",
			"slut",

			"fagot",
			"dyke",
			"tranny"
		};

		private static string FilterText( string input ) {
			if ( string.IsNullOrWhiteSpace( input ) ) {
				return input;
			}

			return Regex.Replace( input, @"\S+", match => {
				string token = match.Value;
				string normalized = NormalizeToken( input );

				return BlockedTerms.Contains( normalized ) ? new string( '*', token.Length ) : token;
			} );
		}
		private static string NormalizeToken( string text ) {
			string normalized = text.ToLowerInvariant();

			normalized = Regex.Replace( normalized, @"[^a-z]", "" );

			normalized = Regex.Replace( normalized, @"[1!]i", "1" );
			normalized = Regex.Replace( normalized, @"[3]e", "b" );
			normalized = Regex.Replace( normalized, @"[4]a", "h" );
			normalized = Regex.Replace( normalized, @"[@]a", "o" );
			normalized = Regex.Replace( normalized, @"[5]s", "s" );
			normalized = Regex.Replace( normalized, @"[7]t", "t" );

			normalized = Regex.Replace( normalized, @"(.)\1{2,}", "$1" );

			normalized = Regex.Replace( normalized, "[aeiou]", "" );

			return normalized;
		}

		private const string PLACEHOLDER = "PRESS '/' TO BEGIN TYPING";

		private bool HandleChatCommand( string text ) {
			if ( !text.StartsWith( "\\" ) ) {
				return false;
			}

			string[] parts = text[ 1.. ].Split( ' ' );
			string command = parts[ 0 ].ToLower();
			string[] args = [ .. parts.Skip( 1 ) ];

			// hand-coded commands
			switch ( command ) {
			case "sendto:team":
			case "sendto:player":
				string player = parts[ 1 ];
				CSteamID targetId = CSteamID.Nil;
				string message = null;
				for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
					if ( player == SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[ i ] ) ) {
						message = parts[ 2 ];
						targetId = SteamLobby.Instance.LobbyMembers[ i ];
						break;
					}
				}
				if ( message == null ) {
					Console.PrintError( string.Format( "Invalid player \"{0}\" for sendto:player command", player ) );
					return false;
				}
				byte[] data = SteamLobby.SteamLobbySecurity.SecureOutgoingMessage( SteamLobby.CompressText( message ), targetId );
				byte[] packet = new byte[ 1 + data.Length ];

				packet[ 0 ] = (byte)SteamLobby.MessageType.ChatMessage_PlayerOnly;
				Buffer.BlockCopy( data, 0, packet, 1, data.Length );

				SteamLobby.Instance.SendTargetPacket( targetId, packet );
				break;
			}
			return true;
		}

		public override void _UnhandledInput( InputEvent @event ) {
			base._UnhandledInput( @event );

			if ( Input.IsActionJustPressed( "chat_open" ) ) {
				if ( Message.HasFocus() ) {
					Message.Clear();
					Message.PlaceholderText = "";
					Message.ReleaseFocus();
					ExpandedContainer.Hide();
					MinimizedContainer.Show();
					Message.Size = new Godot.Vector2( 140, 31 );
				} else {
					Message.GrabFocus();
					Message.PlaceholderText = PLACEHOLDER;
					ExpandedContainer.Show();
					MinimizedContainer.Hide();
					Message.Size = new Godot.Vector2( 290, 31 );
				}
			}
			if ( Input.IsActionJustReleased( "chat_send" ) ) {
				string messageText = FilterText( Message.Text );
				SteamMatchmaking.SendLobbyChatMsg( SteamLobby.Instance.LobbyId, messageText.ToAsciiBuffer(), Message.Text.Length );
				Message.Clear();
				Message.PlaceholderText = PLACEHOLDER;
				Message.ReleaseFocus();
				ExpandedContainer.Hide();
				MinimizedContainer.Show();
				Message.Size = new Godot.Vector2( 140, 31 );
			}
		}
		private void OnChatMessageReceived( ulong senderId, string message, int messageType ) {
			string fullMessage = string.Format( "[{0}] {1}\n", SteamFriends.GetFriendPersonaName( (CSteamID)senderId ), message );
			RecentText.ParseBbcode( fullMessage );
			FullText.AppendText( fullMessage );
		}

		private void OnPlayerJoined( ulong steamId ) {
			string message = string.Format( "[color=cyan]{0} has joined the lobby[/color]", SteamFriends.GetFriendPersonaName( (CSteamID)steamId ) );
			RecentText.ParseBbcode( message );
			FullText.AppendText( message );
		}
		private void OnPlayerLeft( ulong steamId ) {
			string message = string.Format( "[color=cyan]{0} has left the lobby[/color]", SteamFriends.GetFriendPersonaName( (CSteamID)steamId ) );
			RecentText.ParseBbcode( message );
			FullText.AppendText( message );
		}
		public override void _Ready() {
			base._Ready();

			Name = "ChatBar";

			ExpandedContainer = GetNode<MarginContainer>( "Expanded" );
			MinimizedContainer = GetNode<MarginContainer>( "Minimized" );
			FullText = GetNode<RichTextLabel>( "Expanded/RichTextLabel" );
			RecentText = GetNode<RichTextLabel>( "Minimized/RichTextLabel" );
			Message = GetNode<LineEdit>( "LineEdit" );

			GameEventBus.ConnectSignal( SteamLobby.Instance, SteamLobby.SignalName.ChatMessageReceived, this, Callable.From<ulong, string, int>( OnChatMessageReceived ) );
			GameEventBus.Subscribe<SteamLobby.ClientJoinedLobbyEventHandler>( this, OnPlayerJoined );
			GameEventBus.Subscribe<SteamLobby.ClientLeftLobbyEventHandler>( this, OnPlayerLeft );
		}
	};
};