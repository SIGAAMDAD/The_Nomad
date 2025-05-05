using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ChallengeMode;
using Godot;
using Renown;
using Renown.Thinkers;

// TODO: allow multiplayer with challenge modes?

public partial class ChallengeLevel : LevelData {
	private enum ScoreBonus {
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
	private Node2D Hellbreaker;
	[Export]
	private Node2D Level;
	[Export]
	private Godot.Collections.Dictionary<string, Variant> State;

	private static Dictionary<string, object> ObjectivesState;
	public static Godot.Collections.Array<Thinker> Enemies;

	private bool QuestCompleted = false;

	private Stopwatch Timer;

	private static int TotalScore = 0;
	private static int MaxCombo = 0;
	private ScoreBonus BonusFlags = ScoreBonus.All;

	private void OnPlayerDie( Entity source, Entity target ) {
		BonusFlags &= ~ScoreBonus.NoDeaths;

		Hellbreaker.Show();
		Hellbreaker.ProcessMode = ProcessModeEnum.Pausable;

		Level.Show();
		Level.ProcessMode = ProcessModeEnum.Disabled;
		
		AddChild( Hellbreaker );
	}
	private void OnPlayerDamaged( Entity source, Entity target, float nAmount ) {
		BonusFlags &= ~ScoreBonus.NoDamage;
	}

	public static void SetObjectiveState( string key, object value ) {
		if ( !ObjectivesState.ContainsKey( key ) ) {
			Console.PrintError( string.Format( "ChallengeLevel.SetObjectiveState: invalid ObjectiveState key \"{0}\"", key ) );
			return;
		}
		ObjectivesState[ key ] = value;
	}

	private void OnEndOfChallengeReached() {
		if ( !QuestCompleted ) {
			ThisPlayer.ThoughtBubble( "You: I need to finish what I started..." );
			return;
		}

		Timer.Stop();

		//
		// calculate total score
		//
		TotalScore += MaxCombo * 10;

		int minutes = (int)Timer.Elapsed.TotalMinutes;
		int seconds = (int)Timer.Elapsed.TotalSeconds;
		int milliseconds = (int)Timer.Elapsed.TotalMilliseconds;

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

		ChallengeCache.UpdateScore( ChallengeCache.GetCurrentLeaderboard(), ChallengeCache.GetCurrentMap(), TotalScore, minutes, seconds, milliseconds );

		GetTree().CallDeferred( "change_scene_to_file", "res://scenes/main_menu.tscn" );
	}
	public static void EndCombo( int nScore ) {
		if ( nScore > MaxCombo ) {
			MaxCombo = nScore;
		}
	}
	public static void IncreaseScore( int nAmount ) {
		TotalScore += nAmount * MaxCombo;
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
			if ( ObjectivesState.TryGetValue( key, out object compare ) ) {
				if ( compare is bool boolValue ) {
					Questify.SetConditionCompleted( requester, boolValue == value.AsBool() );
				} else if ( compare is float floatValue ) {
					int index = queryType.Find( ':' );
					if ( index != -1 ) {
						string op = queryType.Substring( index + 1 );
						switch ( op ) {
						case "eq":
						case "==":
							Questify.SetConditionCompleted( requester, floatValue == value.AsDouble() );
							break;
						case "neq":
						case "!=":
						case "!eq":
						case "ne":
							Questify.SetConditionCompleted( requester, floatValue != value.AsDouble() );
							break;
						case "lt":
						case "<":
							Questify.SetConditionCompleted( requester, floatValue < value.AsDouble() );
							break;
						case "lte":
						case "<=":
							Questify.SetConditionCompleted( requester, floatValue <= value.AsDouble() );
							break;
						case "gt":
						case ">":
							Questify.SetConditionCompleted( requester, floatValue > value.AsDouble() );
							break;
						case "gte":
						case ">=":
							Questify.SetConditionCompleted( requester, floatValue >= value.AsDouble() );
							break;
						default:
							Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid queryType operator {0}", op ) );
							break;
						};
					} else {
						Questify.SetConditionCompleted( requester, floatValue == value.AsDouble() );
					}
				} else if ( compare is int intValue ) {
					int index = queryType.Find( ':' );
					if ( index != -1 ) {
						string op = queryType.Substring( index + 1 );
						switch ( op ) {
						case "eq":
						case "==":
							Questify.SetConditionCompleted( requester, intValue == value.AsInt32() );
							break;
						case "neq":
						case "!=":
						case "!eq":
						case "ne":
							Questify.SetConditionCompleted( requester, intValue != value.AsInt32() );
							break;
						case "lt":
						case "<":
							Questify.SetConditionCompleted( requester, intValue < value.AsInt32() );
							break;
						case "lte":
						case "<=":
							Questify.SetConditionCompleted( requester, intValue <= value.AsInt32() );
							break;
						case "gt":
						case ">":
							Questify.SetConditionCompleted( requester, intValue > value.AsInt32() );
							break;
						case "gte":
						case ">=":
							Questify.SetConditionCompleted( requester, intValue >= value.AsInt32() );
							break;
						default:
							Console.PrintError( string.Format( "ChallengeLevel.OnConditionQueryRequested: invalid queryType operator {0}", op ) );
							break;
						};
					} else {
						Questify.SetConditionCompleted( requester, intValue == value.AsInt32() );
					}
				} else if ( compare is string stringValue ) {
					Questify.SetConditionCompleted( requester, stringValue == value.AsString() );
				} else if ( compare is Vector2 vectorValue ) {
					Questify.SetConditionCompleted( requester, vectorValue == value.AsVector2() );
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
	private void OnQuestCompleted( Resource questResource ) {
		QuestCompleted = true;
	}

	public override void _Ready() {
		base._Ready();

		ResourceLoadThread = new Thread( () => { ResourceCache.Cache( this, null ); } );
		ResourceLoadThread.Start();

		ThisPlayer.Die += OnPlayerDie;
		ThisPlayer.Damaged += OnPlayerDamaged;

		EndOfChallenge end = GetNode<EndOfChallenge>( "Level/EndOfChallenge" );
		end.Connect( "Triggered", Callable.From( OnEndOfChallengeReached ) );

		Questify.ConnectConditionQueryRequested( OnConditionQueryRequested );
		Questify.ConnectQuestObjectiveCompleted( OnConditionObjectiveCompleted );
		Questify.ConnectQuestCompleted( OnQuestCompleted );

		ObjectivesState = new Dictionary<string, object>();
		foreach ( var state in State ) {
			ObjectivesState.Add( state.Key, state.Value );
		}

		Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Enemies" );
		Enemies = new Godot.Collections.Array<Thinker>();
		for ( int i = 0; i < nodes.Count; i++ ) {
			Enemies.Add( nodes[i] as Thinker );
		}
	}
};
