using System;
using Godot;
using Multiplayer;
using System.Threading;
using System.Linq;

public partial class LobbyFactory : Control {
	private LineEdit LobbyName;
	private HSlider MaxPlayers;
	private OptionButton MapList;
	private OptionButton GameModeList;
	private Label PlayerCountLabel;

	private static Thread LoadThread;
	private static PackedScene LoadedLevel;

	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void OnMapSelectionChanged( int nSelected ) {
		MultiplayerMapManager.MapData data = MultiplayerMapManager.MapCache.Values.ElementAt( nSelected );

		GameModeList.Clear();
		if ( data.ModeBloodbath ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.Bloodbath ] );
		}
		if ( data.ModeTeamBrawl ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.TeamBrawl ] );
		}
		if ( data.ModeCaptureTheFlag ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.CaptureTheFlag ] );
		}
		if ( data.ModeKingOfTheHill ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.KingOfTheHill ] );
		}
		if ( data.ModeDuel ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.Duel ] );
		}
		GameModeList.Selected = 0;
	}

	public override void _Ready() {
		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		Label TitleLabel = GetNode<Label>( "TitleLabel" );

		LobbyName = GetNode<LineEdit>( "MarginContainer/VBoxContainer/NameContainer/NameLineEdit" );
		MaxPlayers = GetNode<HSlider>( "MarginContainer/VBoxContainer/MaxPlayersContainer/HBoxContainer/MaxPlayersHSlider" );
		MaxPlayers.Connect( "value_changed", Callable.From<double>( ( value ) => { PlayerCountLabel.Text = Convert.ToString( (int)value ); } ) );

		PlayerCountLabel = GetNode<Label>( "MarginContainer/VBoxContainer/MaxPlayersContainer/HBoxContainer/PlayerCountLabel" );
		PlayerCountLabel.Text = Convert.ToString( MaxPlayers.Value );

		MapList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/MapContainer/MapOptionButton" );
		MapList.Connect( "item_selected", Callable.From<int>( OnMapSelectionChanged ) );

		GameModeList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/GameModeContainer/GameModeOptionButton" );

		Button CreateButton = GetNode<Button>( "CreateButton" );
		CreateButton.Connect( "pressed", Callable.From( OnCreateButtonPressed ) );

		foreach ( var map in MultiplayerMapManager.MapCache ) {
			MapList.AddItem( map.Value.Name );
		}
		for ( int i = 0; i < (int)Mode.GameMode.Count; i++ ) {
			GameModeList.AddItem( Mode.ModeNames[ (Mode.GameMode)i ] );
		}
	}

	private void OnCreateButtonPressed() {
		Console.PrintLine( "Creating lobby..." );

		string name = LobbyName.Text;
		if ( LobbyName.Text.Length == 0 ) {
			string username = SteamManager.GetSteamName();
			if ( username[ username.Length - 1 ] == 's' ) {
				name = string.Format( "{0}' Lobby", username );
			} else {
				name = string.Format( "{0}'s Lobby", username );
			}
		}

		SteamLobby.Instance.SetLobbyName( LobbyName.Text );
		SteamLobby.Instance.SetMaxMembers( (int)MaxPlayers.Value );
		SteamLobby.Instance.SetMap( MultiplayerMapManager.MapCache.Values.ElementAt( MapList.Selected ).Name );
		SteamLobby.Instance.SetGameMode( (uint)GameModeList.Selected );
		SteamLobby.Instance.SetHostStatus( true );
		GameConfiguration.GameMode = GameMode.Multiplayer;

		SteamLobby.Instance.CreateLobby();

		Console.PrintLine( string.Format( "Starting game [map: {0}, gamemode: {1}]...", MultiplayerMapManager.MapCache.Values.ElementAt( MapList.Selected ).Name, GameModeList.Selected ) );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );

		GetTree().ChangeSceneToPacked( ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/lobby_room.tscn" ) );
	}
};
