using Godot;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Steamworks;
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

	private bool SaveSlotMode = false;

	private Button[] ButtonList;
	private int ButtonIndex = 0;

	private Button DeleteSaveButton;

	[Signal]
	public delegate void BeginGameEventHandler();
	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void OnContinueGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnContinueGameFinished ) );

		Hide();
		GetNode<LoadingScreen>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Console.PrintLine( "Loading game..." );

		/*
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
		}
		*/
		GameConfiguration.GameMode = GameMode.SinglePlayer;

		World.LoadTime = Stopwatch.StartNew();

		FinishedLoading += () => {
			LoadThread.Join();
			GetTree().ChangeSceneToPacked( LoadedWorld );
		};
		LoadThread = new Thread( () => {
			ArchiveSystem.LoadGame( SettingsData.GetSaveSlot() );
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/world.tscn" );
			CallDeferred( MethodName.EmitSignal, SignalName.FinishedLoading );
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
	private void LoadSaveSlots() {
		List<string> saveSlots = GetSaveSlotList( "user://SaveData" );
		Console.PrintLine( string.Format( "Found {0} save slots.", saveSlots.Count ) );

		// clear out the children first
		for ( int i = 0; i < SlotsContainer.GetChildCount(); i++ ) {
			SlotsContainer.GetChild( i ).CallDeferred( MethodName.QueueFree );
			SlotsContainer.RemoveChild( SlotsContainer.GetChild( i ) );
		}

		ButtonIndex = 0;

		ButtonList = new Button[ saveSlots.Count ];
		for ( int i = 0; i < saveSlots.Count; i++ ) {
			Button label = new Button();

			label.Text = saveSlots[ i ];
			label.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( label ) ) );
			label.Connect( Button.SignalName.FocusExited, Callable.From( () => OnButtonUnfocused( label ) ) );
			label.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( label ) ) );
			label.Connect( Button.SignalName.MouseExited, Callable.From( () => OnButtonUnfocused( label ) ) );
			label.Connect( Button.SignalName.rPressed, Callable.From( () => OnSaveSlotButtonPressed( label ) ) );
			label.SetMeta( "index", i );
			label.Show();
			SlotsContainer.AddChild( label );

			ButtonList[ i ] = label;
		}
	}
	private void OnLoadGameButtonPresed() {
		SaveSlotMode = true;

		DeleteSaveButton.Show();

		LoadSaveSlots();

		SlotsContainer.Show();
		LoadGameButton.Hide();
		NewGameButton.Hide();
		ContinueGameButton.Hide();
	}
	private void OnDeleteSaveButtonPressed() {
		if ( !SaveSlotMode ) {
			Console.PrintWarning( "StoryMenu.OnDeleteSaveButtonPressed: called with invalid state" );
			return;
		}

		ArchiveSystem.DeleteSave( ButtonIndex );
		ButtonIndex = 0;

		LoadSaveSlots();
	}

	private void OnButtonFocused( Button button ) {
		UIAudioManager.OnButtonFocused();

		button.Modulate = MainMenu.Selected;

		if ( ButtonList[ ButtonIndex ] != button ) {
			OnButtonUnfocused( ButtonList[ ButtonIndex ] );
			ButtonIndex = (int)button.GetMeta( "index" );
		}
	}
	private void OnButtonUnfocused( Button button ) {
		if ( SaveSlotMode && ButtonIndex != (int)button.GetMeta( "index" ) ) {
			button.Modulate = MainMenu.Unselected;
			return;
		}
		button.Modulate = MainMenu.Unselected;
	}

	public override void _Ready() {
		base._Ready();

		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		bool bHasSaveData = DirAccess.DirExistsAbsolute( ProjectSettings.GlobalizePath( "user://SaveData" ) );

		ConfirmationDialog DeleteSaveConfirm = GetNode<ConfirmationDialog>( "DeleteSaveConfirmation" );
		DeleteSaveConfirm.Connect( "canceled", Callable.From( Hide ) );
		DeleteSaveConfirm.Connect( "confirmed", Callable.From( OnDeleteSaveButtonPressed ) );

		DeleteSaveButton = GetNode<Button>( "DeleteSaveButton" );
		DeleteSaveButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( DeleteSaveButton ) ) );
		DeleteSaveButton.Connect( Button.SignalName.FocusExited, Callable.From( () => OnButtonUnfocused( DeleteSaveButton ) ) );
		DeleteSaveButton.Connect( Button.SignalName.Pressed, Callable.From( DeleteSaveConfirm.Show ) );

		ContinueGameButton = GetNode<Button>( "MainContainer/OptionsContainer/ContinueGameButton" );
		if ( bHasSaveData ) {
			ContinueGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( ContinueGameButton ) ) );
			ContinueGameButton.Connect( Button.SignalName.FocusExited, Callable.From( () => OnButtonUnfocused( ContinueGameButton ) ) );
			ContinueGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( ContinueGameButton ) ) );
			ContinueGameButton.Connect( Button.SignalName.MouseExited, Callable.From( () => OnButtonUnfocused( ContinueGameButton ) ) );
			ContinueGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnContinueGameButtonPressed ) );
		} else {
			ContinueGameButton.Modulate = new Color( 0.60f, 0.60f, 0.60f, 1.0f );
		}
		ContinueGameButton.SetMeta( "index", 0 );

		LoadGameButton = GetNode<Button>( "MainContainer/OptionsContainer/LoadGameButton" );
		if ( bHasSaveData ) {
			LoadGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( LoadGameButton ) ) );
			LoadGameButton.Connect( Button.SignalName.FocusExited, Callable.From( () => OnButtonUnfocused( LoadGameButton ) ) );
			LoadGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( LoadGameButton ) ) );
			LoadGameButton.Connect( Button.SignalName.MouseExited, Callable.From( () => OnButtonUnfocused( LoadGameButton ) ) );
			LoadGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnLoadGameButtonPresed ) );
		} else {
			LoadGameButton.Modulate = new Color( 0.60f, 0.60f, 0.60f, 1.0f );
		}
		LoadGameButton.SetMeta( "index", 1 );

		NewGameButton = GetNode<Button>( "MainContainer/OptionsContainer/NewGameButton" );
		NewGameButton.Connect( Button.SignalName.FocusEntered, Callable.From( () => OnButtonFocused( NewGameButton ) ) );
		NewGameButton.Connect( Button.SignalName.FocusExited, Callable.From( () => OnButtonUnfocused( NewGameButton ) ) );
		NewGameButton.Connect( Button.SignalName.MouseEntered, Callable.From( () => OnButtonFocused( NewGameButton ) ) );
		NewGameButton.Connect( Button.SignalName.MouseExited, Callable.From( () => OnButtonUnfocused( NewGameButton ) ) );
		NewGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnNewGameButtonPressed ) );
		NewGameButton.SetMeta( "index", 2 );

		SlotsContainer = GetNode<VBoxContainer>( "MainContainer/OptionsContainer/SaveSlotsContainer" );

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

		ButtonList = [
			ContinueGameButton,
			LoadGameButton,
			NewGameButton
		];

		ContinueGameButton.Modulate = MainMenu.Selected;
	}
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( Input.IsActionJustPressed( "ui_down" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusExited );
			if ( ButtonIndex == ButtonList.Length - 1 ) {
				ButtonIndex = 0;
			} else {
				ButtonIndex++;
			}
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
		} else if ( Input.IsActionJustPressed( "ui_up" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusExited );
			if ( ButtonIndex == 0 ) {
				ButtonIndex = ButtonList.Length - 1;
			} else {
				ButtonIndex--;
			}
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
		} else if ( Input.IsActionJustPressed( "ui_accept" ) || Input.IsActionJustPressed( "ui_enter" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( Button.SignalName.FocusEntered );
			ButtonList[ ButtonIndex ].CallDeferred( Button.MethodName.EmitSignal, Button.SignalName.Pressed );
		}
	}
};