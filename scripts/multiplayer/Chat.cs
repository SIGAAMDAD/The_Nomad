using Godot;
using Steamworks;
using System;
using System.Linq;

namespace Multiplayer {
	public partial class Chat : CanvasLayer {
		private MarginContainer ExpandedContainer;
		private MarginContainer MinimizedContainer;
		private RichTextLabel FullText;
		private RichTextLabel RecentText;
		private LineEdit Message;

		private void HandleChatCommand( string text ) {
			if ( !text.StartsWith( "/" ) ) {
				return;
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
					return;
				}
				byte[] data = SteamLobby.SteamLobbySecurity.SecureOutgoingMessage( SteamLobby.CompressText( message ), targetId );
				byte[] packet = new byte[ 1 + data.Length ];

				packet[ 0 ] = (byte)SteamLobby.MessageType.ChatMessage_PlayerOnly;
				Buffer.BlockCopy( data, 0, packet, 1, data.Length );

				SteamLobby.Instance.SendTargetPacket( targetId, packet );
				break;
			};
		}

		public override void _UnhandledInput( InputEvent @event ) {
			base._UnhandledInput( @event );

			if ( Input.IsActionJustPressed( "chat_open" ) ) {
				if ( Message.HasFocus() ) {
					Message.Clear();
					Message.ReleaseFocus();
					ExpandedContainer.Hide();
					MinimizedContainer.Show();
					Message.Size = new Godot.Vector2( 140, 31 );
				} else {
					Message.GrabFocus();
					ExpandedContainer.Show();
					MinimizedContainer.Hide();
					Message.Size = new Godot.Vector2( 290, 31 );
				}
			}
			if ( Input.IsActionJustReleased( "chat_send" ) ) {
				HandleChatCommand( Message.Text );
				SteamMatchmaking.SendLobbyChatMsg( SteamLobby.Instance.GetLobbyID(), Message.Text.ToAsciiBuffer(), Message.Text.Length );
				Message.Clear();
				Message.ReleaseFocus();
				ExpandedContainer.Hide();
				MinimizedContainer.Show();
				Message.Size = new Godot.Vector2( 140, 31 );
			}
		}
		private void OnChatMessageReceived( ulong senderId, string message ) {
			string fullMessage = string.Format( "[{0}] {1}\n", SteamFriends.GetFriendPersonaName( (CSteamID)senderId ), message );
			RecentText.Text = fullMessage;
			FullText.AppendText( fullMessage );
		}
        public override void _Ready() {
			base._Ready();

			Name = "ChatBar";

			ExpandedContainer = GetNode<MarginContainer>( "Expanded" );
			MinimizedContainer = GetNode<MarginContainer>( "Minimized" );
			FullText = GetNode<RichTextLabel>( "Expanded/RichTextLabel" );
			RecentText = GetNode<RichTextLabel>( "Minimized/RichTextLabel" );
			Message = GetNode<LineEdit>( "LineEdit" );

			SteamLobby.Instance.Connect( "ChatMessageReceived", Callable.From<ulong, string>( OnChatMessageReceived ) );
		}
    };
};