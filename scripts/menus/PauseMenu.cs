using Godot;

public partial class PauseMenu : Control {
	private ConfirmationDialog ConfirmExitDlg;
	private ConfirmationDialog ConfirmQuitDlg;
	private ColorRect ConfirmDlgOverlay;

	[Signal]
	public delegate void LeaveLobbyEventHandler();

	private void Pause() {
		if ( GameConfiguration.Paused ) {
			Hide();
			if ( !( SteamLobby.Instance.LobbyMembers.Count <= 1 ) ) {
				GetTree().Paused = false;
			}
			Engine.TimeScale = 1.0f;
		} else {
			Show();
			if ( !( SteamLobby.Instance.LobbyMembers.Count <= 1 ) ) {
				GetTree().Paused = true;
			}
			Engine.TimeScale = 0.0f;
		}
		GameConfiguration.Paused = !GameConfiguration.Paused;
	}
	private void OnConfirmExitConfirmed() {
		GameConfiguration.Paused = false;

		Engine.TimeScale = 1.0f;
		GameConfiguration.LoadedLevel.Call( "UnloadScene" );
		GameConfiguration.LoadedLevel.Free();
		GameConfiguration.LoadedLevel = null;

		if ( GameConfiguration.GameMode == GameMode.Multiplayer
			|| GameConfiguration.GameMode == GameMode.Online )
		{
			EmitSignal( "LeaveLobby" );
		}
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
	}
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );
		
		if ( @event.IsActionPressed( "ui_exit" ) ) {
			CallDeferred( "Pause" );
		}
	}
};
