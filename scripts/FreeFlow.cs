using Godot;
using System.Runtime.CompilerServices;

public enum KillType : uint {
	Bodyshot,
	Headshot,
	Burning,
	Execution,

	Count
};

public partial class FreeFlow : Node {
	private static int KillCounter = 0;
	private static int ComboCounter = 0;
	private static int MaxCombo = 0;
	private static int HeadshotCounter = 0;
	private static Timer BurnoutTimer;
	private static int TotalScore = 0;
	private static int HellbreakCounter = 0;

	[Signal]
	public delegate void ComboFinishedEventHandler( int nCombo );
	[Signal]
	public delegate void NewHighestComboEventHandler( int nCombo );
	[Signal]
	public delegate void KillAddedEventHandler( KillType nType );

	public static void AddKill( KillType nType, int nScore ) {
		switch ( nType ) {
		case KillType.Bodyshot:
			break;
		case KillType.Headshot:
			HeadshotCounter++;
			break;
		};
		KillCounter++;
		TotalScore += nScore;
	}
	public static void IncreaseCombo( int nAmount = 1 ) {
		ComboCounter += nAmount;
	}
	public static void EndCombo() {
		if ( ComboCounter > MaxCombo ) {
			MaxCombo = ComboCounter;
		}
		ComboCounter = 0;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetCurrentCombo() => ComboCounter;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetHighestCombo() => MaxCombo;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetKillCounter() => KillCounter;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetTotalScore() => TotalScore;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void IncreaseTotalScore( int nAmount ) {
		TotalScore += nAmount;
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CalculateEncounterScore() {
		TotalScore += MaxCombo * 10;
		TotalScore += HellbreakCounter * 5;
	}

	public override void _Ready() {
		base._Ready();

		BurnoutTimer = new Timer();
		BurnoutTimer.Name = "FreeFlowComboBurnoutTimer";
		BurnoutTimer.WaitTime = 3.5f;
		BurnoutTimer.OneShot = true;
		BurnoutTimer.Connect( Timer.SignalName.Timeout, Callable.From( EndCombo ) );
		AddChild( BurnoutTimer );
	}
};