/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System.Collections.Generic;
using ChallengeMode;
using Godot;
using Steamworks;
using Steam;

namespace Menus {
	/*
	===================================================================================

	ExtrasMenu

	===================================================================================
	*/

	public partial class ExtrasMenu : Control {
		private VScrollBar StoryModeOptions;
		private VScrollBar CoopOptions;

		private MultiplayerMenu MultiplayerMenu;

		private HBoxContainer OptionsScroll;

		private Button CoopButton;
		private Button MultiplayerButton;
		private Button DeveloperCommentaryButton;

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

		/*
		===============
		OnCoopButtonPressed
		===============
		*/
		private void OnCoopButtonPressed() {
			//		CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
			//		MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
			//		StoryModeButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;

			UIAudioManager.OnButtonPressed();

			StoryModeData.Hide();
		}

		/*
		===============
		OnMultiplayerButtonPressed
		===============
		*/
		private void OnMultiplayerButtonPressed() {
			MainContainer.Hide();

			MultiplayerMenu ??= ResourceLoader.Load<PackedScene>( "res://scenes/menus/multiplayer_menu.tscn" ).Instantiate<MultiplayerMenu>();

			SteamLobby.Instance.SetPhysicsProcess( true );

			UIAudioManager.OnButtonPressed();

			AddChild( MultiplayerMenu );
			MultiplayerMenu.Show();
		}

		/*
		===============
		OnStoryModeButtonPressed
		===============
		*/
		private void OnStoryModeButtonPressed() {
			CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
			MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
			DeveloperCommentaryButton.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;

			UIAudioManager.OnButtonPressed();

			OptionsScroll.Show();
			StoryModeOptions.Show();
		}

		/*
		===============
		Reset
		===============
		*/
		public void Reset() {
			CoopButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
			MultiplayerButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
			DeveloperCommentaryButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;

			SteamLobby.Instance.SetPhysicsProcess( false );

			OptionsScroll.Hide();
			StoryModeOptions.Hide();
			StoryModeData.Hide();
			MainContainer.Show();
		}

		/*
		===============
		ClearLeaderboard
		===============
		*/
		private void ClearLeaderboard() {
			for ( int i = 0; i < LeaderboardEntries.Count; i++ ) {
				Leaderboard.RemoveChild( LeaderboardEntries[ i ] );
			}
			LeaderboardEntries.Clear();
		}

		/*
		===============
		FetchLevelLeaderboardStats
		===============
		*/
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

		/*
		===============
		OnStoryModeMapSelected
		===============
		*/
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

			StoryModeData.Show();

			DescriptionLabel.Text = TranslationServer.Translate( string.Format( "CHALLENGE{0}_DESCRIPTION", SelectedMapIndex ) );
			ObjectiveLabel.ParseBbcode( string.Format( "(OBJECTIVE) [i]{0}[/i]", TranslationServer.Translate( string.Format( "CHALLENGE{0}_OBJECTIVE", SelectedMapIndex ) ) ) );
		}

		/*
		===============
		OnStoryModeMapFinishedLoading
		===============
		*/
		private void OnStoryModeMapFinishedLoading( PackedScene mapData, Resource quest ) {
			Resource questData = Questify.Instantiate( quest );
			ChallengeCache.SetQuestData( questData );

			GameConfiguration.GameMode = GameMode.ChallengeMode;

			QueueFree();
			GetTree().ChangeSceneToPacked( mapData );
		}

		/*
		===============
		OnAudioFadeFinished
		===============
		*/
		private void OnAudioFadeFinished() {
			GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
			AudioFade.Finished -= OnAudioFadeFinished;
		}

		/*
		===============
		OnStoryModeFadeFinished
		===============
		*/
		private void OnStoryModeFadeFinished() {
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnStoryModeFadeFinished ) );

			// FIXME?
			Hide();
			GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( "" );

			ChallengeCache.SetCurrentMap( SelectedMapIndex );
			ChallengeCache.MapList[ SelectedMapIndex ].Load( OnStoryModeMapFinishedLoading );
		}

		/*
		===============
		OnStartChallengeButtonPressed
		===============
		*/
		private void OnStartChallengeButtonPressed() {
			if ( SelectedMapIndex < 0 || SelectedMapIndex >= ChallengeCache.MapList.Count ) {
				Console.PrintWarning( "ExtrasMenu.OnStartChallengeButtonPressed: invalid SelectedMapIndex" );
				return;
			}

			Console.PrintLine( string.Format( "Loading story mode map {0}...", ChallengeCache.MapList[ SelectedMapIndex ].MapName ) );

			AudioFade = GetTree().Root.CreateTween();
			AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
			AudioFade.Connect( Tween.SignalName.Finished, Callable.From( OnAudioFadeFinished ) );

			UIAudioManager.OnActivate();

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnStoryModeFadeFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			ChallengeCache.Init();

			Theme = SettingsData.DyslexiaMode ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

			MainContainer = GetNode<VBoxContainer>( "MainContainer" );

			CoopButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/CoopButton" );
			CoopButton.Connect( Button.SignalName.Pressed, Callable.From( OnCoopButtonPressed ) );

			MultiplayerButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/MultiplayerButton" );
			MultiplayerButton.Connect( Button.SignalName.Pressed, Callable.From( OnMultiplayerButtonPressed ) );

			DeveloperCommentaryButton = GetNode<Button>( "MainContainer/HSplitContainer/VBoxContainer/DeveloperCommentaryButton" );
			DeveloperCommentaryButton.Connect( Button.SignalName.Pressed, Callable.From( OnStoryModeButtonPressed ) );

			OptionsScroll = GetNode<HBoxContainer>( "MainContainer/HSplitContainer/HBoxContainer" );

			StoryModeOptions = GetNode<VScrollBar>( "MainContainer/HSplitContainer/HBoxContainer/StoryModeOptions" );
			if ( StoryModeOptions.GetChild( 0 ).GetChildCount() == 0 ) {
				for ( int i = 0; i < ChallengeCache.MapList.Count; i++ ) {
					Button button = new Button();
					button.Text = TranslationServer.Translate( string.Format( "CHALLENGE{0}_NAME", i ) );
					button.SetMeta( "MapIndex", i );
					button.Connect( Button.SignalName.Pressed, Callable.From( () => OnStoryModeMapSelected( button ) ) );
					( StoryModeOptions.GetChild( 0 ) as VBoxContainer ).AddChild( button );
				}
			}

			StoryModeData = GetNode<VBoxContainer>( "MainContainer/StoryInfoContainer" );

			FetchingLeaderboard = StoryModeData.GetNode<Label>( "FetchingLabel" );
			LeaderboardData = StoryModeData.GetNode<HBoxContainer>( "LeaderboardScroll/Leaderboard/HBoxContainer" );
			Leaderboard = StoryModeData.GetNode<VBoxContainer>( "LeaderboardScroll/Leaderboard" );
			LeaderboardEntries = new List<HBoxContainer>();

			Button startChallengeButton = StoryModeData.GetNode<Button>( "StartButton" );
			startChallengeButton.Connect( Button.SignalName.Pressed, Callable.From( OnStartChallengeButtonPressed ) );

			CoopOptions = GetNode<VScrollBar>( "MainContainer/HSplitContainer/HBoxContainer/CoopOptions" );

			Reset();
		}
	};
};