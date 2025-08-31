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
using System.Runtime.CompilerServices;
using Steam;
using System;

public enum KillType : uint {
	Bodyshot,
	Headshot,
	Burning,
	Execution,

	Count
};

/*
===================================================================================

FreeFlow

===================================================================================
*/
/// <summary>
/// Manages player combo state and berserker mode
/// </summary>

public partial class FreeFlow : CanvasLayer {
	public int KillCounter { get; private set; } = 0;
	public int ComboCounter { get; private set; } = 0;
	public int MaxCombo { get; private set; } = 0;
	public int HeadshotCounter { get; private set; } = 0;
	public int TotalScore { get; private set; } = 0;

	private int HellbreakCounter = 0;
	private Timer? BurnoutTimer;
	private Timer? SlowmoTimer;
	private TextureRect? BerserkOverlay;

	private int JohnWickCounter = 0;

	public static FreeFlow? Instance { get; private set; }

	[Signal]
	public delegate void ComboFinishedEventHandler( int combo );
	[Signal]
	public delegate void NewHighestComboEventHandler( int combo );
	[Signal]
	public delegate void KillAddedEventHandler( KillType type );

	/*
	===============
	ActivateBerserkerMode
	===============
	*/
	private void ActivateBerserkerMode() {
		SteamAchievements.ActivateAchievement( "ACH_LOSE_YOURSELF" );
		LevelData.Instance.ThisPlayer.SetFlags( LevelData.Instance.ThisPlayer.Flags | Player.PlayerFlags.Berserker );
	}

	/*
	===============
	DeactivateBerserkerMode
	===============
	*/
	private void DeactivateBerserkerMode() {
		LevelData.Instance.ThisPlayer.SetFlags( LevelData.Instance.ThisPlayer.Flags & ~Player.PlayerFlags.Berserker );
		BerserkOverlay.Set( "shader_parameter/vignette_intensity", 0.0f );
	}

	/*
	===============
	AddKill
	===============
	*/
	/// <summary>
	/// Adds a kill to the current score of the encounter
	/// </summary>
	/// <param name="type"></param>
	/// <param name="score"></param>
	public static void AddKill( KillType type, int score ) {
		if ( Instance == null ) {
			throw new InvalidOperationException( "FreeFlow must be instantiated before being called" );
		}
		switch ( type ) {
			case KillType.Bodyshot:
				break;
			case KillType.Headshot:
				Instance.HeadshotCounter++;
				Instance.JohnWickCounter++;
				if ( Instance.JohnWickCounter > 30 ) {
					SteamAchievements.ActivateAchievement( "ACH_JOHN_WICK_MODE" );
				}
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( type ) );
		}
		Instance.KillCounter++;
		Instance.TotalScore += score;
		Instance.EmitSignalKillAdded( type );
	}

	/*
	===============
	IncreaseCombo
	===============
	*/
	/// <summary>
	/// Increase the current freeflow combo counter by <b>amount</b>
	/// </summary>
	/// <param name="amount">The amount to increase the combo by</param>
	public static void IncreaseCombo( int amount = 1 ) {
		if ( Instance == null ) {
			throw new InvalidOperationException( "FreeFlow must be instantiated before being called" );
		}
		ArgumentOutOfRangeException.ThrowIfLessThan( amount, 1 );

		Instance.ComboCounter += amount;
		if ( Instance.ComboCounter > 10 ) {
			Instance.ActivateBerserkerMode();

			float redAmount = Mathf.Lerp( 0.0f, 1.0f, ( Instance.ComboCounter - 30.0f ) / 30.0f );
			Instance.BerserkOverlay.Set( "shader_parameter/vignette_intensity", redAmount );
		}
	}

	/*
	===============
	EndCombo
	===============
	*/
	public static void EndCombo() {
		if ( Instance == null ) {
			throw new InvalidOperationException( "FreeFlow must be instantiated before being called" );
		}
		if ( Instance.ComboCounter > Instance.MaxCombo ) {
			Instance.MaxCombo = Instance.ComboCounter;
		}
		Instance.ComboCounter = 0;
		if ( ( LevelData.Instance.ThisPlayer.Flags & Player.PlayerFlags.Berserker ) != 0 ) {
			Instance.DeactivateBerserkerMode();
		}
	}

	/*
	===============
	IncreaseTotalScore
	===============
	*/
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void IncreaseTotalScore( int amount ) {
		if ( Instance == null ) {
			throw new InvalidOperationException( "FreeFlow must be instantiated before being called" );
		}
		ArgumentOutOfRangeException.ThrowIfLessThan( amount, 0 );

		Instance.TotalScore += amount;
	}

	/*
	===============
	CalculateEncounterScore
	===============
	*/
	public static void CalculateEncounterScore() {
		if ( Instance == null ) {
			throw new InvalidOperationException( "FreeFlow must be instantiated before being called" );
		}

		Instance.TotalScore += Instance.MaxCombo * 10;
		Instance.TotalScore += Instance.HellbreakCounter * 5;
	}

	/*
	===============
	StartEncounter
	===============
	*/
	public static void StartEncounter() {
		Instance.TotalScore = 0;
		Instance.MaxCombo = 0;
		Instance.ComboCounter = 0;
		Instance.KillCounter = 0;
		Instance.HeadshotCounter = 0;
	}

	/*
	===============
	Hitstop
	===============
	*/
	/// <summary>
	/// Used for hitstop effects, exclusively for singleplayer
	/// </summary>
	/// <param name="gameSpeed">What to set Engine.TimeScale to</param>
	/// <param name="duration">The duration (in seconds) of the effect</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if duration or gamespeed or duration is less than or equal to 0.0f</exception>
	/// <exception cref="InvalidOperationException">Thrown if FreeFlow was called without being instantiated</exception>
	public static void Hitstop( float gameSpeed, float duration ) {
		if ( Instance == null ) {
			throw new InvalidOperationException( "FreeFlow must be instantiated before being called" );
		}
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual( duration, 0.0f );
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual( gameSpeed, 0.0f );

		Engine.TimeScale = gameSpeed;
		AudioServer.PlaybackSpeedScale = gameSpeed;

		Instance.SlowmoTimer.WaitTime = duration;
		Instance.SlowmoTimer.Start();
	}

	/*
	===============
	OnSlowmoTimerTimeout
	===============
	*/
	private void OnSlowmoTimerTimeout() {
		if ( Instance == null ) {
			throw new InvalidOperationException( "FreeFlow must be instantiated before being called" );
		}
		Engine.TimeScale = 1.0f;
		AudioServer.PlaybackSpeedScale = 1.0f;
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

		Console.PrintLine( "FreeFlow initialized." );

		LevelData.Instance.ThisPlayer.Damaged += ( source, target, amount ) => JohnWickCounter = 0;

		SlowmoTimer = new Timer();
		SlowmoTimer.Name = "SlowmoTimer";
		SlowmoTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSlowmoTimerTimeout ) );
		AddChild( SlowmoTimer );

		BerserkOverlay = GetNode<TextureRect>( "BerserkModeOverlay" );

		BurnoutTimer = GetNode<Timer>( "BurnoutTimer" );
		BurnoutTimer.Connect( Timer.SignalName.Timeout, Callable.From( EndCombo ) );

		Instance = this;
	}
};