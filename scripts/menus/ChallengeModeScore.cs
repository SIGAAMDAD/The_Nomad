using ChallengeMode;
using Godot;

public partial class ChallengeModeScore : CanvasLayer {
	private Label MapNameLabel;
	private Label TotalScoreLabel;
	private Label HighestComboLabel;
	private Label DeathCounterLabel;

	private Label TimeMinutesLabel;
	private Label TimeSecondsLabel;
	private Label TimeMillisecondsLabel;

	private Label NoDeathsLabel;
	private Label NoDamageLabel;
	private Label FlawlessLabel;
	private Label LegendLabel;
	
	private Button ExitButton;

	private void OnExitButtonPressed() {
		GetTree().CallDeferred( "change_scene_to_file", "res://scenes/main_menu.tscn" );
	}

	public override void _Ready() {
		base._Ready();

		MapNameLabel = GetNode<Label>( "MarginContainer/VBoxContainer/MapNameLabel" );

		TotalScoreLabel = GetNode<Label>( "MarginContainer/VBoxContainer/TotalScoreContainer/TotalScoreLabel" );
		HighestComboLabel = GetNode<Label>( "MarginContainer/VBoxContainer/HighestComboContainer/HighestComboLabel" );

		DeathCounterLabel = GetNode<Label>( "MarginContainer/VBoxContainer/DeathCounterContainer/DeathCounterLabel" );

		TimeMinutesLabel = GetNode<Label>( "MarginContainer/VBoxContainer/TimeContainer/TimeMinutesLabel" );
		TimeSecondsLabel = GetNode<Label>( "MarginContainer/VBoxContainer/TimeContainer/TimeSecondsLabel" );
		TimeMillisecondsLabel = GetNode<Label>( "MarginContainer/VBoxContainer/TimeContainer/TimeMillisecondsLabel" );

		NoDeathsLabel = GetNode<Label>( "MarginContainer/VBoxContainer/ExtrasContainer/NoDeathsLabel" );	
		NoDamageLabel = GetNode<Label>( "MarginContainer/VBoxContainer/ExtrasContainer/NoDamageLabel" );	
		
		ExitButton = GetNode<Button>( "ExitButton" );
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );
	}
	public void SetScores( int TotalScore, int MaxCombo, int TimeMinutes, int TimeSeconds, int TimeMilliseconds ) {
		MapNameLabel.Text = TranslationServer.Translate( string.Format( "CHALLENGE{0}_NAME", ChallengeCache.GetCurrentMap() ) );

		TotalScoreLabel.Text = TotalScore.ToString();
		HighestComboLabel.Text = MaxCombo.ToString();

		TimeMinutesLabel.Text = TimeMinutes.ToString();
		TimeSecondsLabel.Text = TimeSeconds.ToString();
		TimeMillisecondsLabel.Text = TimeMilliseconds.ToString();

		if ( ( ChallengeLevel.BonusFlags & ChallengeLevel.ScoreBonus.NoDeaths ) != 0 ) {
			DeathCounterLabel.Hide();
			NoDeathsLabel.Show();
		} else {
			DeathCounterLabel.Text = ChallengeLevel.DeathCounter.ToString();
		}

		if ( ( ChallengeLevel.BonusFlags & ChallengeLevel.ScoreBonus.NoDamage ) != 0 ) {
			NoDamageLabel.Show();
		}

		if ( NoDeathsLabel.Visible && NoDamageLabel.Visible ) {
			if ( ChallengeLevel.HeadshotCounter == ChallengeLevel.TotalEnemies ) {
				// all headshots, no damage, and no deaths. The true run
				LegendLabel = GetNode<Label>( "MarginContainer/VBoxContainer/ExtrasContainer/LegendLabel" );
				LegendLabel.Show();
			} else {
				FlawlessLabel = GetNode<Label>( "MarginContainer/VBoxContainer/ExtrasContainer/FlawlessLabel" );
				FlawlessLabel.Show();
			}
		}
	}
};
