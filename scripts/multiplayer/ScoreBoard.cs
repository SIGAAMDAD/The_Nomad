using Godot;
using Steamworks;

namespace Multiplayer {
	public partial class ScoreBoard : Control {
		public void SetDuelData( uint round0Winner, uint round1Winner, uint round2Winner, CSteamID thisPlayer, CSteamID otherPlayer ) {
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
			}
			if ( round2Winner == 0 ) {
				ThisPlayerRound2.Text = "1";
				OtherPlayerRound2.Text = "0";
			} else if ( round2Winner == 1 ) {
				ThisPlayerRound2.Text = "0";
				OtherPlayerRound2.Text = "1";
			}
		}
	};
};