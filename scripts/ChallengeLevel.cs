using System;
using System.Diagnostics;
using ChallengeMode;
using Godot;
using PlayerSystem;
using Renown;
using Renown.Thinkers;

// TODO: allow multiplayer with challenge modes?

public partial class ChallengeLevel : LevelData {
	public enum ScoreBonus {
		None		= 0x0000,
		NoDamage	= 0x0001,
		NoDeaths	= 0x0002,

		All			= NoDamage | NoDeaths,

		Count
	};

	[Export]
	private int MinTimeMinutes = 0;
	[Export]
	private int MinTimeSeconds = 0;
	[Export]
	private Hellbreaker Hellbreaker;
	[Export]
	private Node2D Level;
	[Export]
	private Godot.Collections.Dictionary<string, Variant> State;
	[Export]
	private string EndOfQuestDialogue;
	[Export]
	private string StartQuestDialogue;

	private static Godot.Collections.Dictionary<string, Variant> ObjectivesState;
	public static Godot.Collections.Array<Thinker> Enemies;

	private bool QuestCompleted = false;

	private Stopwatch Timer;

	// extra flags
	public static int DeathCounter = 0;
	public static int TotalEnemies = 0;
	public static int HeadshotCounter = 0;
	public static int HellbreakCounter = 0;
	public static ScoreBonus BonusFlags = ScoreBonus.All;

	private static int TotalScore = 0;
	private static int MaxCombo = 0;

