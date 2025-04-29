using ChallengeMode;
using Godot;
using Steamworks;

public partial class ExtrasMenu : Control {
	private VScrollBar ChallengeModeOptions;
	private VScrollBar CoopOptions;

	private MultiplayerMenu MultiplayerMenu;
	private LobbyBrowser LobbyBrowser;
	private LobbyFactory LobbyFactory;

	private HBoxContainer OptionsScroll;

	private Button CoopButton;
	private Button MultiplayerButton;
	private Button ChallengeModeButton;

	private VBoxContainer MainContainer;

	private VBoxContainer ChallengeModeData;
	private int SelectedMapIndex = -1;

	private AudioStreamPlayer UIChannel;
	private Color FocusedColor = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private Color UnfocusedColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

	private void OnCoopButtonPressed() {
//		CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
//		MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
//		ChallengeModeButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;

		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		ChallengeModeData.Hide();
	}
	private void OnMultiplayerButtonPressed() {
		MainContainer.Hide();

		MultiplayerMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/multiplayer_menu.tscn" ).Instantiate<MultiplayerMenu>();

		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		AddChild( MultiplayerMenu );
		MultiplayerMenu.Show();
	}
	private void OnChallengeModeButtonPressed() {
		CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		ChallengeModeButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;

		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		OptionsScroll.Show();
		ChallengeModeOptions.Show();
	}

	public void Reset() {
		CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		ChallengeModeButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;

		MultiplayerMenu?.GetNode<LobbyBrowser>( "LobbyBrowser" ).ResetBrowser();

		OptionsScroll.Hide();
		ChallengeModeOptions.Hide();
		ChallengeModeData.Hide();
		MainContainer.Show();
	}

	private void OnChallengeModeMapSelected( Button button ) {
		SelectedMapIndex = (int)button.GetMeta( "MapIndex" );

		Label DescriptionLabel = ChallengeModeData.GetNode<Label>( "DescriptionLabel" );
		RichTextLabel ObjectiveLabel = ChallengeModeData.GetNode<RichTextLabel>( "ObjectiveLabel" );

		int score, minutes, seconds, milliseconds;
		ChallengeCache.GetScore( SelectedMapIndex, out score, out minutes, out seconds, out milliseconds );

		Label BestTimeLabel = ChallengeModeData.GetNode<Label>( "ScoreContainer/BestTimeLabel" );
		BestTimeLabel.Text = string.Format( "{0}:{1}.{2}", minutes, seconds, milliseconds );

		Label ScoreLabel = ChallengeModeData.GetNode<Label>( "ScoreContainer/ScoreLabel" );
		ScoreLabel.Text = score.ToString();

//		ChallengeMap map = ChallengeCache.MapList[ SelectedMapIndex ];

		ChallengeModeData.Show();

		DescriptionLabel.Text = TranslationServer.Translate( string.Format( "CHALLENGE{0}_DESCRIPTION", SelectedMapIndex ) );
		ObjectiveLabel.ParseBbcode( string.Format( "(OBJECTIVE) [i]{0}[/i]", TranslationServer.Translate( string.Format( "CHALLENGE{0}_NAME", SelectedMapIndex ) ) ) );
	}
	private void OnStartChallengeButtonPressed() {
		if ( SelectedMapIndex < 0 || SelectedMapIndex >= ChallengeCache.MapList.Count ) {
			Console.PrintWarning( "ExtrasMenu.OnStartChallengeButtonPressed: invalid SelectedMapIndex" );
			return;
		}
	}

	private void OnElementFocused( Control element ) {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();

		element.Modulate = FocusedColor;
	}
	private void OnElementUnfocused( Control element ) {
		element.Modulate = UnfocusedColor;
	}

	public override void _Ready() {
		base._Ready();

		ChallengeCache.Init();

		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		MainContainer = GetNode<VBoxContainer>( "MainContainer" );

		CoopButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/CoopButton" );
		CoopButton.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( CoopButton ); } ) );
		CoopButton.Connect( "focus_entered", Callable.From( () => { OnElementFocused( CoopButton ); } ) );
		CoopButton.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( CoopButton ); } ) );
		CoopButton.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( CoopButton ); } ) );
		CoopButton.Connect( "pressed", Callable.From( OnCoopButtonPressed ) );
		
		MultiplayerButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/MultiplayerButton" );
		MultiplayerButton.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( MultiplayerButton ); } ) );
		MultiplayerButton.Connect( "focus_entered", Callable.From( () => { OnElementFocused( MultiplayerButton ); } ) );
		MultiplayerButton.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( MultiplayerButton ); } ) );
		MultiplayerButton.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( MultiplayerButton ); } ) );
		MultiplayerButton.Connect( "pressed", Callable.From( OnMultiplayerButtonPressed ) );

		ChallengeModeButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/ChallengeModeButton" );
		ChallengeModeButton.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( ChallengeModeButton ); } ) );
		ChallengeModeButton.Connect( "focus_entered", Callable.From( () => { OnElementFocused( ChallengeModeButton ); } ) );
		ChallengeModeButton.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( ChallengeModeButton ); } ) );
		ChallengeModeButton.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( ChallengeModeButton ); } ) );
		ChallengeModeButton.Connect( "pressed", Callable.From( OnChallengeModeButtonPressed ) );

		OptionsScroll = GetNode<HBoxContainer>( "MainContainer/HSplitContainer/HBoxContainer" );

		ChallengeModeOptions = GetNode<VScrollBar>( "MainContainer/HSplitContainer/HBoxContainer/ChallengeModeOptions" );
		if ( ChallengeModeOptions.GetChildCount() == 0 ) {
			for ( int i = 0; i < ChallengeCache.MapList.Count; i++ ) {
				Button button = new Button();
				button.Text = TranslationServer.Translate( string.Format( "CHALLENGE{0}_NAME", i ) );
				button.SetMeta( "MapIndex", i );
				button.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( button ); } ) );
				button.Connect( "focus_entered", Callable.From( () => { OnElementFocused( button ); } ) );
				button.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( button ); } ) );
				button.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( button ); } ) );
				button.Connect( "pressed", Callable.From( () => { OnChallengeModeMapSelected( button ); } ) );
				( ChallengeModeOptions.GetChild( 0 ) as VBoxContainer ).AddChild( button );
			}
		}

		ChallengeModeData = GetNode<VBoxContainer>( "MainContainer/ChallengeInfoContainer" );

		Button StartChallengeButton = ChallengeModeData.GetNode<Button>( "StartButton" );
		StartChallengeButton.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( StartChallengeButton ); } ) );
		StartChallengeButton.Connect( "focus_entered", Callable.From( () => { OnElementFocused( StartChallengeButton ); } ) );
		StartChallengeButton.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( StartChallengeButton ); } ) );
		StartChallengeButton.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( StartChallengeButton ); } ) );
		StartChallengeButton.Connect( "pressed", Callable.From( OnStartChallengeButtonPressed ) );

		CoopOptions = GetNode<VScrollBar>( "MainContainer/HSplitContainer/HBoxContainer/CoopOptions" );

		UIChannel = GetNode<AudioStreamPlayer>( "../UIChannel" );
	}
};
