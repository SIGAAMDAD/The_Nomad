using System.Collections.Generic;
using ChallengeMode;
using Godot;
using Steamworks;

public partial class ExtrasMenu : Control {
	private class LeaderboardEntry {
		public readonly int Score = 0;
		public readonly int TimeCompletedMinutes = 0;
		public readonly int TimeCompletedSeconds = 0;
		public readonly int TimeCompletedMillseconds = 0;
		public readonly CSteamID UserID = CSteamID.Nil;
		
		public LeaderboardEntry( LeaderboardEntry_t entry, int[] details ) {
			UserID = entry.m_steamIDUser;
			Score = entry.m_nScore;

			if ( details.Length != 3 ) {
				Console.PrintError( "[STEAM] Invalid leaderboard entry data!" );
				return;
			}

			TimeCompletedMinutes = details[0];
			TimeCompletedSeconds = details[1];
			TimeCompletedMillseconds = details[2];
		}
	};

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

	/// <summary>
	/// data for the currently selected map
	/// </summary>
	private Dictionary<int, LeaderboardEntry> LeaderboardData = new Dictionary<int, LeaderboardEntry>();
	private SteamLeaderboardEntries_t LeaderboardEntries;	

	private Dictionary<StringName, SteamLeaderboard_t> Leaderboards;
	private CallResult<LeaderboardFindResult_t> OnLeaderboardFindResult;
	private CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloaded;
	private CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploaded;

	private void FetchLeaderboardData( SteamLeaderboard_t hLeaderboard ) {
		Console.PrintLine( "Found leaderboard." );

		int entryCount = SteamUserStats.GetLeaderboardEntryCount( hLeaderboard );

		SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries( hLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, entryCount );
		OnLeaderboardScoresDownloaded.Set( handle );
	}

	private void OnFindLeaderboard( LeaderboardFindResult_t pCallback, bool bIOFailure ) {
		if ( pCallback.m_bLeaderboardFound == 0 ) {
			Console.PrintError( "[STEAM] Error finding leaderboard!" );
			return;
		}
		Leaderboards.Add( SteamUserStats.GetLeaderboardName( pCallback.m_hSteamLeaderboard ), pCallback.m_hSteamLeaderboard );
	}
	private void OnScoreUploaded( LeaderboardScoreUploaded_t pCallback, bool bIOFailure ) {
		if ( pCallback.m_bSuccess == 0 ) {
			Console.PrintError( "[STEAM] Error uploading stats to steam leaderboards!" );
			return;
		}
	}
	private void OnScoreDownloaded( LeaderboardScoresDownloaded_t pCallback, bool bIOFailure ) {
		int[] details = new int[4];
		LeaderboardEntries = pCallback.m_hSteamLeaderboardEntries;

		LeaderboardData.Clear();
		for ( int i = 0; i < pCallback.m_cEntryCount; i++ ) {
			LeaderboardEntry_t entry;
			if ( !SteamUserStats.GetDownloadedLeaderboardEntry( LeaderboardEntries, i, out entry, details, details.Length ) ) {
				Console.PrintError( "[STEAM] Error fetching downloaded leaderboard entry!" );
				continue;
			}
			LeaderboardData.Add( entry.m_nGlobalRank, new LeaderboardEntry( entry, details ) );
		}
	}
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

		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		MultiplayerMenu.Show();
		LobbyBrowser.Show();
		LobbyFactory.Hide();
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

		LobbyBrowser.ResetBrowser();
		MultiplayerMenu.Hide();

		OptionsScroll.Hide();
		ChallengeModeOptions.Hide();
		ChallengeModeData.Hide();
		MainContainer.Show();
	}

	private void OnChallengeModeMapSelected( Button button ) {
		SelectedMapIndex = (int)button.GetMeta( "MapIndex" );
		FetchLeaderboardData( Leaderboards[ string.Format( "Challenge{0}", SelectedMapIndex ) ] );

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

		OnLeaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create( OnFindLeaderboard );
		OnLeaderboardScoreUploaded = CallResult<LeaderboardScoreUploaded_t>.Create( OnScoreUploaded );
		OnLeaderboardScoresDownloaded = CallResult<LeaderboardScoresDownloaded_t>.Create( OnScoreDownloaded );

		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		MainContainer = GetNode<VBoxContainer>( "MainContainer" );

		MultiplayerMenu = GetNode<MultiplayerMenu>( "MultiplayerMenu" );
		LobbyFactory = GetNode<LobbyFactory>( "MultiplayerMenu/LobbyFactory" );
		LobbyBrowser = GetNode<LobbyBrowser>( "MultiplayerMenu/LobbyBrowser" );

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
		for ( int i = 0; i < ChallengeCache.MapList.Count; i++ ) {
			Button button = new Button();
			button.Text = TranslationServer.Translate( string.Format( "CHALLENGE{0}_NAME", SelectedMapIndex ) );
			button.SetMeta( "MapIndex", i );
			button.Connect( "mouse_entered", Callable.From( () => { OnElementFocused( button ); } ) );
			button.Connect( "focus_entered", Callable.From( () => { OnElementFocused( button ); } ) );
			button.Connect( "mouse_exited", Callable.From( () => { OnElementUnfocused( button ); } ) );
			button.Connect( "focus_exited", Callable.From( () => { OnElementUnfocused( button ); } ) );
			button.Connect( "pressed", Callable.From( () => { OnChallengeModeMapSelected( button ); } ) );
			( ChallengeModeOptions.GetChild( 0 ) as VBoxContainer ).AddChild( button );

			SteamAPICall_t handle = SteamUserStats.FindLeaderboard( string.Format( "Challenge{0}", i ) );
			OnLeaderboardFindResult.Set( handle );
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
