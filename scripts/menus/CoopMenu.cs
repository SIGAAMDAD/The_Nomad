using Godot;
using System.Linq;
using Multiplayer;

public partial class CoopMenu : Control {
	private OptionButton MapList;
	private OptionButton GameModeList;

	private PackedScene LoadedWorld;
	private System.Threading.Thread LoadThread;

	private AudioStreamPlayer UIChannel;

	private bool Loaded = false;

	private static Tween AudioFade;

	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void OnAudioFadeFinished() {
		GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
		AudioFade.Finished -= OnAudioFadeFinished;
	}

	private void OnMapSelectionChanged( int nSelected ) {
		MultiplayerMapManager.MapData data = MultiplayerMapManager.MapCache.Values.ElementAt( nSelected );

		GameModeList.Clear();
		if ( data.ModeBloodbath ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.Bloodbath ], (int)Mode.GameMode.Bloodbath );
		}
		if ( data.ModeTeamBrawl ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.TeamBrawl ], (int)Mode.GameMode.TeamBrawl );
		}
		if ( data.ModeCaptureTheFlag ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.CaptureTheFlag ], (int)Mode.GameMode.CaptureTheFlag );
		}
		if ( data.ModeKingOfTheHill ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.KingOfTheHill ], (int)Mode.GameMode.KingOfTheHill );
		}
		if ( data.ModeDuel ) {
			GameModeList.AddItem( Mode.ModeNames[ Mode.GameMode.Duel ], (int)Mode.GameMode.Duel );
		}
		GameModeList.Selected = 0;
	}

	private void OnFinishedLoading() {
		FinishedLoading -= OnFinishedLoading;

		LoadThread.Join();
		GetTree().ChangeSceneToPacked( LoadedWorld );
	}
	private void OnTransitionFinished() {
		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Console.PrintLine( "Loading game..." );

		FinishedLoading += OnFinishedLoading;

		string modeName;
		switch ( (Mode.GameMode)GameModeList.GetItemId( GameModeList.Selected ) ) {
		case Mode.GameMode.Duel:
			modeName = "duel";
			break;
		default:
			modeName = "";
			break;
		};

		string mapName = "res://levels/" + MultiplayerMapManager.MapCache.Values.ElementAt( MapList.Selected ).FileName + "_" + modeName + "_2p.tscn";
		LoadThread = new System.Threading.Thread( () => {
			LoadedWorld = ResourceLoader.Load<PackedScene>( mapName );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	private void OnStartButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnTransitionFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
		/*
		TODO: split screen with online
		string name = LobbyName.Text;
		if ( LobbyName.Text.Length == 0 ) {
			string username = SteamManager.GetSteamName();
			if ( username[ username.Length - 1 ] == 's' ) {
				name = string.Format( "{0}' Lobby", username );
			} else {
				name = string.Format( "{0}'s Lobby", username );
			}
		}
		*/

		GameConfiguration.GameMode = GameMode.LocalCoop2;

		Console.PrintLine( string.Format( "Starting game [map: {0}, gamemode: {1}]...", MultiplayerMapManager.MapCache.Values.ElementAt( MapList.Selected ).Name, GameModeList.Selected ) );
	}

	public override void _Ready() {
		base._Ready();

		MapList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/MapContainer/MapOptionButton" );
		MapList.Connect( "item_selected", Callable.From<int>( OnMapSelectionChanged ) );

		GameModeList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/GameModeContainer/GameModeOptionButton" );

		UIChannel = GetNode<AudioStreamPlayer>( "../../UIChannel" );

		Button StartButton = GetNode<Button>( "StartButton" );
		StartButton.Connect( "pressed", Callable.From( OnStartButtonPressed ) );

		foreach ( var map in MultiplayerMapManager.MapCache ) {
			MapList.AddItem( map.Value.Name );
		}
		for ( int i = 0; i < (int)Mode.GameMode.Count; i++ ) {
			GameModeList.AddItem( Mode.ModeNames[ (Mode.GameMode)i ] );
		}
	}
};
