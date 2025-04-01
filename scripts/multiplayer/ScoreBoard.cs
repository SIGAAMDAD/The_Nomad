using System.Data;
using Godot;

namespace Multiplayer {
	public partial class ScoreBoard : Control {
		private MultiplayerData Root;
		private VBoxContainer BloodbathData;

		public override void _Ready() {
			base._Ready();

			Root = GetNode<MultiplayerData>( "/root/Multiplayer" );
			BloodbathData = GetNode<VBoxContainer>( "MarginContainer/PlayerList/BloodbathData" );

			int startIndex = 2;
			VBoxContainer dataTable = null;

			switch ( Root.GetMode() ) {
			case Mode.GameMode.Bloodbath:
				dataTable = BloodbathData;
				break;
			};

			/*
			foreach ( var player in Root.GetPlayers().Values ) {
				HBoxContainer data = (HBoxContainer)dataTable.GetChild( startIndex );
				
				data.Show();
				( (Label)data.GetChild( 0 ) ).Text = player.MultiplayerUsername;
				( (Label)data.GetChild( 1 ) ).Text = player.MultiplayerKills.ToString();
				( (Label)data.GetChild( 2 ) ).Text = player.MultiplayerDeaths.ToString();

				startIndex++;
			}
			*/
		}
	};
};