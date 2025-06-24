using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public partial class StoryMenu : Control {
	/*
	private TextureRect TextureRect;
	private Label LocationLabel;
	private Label TimeLabel;
	*/

	private Button ContinueGameButton;
	private Button LoadGameButton;
	private Button NewGameButton;

	private PackedScene LoadedWorld;
	private Thread LoadThread;

	private VBoxContainer SlotsContainer;

	private ConfirmationDialog TutorialsPopup;

	[Signal]
	public delegate void BeginGameEventHandler();
	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void OnContinueGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnContinueGameFinished ) );

		Hide();
		GetNode<LoadingScreen>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Console.PrintLine( "Loading game..." );

		if ( SettingsData.GetNetworkingEnabled() ) {
			Console.PrintLine( "Networking enabled, creating co-op lobby..." );

			GameConfiguration.GameMode = GameMode.Online;

			SteamLobby.Instance.SetMaxMembers( 4 );
			string name = SteamManager.GetSteamName();
			if ( name[ name.Length - 1 ] == 's' ) {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}' Lobby", name ) );
			} else {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}'s Lobby", name ) );
			}

			SteamLobby.Instance.CreateLobby();
		} else {
			GameConfiguration.GameMode = GameMode.SinglePlayer;
		}

		World.LoadTime = Stopwatch.StartNew();

		FinishedLoading += () => {
			LoadThread.Join();
			GetTree().ChangeSceneToPacked( LoadedWorld );
		};
		LoadThread = new Thread( () => {
			ArchiveSystem.LoadGame( SettingsData.GetSaveSlot() );
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/world.tscn" );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	private void OnContinueGameButtonPressed() {
		if ( MainMenu.Loaded ) {
			return;
		}
		MainMenu.Loaded = true;

		EmitSignalBeginGame();

		UIAudioManager.OnActivate();

		Hide();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnContinueGameFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		UIAudioManager.FadeMusic();
	}

	private void OnSaveSlotButtonPressed( Button SaveSlot ) {
		if ( MainMenu.Loaded ) {
			return;
		}
		MainMenu.Loaded = true;

		EmitSignalBeginGame();

		UIAudioManager.OnActivate();

		SettingsData.SetSaveSlot( (int)SaveSlot.GetMeta( "index" ) );

		Hide();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnContinueGameFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		UIAudioManager.FadeMusic();
	}

	private void OnBeginGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnBeginGameFinished ) );
		QueueFree();
		GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
	}
	private void OnNewGameButtonPressed() {
		if ( MainMenu.Loaded ) {
			return;
		}
		MainMenu.Loaded = true;

		int SlotIndex = 0;
		while ( ArchiveSystem.SlotExists( SlotIndex ) ) {
			SlotIndex++;
		}
		SettingsData.SetSaveSlot( SlotIndex );

		TutorialsPopup.Show();
	}

	private List<string> GetSaveSlotList( string directory ) {
		List<string> saveSlotList = new List<string>();

		DirAccess dir = DirAccess.Open( directory );
		if ( dir != null ) {
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while ( fileName.Length > 0 ) {
				if ( fileName.GetExtension() != "ngd" ) {
					fileName = dir.GetNext();
					continue;
				}
				saveSlotList.Add( fileName );
				fileName = dir.GetNext();
			}
		} else {
			Console.PrintError( string.Format( "An error occurred when trying to access path \"{0}\"", directory ) );
		}

		return saveSlotList;
	}
	private void OnLoadGameButtonPresed() {
		List<string> saveSlots = GetSaveSlotList( "user://SaveData" );
		Console.PrintLine( string.Format( "Found {0} save slots.", saveSlots.Count ) );

		// clear out the children first
		for ( int i = 0; i < SlotsContainer.GetChildCount(); i++ ) {
			SlotsContainer.CallDeferred( "remove_child", SlotsContainer.GetChild( i ) );
			SlotsContainer.GetChild( i ).CallDeferred( "queue_free" );
		}

		for ( int i = 0; i < saveSlots.Count; i++ ) {
			Button label = new Button();
			label.Text = saveSlots[ i ];
			label.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( label ); } ) );
			label.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( label ); } ) );
			label.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( label ); } ) );
			label.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( label ); } ) );
			label.Connect( "pressed", Callable.From( () => { OnSaveSlotButtonPressed( label ); } ) );
			label.SetMeta( "index", i );
			label.Show();
			SlotsContainer.AddChild( label );
		}

		SlotsContainer.Show();
		NewGameButton.Hide();
		ContinueGameButton.Hide();
	}

	private void OnButtonFocused( Button button ) {
		UIAudioManager.OnButtonFocused();

		button.Modulate = MainMenu.Selected;
	}
	private void OnButtonUnfocused( Button button ) {
		button.Modulate = MainMenu.Unselected;
	}

	public override void _Ready() {
		base._Ready();

		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		bool bHasSaveData = DirAccess.DirExistsAbsolute( ProjectSettings.GlobalizePath( "user://SaveData" ) );

		ContinueGameButton = GetNode<Button>( "MainContainer/OptionsContainer/ContinueGameButton" );
		if ( bHasSaveData ) {
			ContinueGameButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( ContinueGameButton ); } ) );
			ContinueGameButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( ContinueGameButton ); } ) );
			ContinueGameButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( ContinueGameButton ); } ) );
			ContinueGameButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( ContinueGameButton ); } ) );
			ContinueGameButton.Connect( "pressed", Callable.From( OnContinueGameButtonPressed ) );
		} else {
			ContinueGameButton.Modulate = new Color( 0.60f, 0.60f, 0.60f, 1.0f );
		}

		LoadGameButton = GetNode<Button>( "MainContainer/OptionsContainer/LoadGameButton" );
		if ( bHasSaveData ) {
			LoadGameButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( LoadGameButton ); } ) );
			LoadGameButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( LoadGameButton ); } ) );
			LoadGameButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( LoadGameButton ); } ) );
			LoadGameButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( LoadGameButton ); } ) );
			LoadGameButton.Connect( "pressed", Callable.From( OnLoadGameButtonPresed ) );
		} else {
			LoadGameButton.Modulate = new Color( 0.60f, 0.60f, 0.60f, 1.0f );
		}

		NewGameButton = GetNode<Button>( "MainContainer/OptionsContainer/NewGameButton" );
		NewGameButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( NewGameButton ); } ) );
		NewGameButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( NewGameButton ); } ) );
		NewGameButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( NewGameButton ); } ) );
		NewGameButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( NewGameButton ); } ) );
		NewGameButton.Connect( "pressed", Callable.From( OnNewGameButtonPressed ) );

		SlotsContainer = GetNode<VBoxContainer>( "MainContainer/OptionsContainer/LoadGameButton/SaveSlotsContainer" );

		TutorialsPopup = GetNode<ConfirmationDialog>( "TutorialsPopup" );
		TutorialsPopup.Canceled += () => {
			SettingsData.SetTutorialsEnabled( false );

			EmitSignalBeginGame();

			UIAudioManager.OnActivate();

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnBeginGameFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
		};
		TutorialsPopup.Confirmed += () => {
			SettingsData.SetTutorialsEnabled( true );

			EmitSignalBeginGame();

			UIAudioManager.OnActivate();
			
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnBeginGameFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
		};
	}
};