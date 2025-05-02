using Godot;

public partial class PauseMenu : Control {
	private ConfirmationDialog ConfirmExitDlg;
	private ConfirmationDialog ConfirmQuitDlg;
	private ColorRect ConfirmDlgOverlay;

	private PackedScene MainMenu;

	[Signal]
	public delegate void LeaveLobbyEventHandler();

	private void Pause() {
		if ( GameConfiguration.Paused ) {
			Hide();
			if ( !( SteamLobby.Instance.LobbyMemberCount <= 1 ) ) {
				GetTree().Paused = false;
			}
			Engine.TimeScale = 1.0f;
			Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://textures/hud/crosshairs/crosshairi.tga" ) );
		} else {
			Show();
			if ( !( SteamLobby.Instance.LobbyMemberCount <= 1 ) ) {
				GetTree().Paused = true;
			}
			Input.SetCustomMouseCursor( ResourceCache.GetTexture( "res://cursor_n.png" ) );
			Engine.TimeScale = 0.0f;
		}
		GameConfiguration.Paused = !GameConfiguration.Paused;
	}
	private void OnConfirmExitConfirmed() {
		GameConfiguration.Paused = false;

		if ( GameConfiguration.GameMode == GameMode.SinglePlayer || GameConfiguration.GameMode == GameMode.Online ) {
			ArchiveSystem.SaveGame( null, 0 );
		}

		GetTree().Paused = false;
		Engine.TimeScale = 1.0f;
		ArchiveSystem.Clear();

		if ( GameConfiguration.GameMode == GameMode.Multiplayer
			|| GameConfiguration.GameMode == GameMode.Online )
		{
			EmitSignalLeaveLobby();
		}
		GetTree().ChangeSceneToPacked( MainMenu );
	}

	public override void _Ready() {
		base._Ready();

		MainMenu = ResourceLoader.Load<PackedScene>( "res://scenes/main_menu.tscn" );

		ConfirmExitDlg = GetNode<ConfirmationDialog>( "ConfirmExit" );
		ConfirmExitDlg.Connect( "confirmed", Callable.From( OnConfirmExitConfirmed ) );
		ConfirmExitDlg.Connect( "canceled", Callable.From( () => {
			ConfirmExitDlg.Hide();
			ConfirmDlgOverlay.Hide();
		} ) );

		ConfirmQuitDlg = GetNode<ConfirmationDialog>( "ConfirmQuit" );
		ConfirmQuitDlg.Connect( "confirmed", Callable.From( () => {
			if ( GameConfiguration.GameMode == GameMode.SinglePlayer || GameConfiguration.GameMode == GameMode.Online ) {
				ArchiveSystem.SaveGame( null, 0 );
			}

			ConfirmDlgOverlay.Hide();
			GetTree().Quit();
		} ) );
		ConfirmQuitDlg.Connect( "canceled", Callable.From( () => {
			ConfirmQuitDlg.Hide();
			ConfirmDlgOverlay.Hide();
		} ) );

		ConfirmDlgOverlay = GetNode<ColorRect>( "ColorRect2" );
		ConfirmDlgOverlay.SetProcess( false );
		ConfirmDlgOverlay.SetProcessInternal( false );

		Button ResumeButton = GetNode<Button>( "MarginContainer/VBoxContainer/ResumeButton" );
		ResumeButton.SetProcess( false );
		ResumeButton.SetProcessInternal( false );
		ResumeButton.Connect( "pressed", Callable.From( Pause ) );

		Button ExitToMainMenuButton = GetNode<Button>( "MarginContainer/VBoxContainer/ExitToMainMenuButton" );
		ExitToMainMenuButton.SetProcess( false );
		ExitToMainMenuButton.SetProcessInternal( false );
		ExitToMainMenuButton.Connect( "pressed", Callable.From( () => {
			ConfirmExitDlg.Show();
			ConfirmDlgOverlay.Show();
		} ) );

		Button ExitGameButton = GetNode<Button>( "MarginContainer/VBoxContainer/ExitGameButton" );
		ExitGameButton.SetProcess( false );
		ExitGameButton.SetProcessInternal( false );
		ExitGameButton.Connect( "pressed", Callable.From( () => {
			ConfirmQuitDlg.Show();
			ConfirmDlgOverlay.Show();
		} ) );

		ProcessMode = ProcessModeEnum.Always;

		Input.JoyConnectionChanged += OnJoyConnectionChanged;
	}

	private void OnJoyConnectionChanged( long device, bool connected ) {
		if ( !connected ) {
			CallDeferred( "Pause" );
		}
	}

	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );
		
		if ( @event.IsActionPressed( "ui_exit" ) ) {
			CallDeferred( "Pause" );
		}
	}
};
