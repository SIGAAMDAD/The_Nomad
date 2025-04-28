using Godot;

public partial class ExtrasMenu : Control {
	private VScrollBar ChallengeModeOptions;
	private VScrollBar CoopOptions;

	private MultiplayerMenu MultiplayerMenu;
	private LobbyBrowser LobbyBrowser;
	private LobbyFactory LobbyFactory;

	private HBoxContainer OptionsScroll;

	private Button MultiplayerButton;
	private Button CoopButton;

	private VBoxContainer MainContainer;

	private void OnCoopButtonPressed() {
	}
	private void OnMultiplayerButtonPressed() {
		MainContainer.Hide();

		MultiplayerMenu.Show();
		LobbyBrowser.Show();
		LobbyFactory.Hide();
	}

	public void Reset() {
		LobbyBrowser.ResetBrowser();
		MultiplayerMenu.Hide();

		MainContainer.Show();
	}

	public override void _Ready() {
		base._Ready();

		MainContainer = GetNode<VBoxContainer>( "MainContainer" );

		MultiplayerMenu = GetNode<MultiplayerMenu>( "MultiplayerMenu" );
		LobbyFactory = GetNode<LobbyFactory>( "MultiplayerMenu/LobbyFactory" );
		LobbyBrowser = GetNode<LobbyBrowser>( "MultiplayerMenu/LobbyBrowser" );

		CoopButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/CoopButton" );
		CoopButton.Connect( "pressed", Callable.From( OnCoopButtonPressed ) );
		
		MultiplayerButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/MultiplayerButton" );
		MultiplayerButton.Connect( "pressed", Callable.From( OnMultiplayerButtonPressed ) );

		Button ChallengeModeButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/ChallengeModeButton" );

		OptionsScroll = GetNode<HBoxContainer>( "MainContainer/HSplitContainer/HBoxContainer" );

		ChallengeModeOptions = GetNode<VScrollBar>( "MainContainer/HSplitContainer/HBoxContainer/ChallengeModeOptions" );
		CoopOptions = GetNode<VScrollBar>( "MainContainer/HSplitContainer/HBoxContainer/CoopOptions" );
	}
};
