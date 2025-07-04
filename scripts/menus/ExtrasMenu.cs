using System.Collections.Generic;
using ChallengeMode;
using Godot;
using Steamworks;

public partial class ExtrasMenu : Control {
	private VScrollBar StoryModeOptions;
	private VScrollBar CoopOptions;

	private MultiplayerMenu MultiplayerMenu;

	private HBoxContainer OptionsScroll;

	private Button CoopButton;
	private Button MultiplayerButton;
	private Button StoryModeButton;

	private VBoxContainer MainContainer;

	private VBoxContainer StoryModeData;
	private int SelectedMapIndex = -1;

	private Tween AudioFade;

	private VBoxContainer Leaderboard;
	private HBoxContainer LeaderboardData;
	private List<HBoxContainer> LeaderboardEntries;
	private Label FetchingLeaderboard;

	private Color FocusedColor = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private Color UnfocusedColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

	private void OnCoopButtonPressed() {
//		CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
//		MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
//		StoryModeButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;

		UIAudioManager.OnButtonPressed();

		StoryModeData.Hide();
	}
	private void OnMultiplayerButtonPressed() {
		MainContainer.Hide();

		MultiplayerMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/multiplayer_menu.tscn" ).Instantiate<MultiplayerMenu>();

		SteamLobby.Instance.SetPhysicsProcess( true );

		UIAudioManager.OnButtonPressed();

		AddChild( MultiplayerMenu );
		MultiplayerMenu.Show();
	}
	private void OnStoryModeButtonPressed() {
		CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		StoryModeButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;

		UIAudioManager.OnButtonPressed();

		OptionsScroll.Show();
		StoryModeOptions.Show();
	}

	public void Reset() {
		CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		StoryModeButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;

		SteamLobby.Instance.SetPhysicsProcess( false );

		OptionsScroll.Hide();
		StoryModeOptions.Hide();
		StoryModeData.Hide();
		MainContainer.Show();
	}

	private void ClearLeaderboard() {
		for ( int i = 0; i < LeaderboardEntries.Count; i++ ) {
			Leaderboard.RemoveChild( LeaderboardEntries[i] );
		}
		LeaderboardEntries.Clear();
	}
	private void FetchLevelLeaderboardStats( Dictionary<int, ChallengeCache.LeaderboardEntry> entries ) {
		Console.PrintLine( string.Format( "Found {0} entries in leaderboard.", entries.Count ) );

		FetchingLeaderboard.Hide();
		StoryModeData.GetNode<VScrollBar>( "LeaderboardScroll" ).Show();

		foreach ( var entry in entries ) {
			HBoxContainer container = LeaderboardData.Duplicate() as HBoxContainer;
			container.GetNode<Label>( "NameLabel" ).Text = SteamFriends.GetFriendPersonaName( entry.Value.UserID );
			container.GetNode<Label>( "ScoreLabel" ).Text = entry.Value.Score.ToString();
			container.GetNode<Label>( "TimeMinutesLabel" ).Text = entry.Value.TimeCompletedMinutes.ToString();
			container.GetNode<Label>( "TimeSecondsLabel" ).Text = entry.Value.TimeCompletedSeconds.ToString();
			container.GetNode<Label>( "TimeMillisecondsLabel" ).Text = entry.Value.TimeCompletedMillseconds.ToString();
			container.Show();
			LeaderboardEntries.Add( container );
			Leaderboard.AddChild( container );
		}
	}
	private void OnStoryModeMapSelected( Button button ) {
		SelectedMapIndex = (int)button.GetMeta( "MapIndex" );

		Label DescriptionLabel = StoryModeData.GetNode<Label>( "DescriptionLabel" );
		RichTextLabel ObjectiveLabel = StoryModeData.GetNode<RichTextLabel>( "ObjectiveLabel" );

		ClearLeaderboard();

		FetchingLeaderboard.Show();
		StoryModeData.GetNode<VScrollBar>( "LeaderboardScroll" ).Show();
		ChallengeCache.GetScore( SelectedMapIndex, out int score, out int minutes, out int seconds, out int milliseconds, new System.Action<Dictionary<int, ChallengeCache.LeaderboardEntry>>( FetchLevelLeaderboardStats ) );

		Label BestTimeLabel = StoryModeData.GetNode<Label>( "ScoreContainer/BestTimeLabel" );
		BestTimeLabel.Text = string.Format( "{0}:{1}.{2}", minutes, seconds, milliseconds );

		Label ScoreLabel = StoryModeData.GetNode<Label>( "ScoreContainer/ScoreLabel" );
		ScoreLabel.Text = score.ToString();

//		ChallengeMap map = ChallengeCache.MapList[ SelectedMapIndex ];

		StoryModeData.Show();

		DescriptionLabel.Text = TranslationServer.Translate( string.Format( "CHALLENGE{0}_DESCRIPTION", SelectedMapIndex ) );
		ObjectiveLabel.ParseBbcode( string.Format( "(OBJECTIVE) [i]{0}[/i]", TranslationServer.Translate( string.Format( "CHALLENGE{0}_OBJECTIVE", SelectedMapIndex ) ) ) );
	}

