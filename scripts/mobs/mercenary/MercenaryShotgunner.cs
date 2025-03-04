using Godot;
using MountainGoap;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

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

		List<BaseGoal> goals = new List<BaseGoal>{
//			new ComparativeGoal(
//				"Survive",
//				0.0f,
//				desiredState: new Dictionary<string, ComparisonValuePair>{
//					{ "Health", new ComparisonValuePair{ Value = 50.0f, Operator = ComparisonOperator.GreaterThan } },
//					{ "Scared", new ComparisonValuePair{ Value = false, Operator = ComparisonOperator.Equals } }
//				}
//			),
			new Goal(
				name: "FindThreats",
				weight: 100.0f,
				desiredState: new Dictionary<string, object>{
					{ "HasTarget", true }
				}
			)
//			new Goal(
//				name: "Cover",
//				weight: 20.0f,
//				desiredState: new Dictionary<string, object>{
//					{ "InCover", true }
//				}
//			)
		};
		List<MountainGoap.Action> actions = new List<MountainGoap.Action>{
			new MountainGoap.Action(
				"GuardNode",
				permutationSelectors: null,
				executor: ( Agent agent, MountainGoap.Action action ) => Action_CheckForTargets( agent, action ),
				0.0f,
				null,
				preconditions: new Dictionary<string, object>{
					{ "HasTarget", false }
				},
				comparativePreconditions: null,
				postconditions: new Dictionary<string, object>{
					{ "HasTarget", true }
				}
			),
			new MountainGoap.Action(
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
			),
			new MountainGoap.Action(
				"InvestigateNode",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if ( SightTarget != null ) {
						return ExecutionStatus.Succeeded;
					}
					if ( LoseInterestTimer.IsStopped() ) {
						// we can't see them, so begin the interest timer
						LoseInterestTimer.Start();
					}
					return ExecutionStatus.Executing;
				},
				20.0f,
				null,
				preconditions: new Dictionary<string, object>{
					{ "Alert", true },
					{ "TargetReached", false },
					{ "CanSeeTarget", false }
				},
				comparativePreconditions: null
			),
			new MountainGoap.Action(
				"RunAway",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if ( GlobalPosition == (Godot.Vector2)agent.State[ "TargetPosition" ] ) {
						return ExecutionStatus.Succeeded;
					}
					Action_GotoNode( agent, action );
					return ExecutionStatus.Executing;
				},
				2.0f,
				null,
				preconditions: null,
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "Health", new() { Operator = ComparisonOperator.LessThan, Value = 20.0f } }
				},
				postconditions: new Dictionary<string, object>{
					{ "Scared", false }
				}
			)
		};
		List<Sensor> sensors = new List<Sensor>{
			new Sensor(
				runCallback: ( Agent agent ) => {
//					agent.State[ "TargetReached" ] = false;
				},
				name: "Scared"
			),
			new Sensor(
				runCallback: ( Agent agent ) => {
					if ( GlobalPosition != (Godot.Vector2)agent.State[ "SightPosition" ] ) {
						RecalcSight();
					}

					CharacterBody2D sightTarget = null;
					for ( int i = 0; i < SightLines.Count; i++ ) {
						if ( SightLines[i].IsColliding() && SightLines[i].GetCollider() is Player || SightLines[i].GetCollider() is MobBase ) {
							sightTarget = (CharacterBody2D)SightLines[i].GetCollider();
							GD.Print( "Got target" );
							break;
						}
					}
					if ( sightTarget == null ) {
						if ( SightDetectionAmount > 0.0f ) {
							SetDetectionColor();
							SightDetectionAmount -= SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();
							if ( SightDetectionAmount < 0.0f ) {
								SightDetectionAmount = 0.0f;
							}
						}
						return;
					}
					else if ( sightTarget is Player ) {
						// got a player
						SightTarget = sightTarget;

						if ( !(bool)agent.State[ "Alert" ] ) {
							if ( SightTarget == null && sightTarget != null ) {
								// we haven't seen anything yet...
								SightTarget = sightTarget;
								SetDetectionColor();

								SightDetectionAmount += SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();
								agent.State[ "Alert" ] = true;

								return;
							}
						}
						else if ( sightTarget != null ) {
							// we still see a potential target
							SetDetectionColor();
							SightDetectionAmount += SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();

							// we've seen enough, FUCK 'EM UP!
							if ( SightDetectionAmount >= SightDetectionTime ) {
								Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.Next( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
								Bark.Play();

								agent.State[ "TargetReached" ] = false;
								agent.State[ "TargetPosition" ] = SightTarget.GlobalPosition;
								agent.State[ "HasTarget" ] = true;
							}
							return;
						}
						else {
							// we saw something, but it's not confirmed
							SetDetectionColor();

							if ( (float)agent.State[ "Fear" ] > 50.0f ) {
								Bark.Stream = MobSfxCache.Alert[ RandomFactory.Next( 0, MobSfxCache.Alert.Count - 1 ) ];
							} else {
								Bark.Stream = MobSfxCache.Confusion[ RandomFactory.Next( 0, MobSfxCache.Confusion.Count - 1 ) ];
							}
							Bark.Play();

							float fear = (float)agent.State[ "Fear" ];
							fear += 5.0f; // make them just a tad bit scared
							agent.State[ "Fear" ] = fear;
							agent.State[ "Alert" ] = true;
							agent.State[ "TargetReached" ] = false;
							agent.State[ "TargetPosition" ] = SightTarget.GlobalPosition;
							agent.State[ "HasTarget" ] = true;
							agent.State[ "CanSeeTarget" ] = false;
						}
						return;
					}

					if ( IsDeadAI( sightTarget ) && sightTarget is MobBase ) {
						Agent other = ( (MobBase)sightTarget ).GetAgent();
						int nBodyCount = (int)agent.Memory[ "BodyCount" ];

						// "heavy down!"
						if ( (bool)other.State[ "IsHeavy" ] ) {
							Bark.Stream = MobSfxCache.HeavyDead[ RandomFactory.Next( 0, MobSfxCache.HeavyDead.Count - 1 ) ];
						}
						// "I need backup NOW!"
						else if ( Squad != null && Squad.GetNumSquadMembers() == 1 ) {
							// "he wiped out the whole squad!"
							if ( nBodyCount > 1 ) {
								Bark.Stream = MobSfxCache.SquadWiped;
							}
							// "I need backup NOW!"
							else {

							}
						}
						// "I've got two men down!"
						else if ( nBodyCount == 2 ) {
							Bark.Stream = MobSfxCache.ManDown2;
						}
						// "I've got three men down!"
						else if ( nBodyCount == 3 ) {
							Bark.Stream = MobSfxCache.ManDown3;
						}
						// "Man down!"
						else {
							Bark.Stream = MobSfxCache.ManDown[ RandomFactory.Next( 0, MobSfxCache.ManDown.Count - 1 ) ];
						}
						Bark.Play();
					}
				},
				name: "SeeDisturbance"
			)
		};

		Agent = new Agent(
			"MercenaryShotgunner",
			new ConcurrentDictionary<string, object>{
				{ "GuardPosition", GlobalPosition },
				{ "HasTarget", false },
				{ "TargetDistance", 0.0f },
				{ "TargetReached", false },
				{ "Fear", 0.0f },
				{ "Alert", false },
				{ "Scared", false },
				{ "CanSeeTarget", false },
				{ "SightPosition", Godot.Vector2.Zero }
			},
			memory: null,
			goals,
			actions,
			sensors
		);
		try {
			Agent.Plan();
		} catch ( System.NullReferenceException exception ) {
			GD.PushError( "Got an error when planning: " + exception.Message + "\n" + exception.StackTrace );
		}
	}

	private void SetDetectionColor() {
		DetectionColor.R = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
		DetectionColor.G = 0.0f;
		DetectionColor.B = 0.0f;
		DetectionColor.A = 1.0f;

		DetectionMeter.DefaultColor = DetectionColor;
	}

	private ExecutionStatus Action_CheckForTargets( Agent agent, MountainGoap.Action action ) {
		/*
		RecalcSight();

		CharacterBody2D sightTarget = null;
		for ( int i = 0; i < SightLines.Count; i++ ) {
			if ( SightLines[i].IsColliding() && SightLines[i].GetCollider() is Player ) {
				sightTarget = (CharacterBody2D)SightLines[i].GetCollider();
				break;
			}
		}

		// we haven't been alerted yet
		if ( !(bool)agent.State[ "Alert" ] ) {
			if ( SightTarget == null && sightTarget != null ) {
				// we haven't seen anything yet...
				SightTarget = sightTarget;
				SetDetectionColor();

				SightDetectionAmount += SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();
				agent.State[ "Alert" ] = true;

				return ExecutionStatus.Executing;
			}
		}
		else if ( SightTarget != null && sightTarget != null ) {
			// we still see a potential target
			SetDetectionColor();
			SightDetectionAmount += SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();
			
			// we've seen enough, FUCK 'EM UP!
			if ( SightDetectionAmount >= SightDetectionTime ) {
				Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.Next( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
				Bark.Play();

				agent.State[ "TargetReached" ] = false;
				agent.State[ "TargetPosition" ] = SightTarget.GlobalPosition;
				agent.State[ "HasTarget" ] = true;

				return ExecutionStatus.Succeeded;
			}
			return ExecutionStatus.Executing;
		}
		else if ( SightTarget != null && sightTarget == null ) {
			// we saw something, but it's not confirmed
			SetDetectionColor();

			if ( (float)agent.State[ "Fear" ] > 50.0f ) {
				Bark.Stream = MobSfxCache.Alert[ RandomFactory.Next( 0, MobSfxCache.Alert.Count - 1 ) ];
			} else {
				Bark.Stream = MobSfxCache.Confusion[ RandomFactory.Next( 0, MobSfxCache.Confusion.Count - 1 ) ];
			}
			Bark.Play();

			float fear = (float)agent.State[ "Fear" ];
			fear += 5.0f; // make them just a tad bit scared
			agent.State[ "Fear" ] = fear;
			agent.State[ "Alert" ] = true;
			agent.State[ "TargetReached" ] = false;
			agent.State[ "TargetPosition" ] = SightTarget.GlobalPosition;
			agent.State[ "HasTarget" ] = true;
			agent.State[ "CanSeeTarget" ] = false;

			return ExecutionStatus.Succeeded;
		}
		*/
		if ( (bool)agent.State[ "HasTarget" ] ) {
			return ExecutionStatus.Succeeded;
		}
		return ExecutionStatus.Executing;
	}

	public override void _Process( double delta ) {
		base._Process( delta );

		if ( (float)Agent.State[ "Fear" ] >= 80.0f ) {
			Bark.Stream = MobSfxCache.Curse[ RandomFactory.Next( 0, MobSfxCache.Curse.Count - 1 ) ];
			Bark.Play();
		}
		Agent.Step();
	}
};