	private void OnHellbreakerExitFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnHellbreakerExitFinished ) );
		
		Hellbreaker.Hide();
		Level.Show();
	}
	public void ExitHellbreaker() {
		Level.ProcessMode = ProcessModeEnum.Pausable;

		Godot.Collections.Array<Node> entities = GetTree().GetNodesInGroup( "Enemies" );
		for ( int i = 0; i < entities.Count; i++ ) {
			entities[i].ProcessMode = ProcessModeEnum.Pausable;
		}

		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnHellbreakerExitFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
		Hellbreaker.SetDeferred( "process_mode", (uint)ProcessModeEnum.Disabled );

		EmitSignalHellbreakerFinished();

		HellbreakCounter++;
	}

	private void OnHellbreakerTransitionFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnHellbreakerTransitionFinished ) );
		Hellbreaker.Start( ThisPlayer );

		EmitSignalHellbreakerBegin();
	}
	private void OnPlayerDie( Entity source, Entity target ) {
		if ( Hellbreaker.Activate( ThisPlayer ) ) {
			Level.CallDeferred( "hide" );
			Level.SetDeferred( "process_mode", (long)ProcessModeEnum.Disabled );

			Godot.Collections.Array<Node> entities = GetTree().GetNodesInGroup( "Enemies" );
			for ( int i = 0; i < entities.Count; i++ ) {
				entities[i].ProcessMode = ProcessModeEnum.Disabled;
			}

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnHellbreakerTransitionFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

			Console.PrintLine( "Beginning hellbreaker..." );
			return;
		}

		DeathCounter++;

		EmitSignalPlayerRespawn();
		BonusFlags &= ~ScoreBonus.NoDeaths;
	}
	private void OnPlayerDamaged( Entity source, Entity target, float nAmount ) {
		BonusFlags &= ~ScoreBonus.NoDamage;
	}

	public static Godot.Variant GetObjectiveState( string key ) {
		return ObjectivesState[ key ];
	}
	public static void SetObjectiveState( string key, Godot.Variant value ) {
		if ( !ObjectivesState.ContainsKey( key ) ) {
			Console.PrintError( string.Format( "ChallengeLevel.SetObjectiveState: invalid ObjectiveState key \"{0}\"", key ) );
			return;
		}
		Console.PrintLine( string.Format( "Set objective state \"{0}\" to {1}", key, value.ToString() ) );
		ObjectivesState[ key ] = value;
	}

	private void OnEndOfChallengeReached() {
		if ( !QuestCompleted ) {
			ThisPlayer.ThoughtBubble( "You: I need to finish what I started..." );
			return;
		}

		Timer.Stop();

		EndCombo( Player.ComboCounter );

		//
		// calculate total score
		//
		TotalScore += MaxCombo * 10;
		TotalScore += HellbreakCounter * 5;

		int milliseconds = (int)Timer.Elapsed.TotalMilliseconds;
		int seconds = (int)( milliseconds / 1000.0f );
		int minutes = (int)( seconds / 60.0f );

		if ( MinTimeMinutes != 0 ) {
			int leftOver = MinTimeMinutes - minutes;
			if ( leftOver > 0 ) {
				TotalScore += leftOver * 100;
			}
		}
		if ( MinTimeSeconds != 0 ) {
			int leftOver = MinTimeSeconds - seconds;
			if ( leftOver > 0 ) {
				TotalScore += leftOver * 10;
			}
		}
		if ( DeathCounter == 0 ) {
			// no deaths
			TotalScore += 1000;
		}
		if ( ( BonusFlags & ScoreBonus.NoDamage ) != 0 ) {
			// no damage
			TotalScore += 10000;
		}
		if ( DeathCounter == 0 && ( BonusFlags & ScoreBonus.NoDamage ) != 0 && HeadshotCounter == TotalEnemies ) {
			// legend
			TotalScore += 10000;
		}

		ThisPlayer.BlockInput( true );

		ChallengeCache.UpdateScore( ChallengeCache.GetCurrentLeaderboard(), ChallengeCache.GetCurrentMap(), TotalScore, minutes, seconds, milliseconds );
		
		ChallengeModeScore ScoreOverlay = ResourceCache.GetScene( "res://scenes/menus/challenge_mode_score.tscn" ).Instantiate<ChallengeModeScore>();
		AddChild( ScoreOverlay );
		ScoreOverlay.SetScores( TotalScore, MaxCombo, minutes, seconds, milliseconds );
	}
	public static void EndCombo( int nCurrentCombo ) {
		if ( nCurrentCombo > MaxCombo ) {
			System.Threading.Interlocked.Exchange( ref MaxCombo, nCurrentCombo );
		}
	}
	public static void IncreaseScore( int nAmount ) {
		System.Threading.Interlocked.Add( ref TotalScore, nAmount );
	}

	protected override void OnResourcesFinishedLoading() {
		ResourceLoadThread.Join();

		ResourceCache.Initialized = true;

		GC.Collect( GC.MaxGeneration, GCCollectionMode.Aggressive );

		Console.PrintLine( "...Finished loading game" );
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
		
		Timer = new Stopwatch();
		Timer.Start();
	}

	private void OnConditionQueryRequested( string queryType, string key, Variant value, Resource requester ) {
		switch ( queryType ) {
		case "State":
			if ( ObjectivesState.TryGetValue( key, out Godot.Variant compare ) ) {
				if ( compare.VariantType == Variant.Type.Bool ) {
					Questify.SetConditionCompleted( requester, compare.AsBool() == value.AsBool() );
				} else if ( compare.VariantType == Variant.Type.Float ) {
					int index = queryType.Find( ':' );
					if ( index != -1 ) {
						string op = queryType.Substring( index + 1 );
						switch ( op ) {
						case "eq":
						case "==":
							Questify.SetConditionCompleted( requester, compare.AsDouble() == value.AsDouble() );
							break;
						case "neq":
						case "!=":
						case "!eq":
						case "ne":
							Questify.SetConditionCompleted( requester, compare.AsDouble() != value.AsDouble() );
							break;
						case "lt":
						case "<":
							Questify.SetConditionCompleted( requester, compare.AsDouble() < value.AsDouble() );
							break;
						case "lte":
						case "<=":
							Questify.SetConditionCompleted( requester, compare.AsDouble() <= value.AsDouble() );
							break;
						case "gt":
						case ">":
							Questify.SetConditionCompleted( requester, compare.AsDouble() > value.AsDouble() );
							break;
						case "gte":
						case ">=":
							Questify.SetConditionCompleted( requester, compare.AsDouble() >= value.AsDouble() );
							break;
						default:
							Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid queryType operator {0}", op ) );
							break;
						};
					} else {
						Questify.SetConditionCompleted( requester, compare.AsDouble() == value.AsDouble() );
					}
				} else if ( compare.VariantType == Variant.Type.Int ) {
					int index = queryType.Find( ':' );
					if ( index != -1 ) {
						string op = queryType.Substring( index + 1 );
						switch ( op ) {
						case "eq":
						case "==":
							Questify.SetConditionCompleted( requester, compare.AsInt32() == value.AsInt32() );
							break;
						case "neq":
						case "!=":
						case "!eq":
						case "ne":
							Questify.SetConditionCompleted( requester, compare.AsInt32() != value.AsInt32() );
							break;
						case "lt":
						case "<":
							Questify.SetConditionCompleted( requester, compare.AsInt32() < value.AsInt32() );
							break;
						case "lte":
						case "<=":
							Questify.SetConditionCompleted( requester, compare.AsInt32() <= value.AsInt32() );
							break;
						case "gt":
						case ">":
							Questify.SetConditionCompleted( requester, compare.AsInt32() > value.AsInt32() );
							break;
						case "gte":
						case ">=":
							Questify.SetConditionCompleted( requester, compare.AsInt32() >= value.AsInt32() );
							break;
						default:
							Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid queryType operator {0}", op ) );
							break;
						};
					} else {
						Questify.SetConditionCompleted( requester, compare.AsInt32() == value.AsInt32() );
					}
				} else if ( compare.VariantType == Variant.Type.String ) {
					Questify.SetConditionCompleted( requester, compare.AsString() == value.AsString() );
				} else if ( compare.VariantType == Variant.Type.Vector2 ) {
					Questify.SetConditionCompleted( requester, compare.AsVector2() == value.AsVector2() );
				}
			}
			break;
		default:
			Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid QueryType {0}", queryType ) );
			break;
		};
	}
	private void OnConditionObjectiveCompleted( Resource questResource, Resource questObjective ) {
	}
	private void OnQuestObjectiveAdded( Resource questResource, Resource questObjective ) {
		Console.PrintLine( "Added quest objective..." );
	}
	private void OnQuestCompleted( Resource questResource ) {
		Console.PrintLine( "Finished quest..." );
		QuestCompleted = true;

		HeadsUpDisplay.StartThoughtBubble( "You: " + EndOfQuestDialogue );
	}

	public override void _Ready() {
		base._Ready();

		ResourceLoadThread = new System.Threading.Thread( () => { ResourceCache.Cache( this, null ); } );
		ResourceLoadThread.Start();

		ThisPlayer.Die += OnPlayerDie;
		ThisPlayer.Damaged += OnPlayerDamaged;

		HeadsUpDisplay.StartThoughtBubble( "You: " + StartQuestDialogue );

		EndOfChallenge end = GetNode<EndOfChallenge>( "Level/EndOfChallenge" );
		end.Connect( "Triggered", Callable.From( OnEndOfChallengeReached ) );

		Questify.ToggleUpdatePolling( true );
		Questify.ConnectConditionQueryRequested( OnConditionQueryRequested );
		Questify.ConnectQuestObjectiveCompleted( OnConditionObjectiveCompleted );
		Questify.ConnectQuestObjectiveAdded( OnQuestObjectiveAdded );
		Questify.ConnectQuestCompleted( OnQuestCompleted );
		Questify.StartQuest( ChallengeCache.GetQuestData() );

		ObjectivesState = new Godot.Collections.Dictionary<string, Variant>();
		foreach ( var state in State ) {
			ObjectivesState.Add( state.Key, state.Value );
		}

		TotalEnemies = GetTree().GetNodeCountInGroup( "Enemies" );
	}
};