	private void OnStoryModeMapFinishedLoading( PackedScene mapData, Resource quest ) {
		Resource questData = Questify.Instantiate( quest );
		ChallengeCache.SetQuestData( questData );

		GameConfiguration.GameMode = GameMode.ChallengeMode;

		QueueFree();
		GetTree().ChangeSceneToPacked( mapData );
	}
	private void OnAudioFadeFinished() {
		GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
		AudioFade.Finished -= OnAudioFadeFinished;
	}
	private void OnStoryModeFadeFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnStoryModeFadeFinished ) );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );
		
		ChallengeCache.SetCurrentMap( SelectedMapIndex );
		ChallengeCache.MapList[ SelectedMapIndex ].Load( OnStoryModeMapFinishedLoading );
	}
	private void OnStartChallengeButtonPressed() {
		if ( SelectedMapIndex < 0 || SelectedMapIndex >= ChallengeCache.MapList.Count ) {
			Console.PrintWarning( "ExtrasMenu.OnStartChallengeButtonPressed: invalid SelectedMapIndex" );
			return;
		}

		Console.PrintLine( string.Format( "Loading story mode map {0}...", ChallengeCache.MapList[ SelectedMapIndex ].MapName ) );

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIAudioManager.OnActivate();

		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnStoryModeFadeFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
	}

	private void OnElementFocused( Control element ) {
		UIAudioManager.OnButtonFocused();

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

		StoryModeButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/StoryModeButton" );
		StoryModeButton.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( StoryModeButton ); } ) );
		StoryModeButton.Connect( "focus_entered", Callable.From( () => { OnElementFocused( StoryModeButton ); } ) );
		StoryModeButton.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( StoryModeButton ); } ) );
		StoryModeButton.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( StoryModeButton ); } ) );
		StoryModeButton.Connect( "pressed", Callable.From( OnStoryModeButtonPressed ) );

		OptionsScroll = GetNode<HBoxContainer>( "MainContainer/HSplitContainer/HBoxContainer" );

		StoryModeOptions = GetNode<VScrollBar>( "MainContainer/HSplitContainer/HBoxContainer/StoryModeOptions" );
		if ( StoryModeOptions.GetChild( 0 ).GetChildCount() == 0 ) {
			for ( int i = 0; i < ChallengeCache.MapList.Count; i++ ) {
				Button button = new Button();
				button.Text = TranslationServer.Translate( string.Format( "CHALLENGE{0}_NAME", i ) );
				button.SetMeta( "MapIndex", i );
				button.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( button ); } ) );
				button.Connect( "focus_entered", Callable.From( () => { OnElementFocused( button ); } ) );
				button.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( button ); } ) );
				button.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( button ); } ) );
				button.Connect( "pressed", Callable.From( () => { OnStoryModeMapSelected( button ); } ) );
				( StoryModeOptions.GetChild( 0 ) as VBoxContainer ).AddChild( button );
			}
		}

		StoryModeData = GetNode<VBoxContainer>( "MainContainer/StoryInfoContainer" );

		FetchingLeaderboard  = StoryModeData.GetNode<Label>( "FetchingLabel" );
		LeaderboardData = StoryModeData.GetNode<HBoxContainer>( "LeaderboardScroll/Leaderboard/HBoxContainer" );
		Leaderboard = StoryModeData.GetNode<VBoxContainer>( "LeaderboardScroll/Leaderboard" );
		LeaderboardEntries = new List<HBoxContainer>();

		Button StartChallengeButton = StoryModeData.GetNode<Button>( "StartButton" );
		StartChallengeButton.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( StartChallengeButton ); } ) );
		StartChallengeButton.Connect( "focus_entered", Callable.From( () => { OnElementFocused( StartChallengeButton ); } ) );
		StartChallengeButton.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( StartChallengeButton ); } ) );
		StartChallengeButton.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( StartChallengeButton ); } ) );
		StartChallengeButton.Connect( "pressed", Callable.From( OnStartChallengeButtonPressed ) );

		CoopOptions = GetNode<VScrollBar>( "MainContainer/HSplitContainer/HBoxContainer/CoopOptions" );

		Reset();
	}
};
