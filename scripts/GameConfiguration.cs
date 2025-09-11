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

using Godot;
using System;
using System.Collections.Concurrent;

public enum GameDifficulty {
	Wanderer,
	Blackdeath,
	Custom,

	// memes, the DNA of the soul!
	MemeMode
};

public enum GameMode {
	SinglePlayer,
	Online,
	LocalCoop2,
	LocalCoop3,
	LocalCoop4,
	Multiplayer,
	ChallengeMode,
	JohnWick,
};

public partial class GameConfiguration : Node {
	public static bool MemeMode = false;
	public static bool Paused = false;
	public static bool DemonEyeActive = false;
	public static GameMode GameMode { get; private set; } = GameMode.SinglePlayer;
	public static GameDifficulty GameDifficulty { get; private set; } = GameDifficulty.Wanderer;

	public static event Action GameModeChanged;
	public static event Action GameDifficultyChanged;

	private static GameConfiguration Instance;

	/*
	===============
	SetGameMode
	===============
	*/
	public static void SetGameMode( GameMode gameMode ) {
		GameMode = gameMode;
		GameModeChanged.Invoke();
	}

	/*
	===============
	SetGameDifficulty
	===============
	*/
	public static void SetGameDifficulty( GameDifficulty difficulty ) {
		GameDifficulty = difficulty;
		GameDifficultyChanged.Invoke();
	}

	public override void _Ready() {
		base._Ready();

		Instance = this;
	}
};