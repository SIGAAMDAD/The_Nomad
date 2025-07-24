using Godot;
using System.Runtime.CompilerServices;

public enum KillType : uint {
	Bodyshot,
	Headshot,
	Burning,
	Execution,

	Count
};

public partial class FreeFlow : CanvasLayer {
	[Export]
	private Player _Owner;

	private int KillCounter = 0;
	private int ComboCounter = 0;
	private int MaxCombo = 0;
	private int HeadshotCounter = 0;
	private int TotalScore = 0;
	private int HellbreakCounter = 0;
	private Timer BurnoutTimer;
	private TextureRect BerserkOverlay;

	private static FreeFlow Instance;

	[Signal]
	public delegate void ComboFinishedEventHandler( int nCombo );
	[Signal]
	public delegate void NewHighestComboEventHandler( int nCombo );
	[Signal]
	public delegate void KillAddedEventHandler( KillType nType );

	private void ActivateBerserkerMode() {
		_Owner.SetFlags( _Owner.GetFlags() | Player.PlayerFlags.Berserker );
	}
	private void DeactivateBerserkerMode() {
		_Owner.SetFlags( _Owner.GetFlags() & ~Player.PlayerFlags.Berserker );
		BerserkOverlay.Set( "shader_parameter/vignette_intensity", 0.0f );
	}

	public static void AddKill( KillType nType, int nScore ) {
		switch ( nType ) {
		case KillType.Bodyshot:
			break;
		case KillType.Headshot:
			Instance.HeadshotCounter++;
			break;
		};
		Instance.KillCounter++;
		Instance.TotalScore += nScore;
		Instance.EmitSignalKillAdded( nType );
	}
	public static void IncreaseCombo( int nAmount = 1 ) {
		Instance.ComboCounter += nAmount;
		if ( Instance.ComboCounter > 10 ) {
			Instance.ActivateBerserkerMode();

			float redAmount = Mathf.Lerp( 0.0f, 1.0f, ( Instance.ComboCounter - 30.0f ) / 30.0f );
			Instance.BerserkOverlay.Set( "shader_parameter/vignette_intensity", redAmount );
		}
	}
	public static void EndCombo() {
		if ( Instance.ComboCounter > Instance.MaxCombo ) {
			Instance.MaxCombo = Instance.ComboCounter;
		}
		Instance.ComboCounter = 0;
		if ( ( Instance._Owner.GetFlags() & Player.PlayerFlags.Berserker ) != 0 ) {
			Instance.DeactivateBerserkerMode();
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetCurrentCombo() => Instance.ComboCounter;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetHighestCombo() => Instance.MaxCombo;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetKillCounter() => Instance.KillCounter;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetTotalScore() => Instance.TotalScore;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void IncreaseTotalScore( int nAmount ) {
		Instance.TotalScore += nAmount;
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void CalculateEncounterScore() {
		Instance.TotalScore += Instance.MaxCombo * 10;
		Instance.TotalScore += Instance.HellbreakCounter * 5;
	}
	public static void StartEncounter() {
		Instance.TotalScore = 0;
		Instance.MaxCombo = 0;
		Instance.ComboCounter = 0;
		Instance.KillCounter = 0;
		Instance.HeadshotCounter = 0;
	}

	public override void _Ready() {
		base._Ready();

		_Owner = GetParent<Player>();

		BerserkOverlay = GetNode<TextureRect>( "BerserkModeOverlay" );

		BurnoutTimer = GetNode<Timer>( "BurnoutTimer" );
		BurnoutTimer.Connect( Timer.SignalName.Timeout, Callable.From( EndCombo ) );

		Instance = this;
	}
};