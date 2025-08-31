using Godot;
using Steamworks;

namespace Multiplayer {
	public partial class ScoreBoard : CanvasLayer {
		[Signal]
		public delegate void LeaveGameEventHandler();

		public void SetDuelData( int round0Winner, int round1Winner, int round2Winner, CSteamID thisPlayer, CSteamID otherPlayer ) {
			Show();

			Label ThisPlayerName = GetNode<Label>( "MarginContainer/PlayerList/DuelData/ThisPlayerContainer/Name" );
			Label ThisPlayerRound0 = GetNode<Label>( "MarginContainer/PlayerList/DuelData/ThisPlayerContainer/Round0" );
			Label ThisPlayerRound1 = GetNode<Label>( "MarginContainer/PlayerList/DuelData/ThisPlayerContainer/Round1" );
			Label ThisPlayerRound2 = GetNode<Label>( "MarginContainer/PlayerList/DuelData/ThisPlayerContainer/Round2" );

			Label OtherPlayerName = GetNode<Label>( "MarginContainer/PlayerList/DuelData/OtherPlayerContainer/Name" );
			Label OtherPlayerRound0 = GetNode<Label>( "MarginContainer/PlayerList/DuelData/OtherPlayerContainer/Round0" );
			Label OtherPlayerRound1 = GetNode<Label>( "MarginContainer/PlayerList/DuelData/OtherPlayerContainer/Round1" );
			Label OtherPlayerRound2 = GetNode<Label>( "MarginContainer/PlayerList/DuelData/OtherPlayerContainer/Round2" );

			VBoxContainer dataTable = GetNode<VBoxContainer>( "MarginContainer/PlayerList/DuelData" );
			dataTable.Show();

			ThisPlayerName.Text = SteamFriends.GetFriendPersonaName( thisPlayer );
			OtherPlayerName.Text = SteamFriends.GetFriendPersonaName( otherPlayer );

			if ( round0Winner == 0 ) {
				ThisPlayerRound0.Text = "1";
				OtherPlayerRound0.Text = "0";
			} else if ( round0Winner == 1 ) {
				ThisPlayerRound0.Text = "0";
				OtherPlayerRound0.Text = "1";
			}
			if ( round1Winner == 0 ) {
				ThisPlayerRound1.Text = "1";
				OtherPlayerRound1.Text = "0";
			} else if ( round1Winner == 1 ) {
				ThisPlayerRound1.Text = "0";
				OtherPlayerRound1.Text = "1";
			} else {
				ThisPlayerRound1.Text = "/";
				OtherPlayerRound1.Text = "/";
			}
			if ( round2Winner == 0 ) {
				ThisPlayerRound2.Text = "1";
				OtherPlayerRound2.Text = "0";
			} else if ( round2Winner == 1 ) {
				ThisPlayerRound2.Text = "0";
				OtherPlayerRound2.Text = "1";
			} else {
				ThisPlayerRound2.Text = "/";
				OtherPlayerRound2.Text = "/";
			}
		}

		public override void _Ready() {
			base._Ready();

			Button LeaveButton = GetNode<Button>( "MarginContainer/LeaveButton" );
			LeaveButton.Connect( Button.SignalName.Pressed, Callable.From( EmitSignalLeaveGame ) );
		}
	};
};