using Godot;
using GodotSteam;

namespace Multiplayer {
	public partial class Chat : Control {
		private MarginContainer ExpandedContainer;
		private MarginContainer MinimizedContainer;
		private RichTextLabel FullText;
		private RichTextLabel RecentText;
		private LineEdit Message;

		public override void _UnhandledInput( InputEvent @event ) {
			base._UnhandledInput( @event );

			if ( @event is not InputEventKey ) {
				return;
			} else if ( ( (InputEventKey)@event ).Keycode == Key.Slash ) {
				if ( Message.Editable ) {
					ExpandedContainer.Hide();
					MinimizedContainer.Show();
					Message.Size = new Godot.Vector2( 140, 31 );
				} else {
					ExpandedContainer.Show();
					MinimizedContainer.Hide();
					Message.Size = new Godot.Vector2( 290, 31 );
				}

				Message.Editable = !Message.Editable;
			} else if ( ( (InputEventKey)@event ).Keycode == Key.Enter ) {
				Message.Editable = false;
				Message.Size = new Godot.Vector2( 140, 31 );
				Steam.SendLobbyChatMsg( SteamLobby.Instance.GetLobbyID(), Message.Text );
				Message.Clear();
			}
		}
		private void OnChatMessageReceived( ulong senderId, string message ) {
			string username = Steam.GetFriendPersonaName( senderId );
			
			RecentText.Clear();
			RecentText.Text = message;

			FullText.AppendText( "[" + username + "] " + message + "\n" );
		}
        public override void _Ready() {
			base._Ready();

			ExpandedContainer = GetNode<MarginContainer>( "Expanded" );
			MinimizedContainer = GetNode<MarginContainer>( "Minimized" );
			FullText = GetNode<RichTextLabel>( "Expanded/RichTextLabel" );
			RecentText = GetNode<RichTextLabel>( "Mimimized/RichTextLabel" );
			Message = GetNode<LineEdit>( "LineEdit" );

			GetNode( "/root/SteamLobby" ).Connect( "ChatMessageReceived", Callable.From<ulong, string>( OnChatMessageReceived ) );
		}
    };
};