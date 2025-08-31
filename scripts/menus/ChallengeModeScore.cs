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

using ChallengeMode;
using Godot;

public struct ScoreData {
	public string MapName;
	public int TotalScore;
	public int MaxCombo;
	public int TimeMinutes;
	public int TimeSeconds;
	public int TimeMilliseconds;
	public int TotalEnemies;
	public int TotalDeaths;
	public ChallengeLevel.ScoreBonus BonusFlags;

	public ScoreData( string MapName, int TotalScore, int MaxCombo, int TimeMinutes, int TimeSeconds, int TimeMilliseconds, int TotalEnemies,
		int TotalDeaths,
		ChallengeLevel.ScoreBonus BonusFlags )
	{
		this.MapName = MapName;
		this.TotalScore = TotalScore;
		this.MaxCombo = MaxCombo;
		this.TimeMinutes = TimeMinutes;
		this.TimeSeconds = TimeSeconds;
		this.TimeMilliseconds = TimeMilliseconds;
		this.TotalEnemies = TotalEnemies;
		this.TotalDeaths = TotalDeaths;
		this.BonusFlags = BonusFlags;
	}
};

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
	public void SetScores( ScoreData score ) {
		MapNameLabel.Text = score.MapName;

		TotalScoreLabel.Text = score.TotalScore.ToString();
		HighestComboLabel.Text = score.MaxCombo.ToString();

		TimeMinutesLabel.Text = score.TimeMinutes.ToString();
		TimeSecondsLabel.Text = score.TimeSeconds.ToString();
		TimeMillisecondsLabel.Text = score.TimeMilliseconds.ToString();

		if ( ( score.BonusFlags & ChallengeLevel.ScoreBonus.NoDeaths ) != 0 ) {
			DeathCounterLabel.Hide();
			NoDeathsLabel.Show();
		} else {
			DeathCounterLabel.Text = score.TotalDeaths.ToString();
		}

		if ( ( score.BonusFlags & ChallengeLevel.ScoreBonus.NoDamage ) != 0 ) {
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
