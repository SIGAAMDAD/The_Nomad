using Godot;

public partial class PauseMenu : CanvasLayer {
	private ConfirmationDialog ConfirmExitDlg;
	private ConfirmationDialog ConfirmQuitDlg;
	private ColorRect ConfirmDlgOverlay;

	private AudioStreamPlayer UIChannel;

	[Signal]
	public delegate void GamePausedEventHandler();

	[Signal]
	public delegate void LeaveLobbyEventHandler();

	private void Pause() {
		if ( GetTree().Paused ) {
			Engine.TimeScale = 1.0f;
			Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ) );
			UIChannel.ProcessMode = ProcessModeEnum.Always;
			UIChannel.Stream = ResourceCache.GetSound( "res://sounds/ui/resume_game.ogg" );
			UIChannel.Play();
		} else {
			Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://cursor_n.png" ) );
			Engine.TimeScale = 0.0f;
			UIChannel.ProcessMode = ProcessModeEnum.Always;
			UIChannel.Stream = ResourceCache.GetSound( "res://sounds/ui/pause_game.ogg" );
			UIChannel.Play();
		}
		if ( GameConfiguration.GameMode != GameMode.Multiplayer ) {
			GetTree().Paused = !GetTree().Paused;
		}
		Visible = !Visible;
		EmitSignalGamePaused();
	}
	private void OnConfirmExitConfirmed() {
		GameConfiguration.Paused = false;

		if ( GameConfiguration.GameMode == GameMode.SinglePlayer || GameConfiguration.GameMode == GameMode.Online ) {
			ArchiveSystem.SaveGame( SettingsData.GetSaveSlot() );
		}

		GetTree().Paused = false;
		Engine.TimeScale = 1.0f;
		ArchiveSystem.Clear();
		
		SteamLobby.Instance.SetPhysicsProcess( false );

		if ( GameConfiguration.GameMode == GameMode.Multiplayer
			|| GameConfiguration.GameMode == GameMode.Online )
		{
			EmitSignalLeaveLobby();
		}
		GetTree().ChangeSceneToFile( "res://scenes/main_menu.tscn" );
	}

	public override void _Ready() {
		base._Ready();

		ConfirmExitDlg = GetNode<ConfirmationDialog>( "ConfirmExit" );
		ConfirmExitDlg.Connect( "confirmed", Callable.From( OnConfirmExitConfirmed ) );
		ConfirmExitDlg.Connect( "canceled", Callable.From( () => {
			ConfirmExitDlg.Hide();
			ConfirmDlgOverlay.Hide();
		} ) );

		ConfirmQuitDlg = GetNode<ConfirmationDialog>( "ConfirmQuit" );
		ConfirmQuitDlg.Connect( "confirmed", Callable.From( () => {
			if ( GameConfiguration.GameMode == GameMode.SinglePlayer || GameConfiguration.GameMode == GameMode.Online ) {
				ArchiveSystem.SaveGame( SettingsData.GetSaveSlot() );
			}

			ConfirmDlgOverlay.Hide();
			GetTree().Quit();
		} ) );
		ConfirmQuitDlg.Connect( "canceled", Callable.From( () => {
			ConfirmQuitDlg.Hide();
			ConfirmDlgOverlay.Hide();
		} ) );

		ConfirmDlgOverlay = GetNode<ColorRect>( "ColorRect2" );

		Button ResumeButton = GetNode<Button>( "MarginContainer/VBoxContainer/ResumeButton" );
		ResumeButton.Connect( "pressed", Callable.From( Pause ) );

		Button ExitToMainMenuButton = GetNode<Button>( "MarginContainer/VBoxContainer/ExitToMainMenuButton" );
		ExitToMainMenuButton.Connect( "pressed", Callable.From( () => {
			ConfirmExitDlg.Show();
			ConfirmDlgOverlay.Show();
		} ) );

		Button ExitGameButton = GetNode<Button>( "MarginContainer/VBoxContainer/ExitGameButton" );
		ExitGameButton.Connect( "pressed", Callable.From( () => {
			ConfirmQuitDlg.Show();
			ConfirmDlgOverlay.Show();
		} ) );

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

		UIChannel = GetNode<AudioStreamPlayer>( "UIChannel" );
		UIChannel.Connect( "finished", Callable.From( () => { UIChannel.SetDeferred( "process_mode", (long)ProcessModeEnum.Disabled ); } ) );

		Input.JoyConnectionChanged += OnJoyConnectionChanged;
	}
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( Input.IsActionJustReleased( "ui_exit" ) ) {
			CallDeferred( "Pause" );
		}
	}

	private void OnJoyConnectionChanged( long device, bool connected ) {
		if ( !connected ) {
			CallDeferred( "Pause" );
		}
	}
};
