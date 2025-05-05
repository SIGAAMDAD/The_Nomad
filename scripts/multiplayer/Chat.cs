using Godot;
using Steamworks;

namespace Multiplayer {
	public partial class Chat : Control {
		private MarginContainer ExpandedContainer;
		private MarginContainer MinimizedContainer;
		private RichTextLabel FullText;
		private RichTextLabel RecentText;
		private LineEdit Message;

		public override void _UnhandledInput( InputEvent @event ) {
			base._UnhandledInput( @event );

			if ( @event is InputEventKey key && key != null && key.IsPressed() ) {
				if ( key.GetKeycodeWithModifiers() == Key.Slash ) {
					if ( Message.Editable ) {
						Message.Editable = false;
						ExpandedContainer.Hide();
						MinimizedContainer.Show();
						Message.Size = new Godot.Vector2( 140, 31 );
					} else {
						Message.Editable = true;
						Message.GrabFocus();
						ExpandedContainer.Show();
						MinimizedContainer.Hide();
						Message.Size = new Godot.Vector2( 290, 31 );
					}
			 	} else if ( key.GetKeycodeWithModifiers() == Key.Enter ) {
					Message.Editable = false;
					Message.Size = new Godot.Vector2( 140, 31 );
					SteamMatchmaking.SendLobbyChatMsg( SteamLobby.Instance.GetLobbyID(), Message.Text.ToAsciiBuffer(), Message.Text.Length );
					Message.Clear();
				}
			}
		}
		private void OnChatMessageReceived( ulong senderId, string message ) {
			string username = SteamFriends.GetFriendPersonaName( (CSteamID)senderId );
			
			RecentText.Text = message;
			
			FullText.AppendText( string.Format( "[{0}] {1}\n", username, message ) );
		}
        public override void _Ready() {
			base._Ready();

			ExpandedContainer = GetNode<MarginContainer>( "Expanded" );
			MinimizedContainer = GetNode<MarginContainer>( "Minimized" );
			FullText = GetNode<RichTextLabel>( "Expanded/RichTextLabel" );
			RecentText = GetNode<RichTextLabel>( "Minimized/RichTextLabel" );
			Message = GetNode<LineEdit>( "LineEdit" );

			SteamLobby.Instance.Connect( "ChatMessageReceived", Callable.From<ulong, string>( OnChatMessageReceived ) );
		}
    };
};