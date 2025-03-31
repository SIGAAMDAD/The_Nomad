using System.Threading;
using Godot;
using Multiplayer;

public partial class LobbyFactory : Control {
	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private LineEdit LobbyName;
	private HSlider MaxPlayers;
	private OptionButton MapList;
	private OptionButton GameModeList;

	private static Thread LoadThread;
	private static PackedScene LoadedLevel;

	public override void _Ready() {
		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		Label TitleLabel = GetNode<Label>( "TitleLabel" );

		LobbyName = GetNode<LineEdit>( "MarginContainer/VBoxContainer/NameContainer/NameLineEdit" );
		MaxPlayers = GetNode<HSlider>( "MarginContainer/VBoxContainer/MaxPlayersContainer/MaxPlayersHSlider" );
		MapList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/MapContainer/MapOptionButton" );
		GameModeList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/GameModeContainer/GameModeOptionButton" );

		Button CreateButton = GetNode<Button>( "MarginContainer/CreateButton" );
		CreateButton.Connect( "pressed", Callable.From( OnCreateButtonPressed ) );

		MultiplayerMapManager.Init();

		foreach ( var map in MultiplayerMapManager.MapCache ) {
			MapList.AddItem( map.Name );
		}
		for ( int i = 0; i < (int)Mode.GameMode.Count; i++ ) {
			GameModeList.AddItem( Mode.ModeNames[ (Mode.GameMode)i ] );
		}
	}

	private void OnFinishedLoading() {
		LoadThread.Join();
		GetTree().ChangeSceneToPacked( LoadedLevel );
		QueueFree();
	}
	private void OnCreateButtonPressed() {
		Console.PrintLine( "Creating lobby..." );

		SteamLobby.Instance.SetLobbyName( LobbyName.Text );
		SteamLobby.Instance.SetMaxMembers( (int)MaxPlayers.Value );
		SteamLobby.Instance.SetMap( MapList.Selected );
		SteamLobby.Instance.SetGameMode( (uint)GameModeList.Selected );
		SteamLobby.Instance.SetHostStatus( true );
		GameConfiguration.GameMode = GameMode.Multiplayer;

		SteamLobby.Instance.CreateLobby();

		Console.PrintLine( "Starting game..." );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		string modeName;
		switch ( GameModeList.Selected ) {
		case (int)Mode.GameMode.Bloodbath:
			modeName = "bloodbath";
			break;
		case (int)Mode.GameMode.TeamBrawl:
			modeName = "teambrawl";
			break;
		case (int)Mode.GameMode.CaptureTheFlag:
			modeName = "ctf";
			break;
		default:
			return;
		};

		Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );
		LoadThread = new Thread( () => {
			LoadedLevel = ResourceLoader.Load<PackedScene>( "res://levels/" + MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName + "_mp_" + modeName + ".tscn" );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
};
