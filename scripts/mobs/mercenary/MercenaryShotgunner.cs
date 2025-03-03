using Godot;
using MountainGoap;
using System.Collections.Generic;
using System.Collections.Concurrent;

public partial class MercenaryShotgunner : MobBase {
	protected override void OnLoseInterestTimerTimeout() {
		float fear = (float)Agent.State[ "Fear" ];
		
		Agent.State[ "Alert" ] = true;
		fear += 10.0f; // they're a little more on edge
		Agent.State[ "Fear" ] = fear;

		SightTarget = null;
		Navigation.TargetPosition = (Godot.Vector2)Agent.State[ "GuardPosition" ];
	}

	public override void _Ready() {
		base._Ready();

		Init();

		Navigation.Connect( "target_reached", Callable.From( OnTargetReached ) );

		List<BaseGoal> goals = new List<BaseGoal>();
		List<MountainGoap.Action> actions = new List<MountainGoap.Action>();

		Goal InvestigateDisturbanceGoal = new Goal(
			"InvestigateDisturbance",
			80.0f,
			desiredState: new Dictionary<string, object>{
			}
		);
		ComparativeGoal SurviveGoal = new ComparativeGoal(
			"Survive",
			100.0f,
			desiredState: new Dictionary<string, ComparisonValuePair>{
				{ "Health", new ComparisonValuePair{ Value = 50.0f, Operator = ComparisonOperator.GreaterThan } }
			}
		);
		Goal FindThreatsGoal = new Goal(
			"FindThreats",
			50.0f,
			desiredState: new Dictionary<string, object>{
				{ "HasTarget", true }
			}
		);

		goals.Add( FindThreatsGoal );
		goals.Add( InvestigateDisturbanceGoal );
		goals.Add( SurviveGoal );

		MountainGoap.Action GuardNode = new MountainGoap.Action(
			"GuardNode",
			null,
			executor: ( Agent agent, MountainGoap.Action action ) => Action_CheckForTargets( agent, action ),
			5.0f,
			null,
			preconditions: new Dictionary<string, object>{
				{ "HasTarget", false }
			}
		);
		MountainGoap.Action InvestigateNode = new MountainGoap.Action(
			"InvestigateNode",
			null,
			executor: ( Agent agent, MountainGoap.Action action ) => Action_InvestigateNode( agent, action ),
			20.0f,
			null,
			preconditions: new Dictionary<string, object>{
				{ "Alert", true },
				{ "TargetReached", false },
				{ "CanSeeTarget", false }
			},
			comparativePreconditions: null,
			postconditions: new Dictionary<string, object>{
				{ "TargetReached", true }
			}
		);
		MountainGoap.Action GotoNode = new MountainGoap.Action(
			"GotoNode",
			null,
			executor: ( Agent agent, MountainGoap.Action action ) => Action_GotoNode( agent, action ),
			10.0f,
			null,
			preconditions: new Dictionary<string, object>{
				{ "TargetReached", false },
			},
			comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
				{ "TargetDistance", new ComparisonValuePair{ Value = 1.0f, Operator = ComparisonOperator.GreaterThan } }
			}
		);
		MountainGoap.Action RunAway = new MountainGoap.Action(
			"RunAway",
			null,
			executor: ( Agent agent, MountainGoap.Action action ) => Action_RunAway( agent, action ),
			2.0f,
			null,
			preconditions: null,
			comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
				{ "Health", new() { Operator = ComparisonOperator.LessThan, Value = 20.0f } }
			},
			postconditions: new Dictionary<string, object>{
			}
		);

		actions.Add( GuardNode );
		actions.Add( InvestigateNode );
		actions.Add( RunAway );
		actions.Add( GotoNode );

		Agent = new Agent(
			"MercenaryShotgunner",
			new ConcurrentDictionary<string, object>{
				{ "GuardPosition", GlobalPosition },
				{ "HasTarget", false },
				{ "TargetDistance", 0.0f },
				{ "TargetReached", false },
				{ "Fear", 0.0f },
				{ "Alert", false },
				{ "CanSeeTarget", false }
			},
			null,
			goals,
			actions,
			null
		);
	}

	private void SetDetectionColor() {
		DetectionColor.R = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
		DetectionColor.G = 0.0f;
		DetectionColor.B = 0.0f;
		DetectionColor.A = 1.0f;

		DetectionMeter.DefaultColor = DetectionColor;
	}
	private ExecutionStatus Action_CheckForTargets( Agent agent, MountainGoap.Action action ) {
		RecalcSight();

		CharacterBody2D sightTarget = null;
		for ( int i = 0; i < SightLines.Count; i++ ) {
			if ( SightLines[i].IsColliding() && SightLines[i].GetCollider() is Player ) {
				sightTarget = (CharacterBody2D)SightLines[i].GetCollider();
				break;
			}
		}

		// we haven't been alerted yet
		if ( !(bool)Agent.State[ "Alert" ] ) {
			if ( SightTarget == null && sightTarget != null ) {
				// we haven't seen anything yet...
				SightTarget = sightTarget;
				SetDetectionColor();

				SightDetectionAmount += SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();
				Agent.State[ "Alert" ] = true;

				return ExecutionStatus.Executing;
			}
		}
		else if ( SightTarget != null && sightTarget != null ) {
			// we still see a potential target
			SetDetectionColor();
			SightDetectionAmount += SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();
			
			// we've seen enough, FUCK 'EM UP!
			if ( SightDetectionAmount >= SightDetectionTime ) {
				Bark.Stream = MobSfxCache.Instance.TargetSpotted[ RandomFactory.Next( 0, MobSfxCache.Instance.TargetSpotted.Count - 1 ) ];
				Bark.Play();

				Agent.State[ "TargetReached" ] = false;
				Agent.State[ "TargetPosition" ] = SightTarget.GlobalPosition;
				Agent.State[ "HasTarget" ] = true;

				return ExecutionStatus.Succeeded;
			}
			return ExecutionStatus.Executing;
		}
		else if ( SightTarget != null && sightTarget == null ) {
			// we saw something, but it's not confirmed
			SetDetectionColor();

			if ( (float)Agent.State[ "Fear" ] > 50.0f ) {
				Bark.Stream = MobSfxCache.Instance.Alert[ RandomFactory.Next( 0, MobSfxCache.Instance.Alert.Count - 1 ) ];
			} else {
				Bark.Stream = MobSfxCache.Instance.Confusion[ RandomFactory.Next( 0, MobSfxCache.Instance.Confusion.Count - 1 ) ];
			}
			Bark.Play();

			float fear = (float)Agent.State[ "Fear" ];
			fear += 5.0f; // make them just a tad bit scared
			Agent.State[ "Fear" ] = fear;
			Agent.State[ "Alert" ] = true;
			Agent.State[ "TargetReached" ] = false;
			Agent.State[ "TargetPosition" ] = SightTarget.GlobalPosition;
			Agent.State[ "HasTarget" ] = true;
			Agent.State[ "CanSeeTarget" ] = false;

			return ExecutionStatus.Succeeded;
		}
		return ExecutionStatus.Executing;
	}

	public override void _Process( double delta ) {
		base._Process( delta );

		if ( (float)Agent.State[ "Fear" ] >= 80.0f ) {
			Bark.Stream = MobSfxCache.Instance.Curse[ RandomFactory.Next( 0, MobSfxCache.Instance.Curse.Count - 1 ) ];
			Bark.Play();
		}

		Agent.Step();
	}
};