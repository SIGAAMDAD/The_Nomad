using Godot;

public partial class PauseMenu : CanvasLayer {
	private ConfirmationDialog ConfirmExitDlg;
	private ConfirmationDialog ConfirmQuitDlg;

	private AudioStreamPlayer UIChannel;

	private TabContainer TabContainer;

	[Signal]
	public delegate void GamePausedEventHandler();

	[Signal]
	public delegate void LeaveLobbyEventHandler();

	private enum TabSelect {
		Backpack,
		Journal,
		Equipment,
		Options
	};
	
	private void OnConfirmExitConfirmed() {
		GameConfiguration.Paused = false;

		if ( GameConfiguration.GameMode == GameMode.SinglePlayer || GameConfiguration.GameMode == GameMode.Online ) {
			ArchiveSystem.SaveGame( SettingsData.GetSaveSlot() );
		}

		GetTree().Paused = false;
		Engine.TimeScale = 1.0f;
		ArchiveSystem.Clear();

		SteamLobby.Instance.SetPhysicsProcess( false );

		if ( GameConfiguration.GameMode == GameMode.Multiplayer || GameConfiguration.GameMode == GameMode.Online ) {
			EmitSignalLeaveLobby();
		}
		GetTree().ChangeSceneToFile( "res://scenes/main_menu.tscn" );
	}

	private void OnToggled() => Visible = !Visible;

	public override void _Ready() {
		base._Ready();

		ConfirmExitDlg = GetNode<ConfirmationDialog>( "ConfirmExit" );
		ConfirmExitDlg.Connect( "confirmed", Callable.From( OnConfirmExitConfirmed ) );
		ConfirmExitDlg.Connect( "canceled", Callable.From( ConfirmExitDlg.Hide ) );

		TabContainer = GetNode<TabContainer>( "MarginContainer/TabContainer" );
		TabContainer.Connect( "tab_clicked", Callable.From<int>(
			( selected ) => {
				UIAudioManager.OnButtonPressed();
				bool paused = selected == (int)TabSelect.Options;

				if ( GameConfiguration.GameMode != GameMode.Multiplayer ) {
					GetTree().Paused = paused;
					Engine.TimeScale = paused ? 0.0f : 1.0f;
				}
				if ( paused ) {
					Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://cursor_n.png" ) );
					EmitSignalGamePaused();
				} else {
					Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ) );
				}
			}
		) );

		ConfirmQuitDlg = GetNode<ConfirmationDialog>( "ConfirmQuit" );
		ConfirmQuitDlg.Connect( "confirmed", Callable.From( () => {
			if ( GameConfiguration.GameMode == GameMode.SinglePlayer || GameConfiguration.GameMode == GameMode.Online ) {
				ArchiveSystem.SaveGame( SettingsData.GetSaveSlot() );
			}

			GetTree().Quit();
		} ) );
		ConfirmQuitDlg.Connect( "canceled", Callable.From( ConfirmQuitDlg.Hide ) );

		Button ResumeButton = GetNode<Button>( "MarginContainer/TabContainer/Options/VBoxContainer/ResumeButton" );
		ResumeButton.Connect( "pressed", Callable.From( OnToggled ) );

		Button ExitToMainMenuButton = GetNode<Button>( "MarginContainer/TabContainer/Options/VBoxContainer/ExitToMainMenuButton" );
		ExitToMainMenuButton.Connect( "pressed", Callable.From( ConfirmExitDlg.Show ) );

		Button ExitGameButton = GetNode<Button>( "MarginContainer/TabContainer/Options/VBoxContainer/ExitGameButton" );
		ExitGameButton.Connect( "pressed", Callable.From( ConfirmQuitDlg.Show ) );

		ProcessMode = ProcessModeEnum.Always;

		switch ( GameConfiguration.GameMode ) {
		case GameMode.SinglePlayer:
		case GameMode.Online:
		case GameMode.JohnWick:
		case GameMode.ChallengeMode:
		case GameMode.LocalCoop2:
		case GameMode.LocalCoop3:
		case GameMode.LocalCoop4:
			ConfirmExitDlg.Set( "dialogue_text", "Are you sure you want to quit?" );
			ConfirmQuitDlg.Set( "dialogue_text", "Are you sure you want to quit?" );
			break;
		case GameMode.Multiplayer:
			ConfirmExitDlg.Set( "dialogue_text", "Are you sure?" );
			ConfirmQuitDlg.Set( "dialogue_text", "Are you sure?" );
			break;
		};

		Input.JoyConnectionChanged += OnJoyConnectionChanged;
	}
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( Input.IsActionJustReleased( "ui_exit" ) ) {
			CallDeferred( MethodName.OnToggled );
		}
	}

	private void OnJoyConnectionChanged( long device, bool connected ) {
		TabContainer.CallDeferred( MethodName.EmitSignal, TabContainer.SignalName.TabSelected, (int)TabSelect.Options );
	}
};
