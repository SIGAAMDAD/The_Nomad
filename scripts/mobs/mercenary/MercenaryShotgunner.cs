using Godot;
using MountainGoap;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Linq.Expressions;

public partial class MercenaryShotgunner : MobBase {
	protected override void OnLoseInterestTimerTimeout() {
		float fear = (float)Agent.State[ "Fear" ];
		
		Agent.State[ "Alert" ] = true;
		fear += 10.0f; // they're a little more on edge
		Agent.State[ "Fear" ] = fear;

		SightTarget = null;
		Navigation.TargetPosition = (Godot.Vector2)Agent.State[ "GuardPosition" ];
	}
	private void OnSoundBoundsBodyAreaEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return; // ignore
		}

		Player player = (Player)body;
		float soundLevel = player.GetSoundLevel();

		soundLevel -= GlobalPosition.DistanceTo( body.GlobalPosition );
		if ( soundLevel >= SoundDetectionLevel / 2.0f ) {
			Agent.State[ "Awareness" ] = Awareness.Suspicious;
			Bark.Stream = MobSfxCache.Confusion[ RandomFactory.RandiRange( 0, MobSfxCache.Confusion.Count - 1 ) ];
			Bark.Play();
		} else if ( soundLevel >= SoundDetectionLevel ) {
			Agent.State[ "Awareness" ] = Awareness.Alert;
			Bark.Stream = MobSfxCache.Alert[ RandomFactory.RandiRange( 0, MobSfxCache.Alert.Count - 1 ) ];
			Bark.Play();
		}
	}

	public override void _Ready() {
		base._Ready();

		Init();

		Navigation.Connect( "target_reached", Callable.From( OnTargetReached ) );
		SoundBounds.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnSoundBoundsBodyAreaEntered ) );

		List<BaseGoal> goals = new List<BaseGoal>{
			new ComparativeGoal(
				"Survive",
				0.0f,
				desiredState: new Dictionary<string, ComparisonValuePair>{
					{ "Health", new ComparisonValuePair{ Value = 50.0f, Operator = ComparisonOperator.GreaterThan } },
					{ "Scared", new ComparisonValuePair{ Value = false, Operator = ComparisonOperator.Equals } }
				}
			),
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
					if ( (float)agent.State[ "Fear" ] > 80.0f ) {
						agent.State[ "Scared" ] = true;
					}
				},
				name: "Scared"
			),
			new Sensor(
				runCallback: async ( Agent agent ) => {
					if ( GlobalPosition != (Godot.Vector2)agent.State[ "SightPosition" ] ) {
						RecalcSight();
					}

					GodotObject sightTarget = null;
					for ( int i = 0; i < SightLines.Count; i++ ) {
						sightTarget = SightLines[i].GetCollider();
						if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
							break;
						} else {
							sightTarget = null;
						}
					}

					// no target
					if ( SightDetectionAmount > 0.0f ) {
						// we still have some suspicion
						switch ( (Awareness)agent.State[ "Awareness" ] ) {
						case Awareness.Relaxed:
							// "must be nothing"
							if ( sightTarget == null ) {
								SightDetectionAmount -= SightDetectionSpeed * (float)GetProcessDeltaTime();
								if ( SightDetectionAmount <= 0.0f ) {
									SightDetectionAmount = 0.0f;
								}
								agent.State[ "Awareness" ] = Awareness.Relaxed;
							}
							break;
						case Awareness.Suspicious:
							// "what was that?"
							break;
						case Awareness.Alert:
							// "He's getting away!"
							break;
						};
						SetDetectionColor();
					}

					SightTarget = (CharacterBody2D)sightTarget;
					if ( sightTarget is Player ) {
						SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();
						
						if ( SightDetectionAmount >= SightDetectionTime && (Awareness)agent.State[ "Awareness" ] != Awareness.Alert ) {
							Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.RandiRange( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
							Bark.Play();
							agent.State[ "Awareness" ] = Awareness.Alert;
						} else if ( SightDetectionAmount >= SightDetectionAmount / 2.0f && (Awareness)agent.State[ "Awareness" ] != Awareness.Suspicious ) {
							Bark.Stream = MobSfxCache.Confusion[ RandomFactory.RandiRange( 0, MobSfxCache.Confusion.Count - 1 ) ];
							Bark.Play();
							agent.State[ "Awareness" ] = Awareness.Suspicious;
						}

						SetDetectionColor();
					}
					else if ( sightTarget is MobBase ) {
						Agent other = ( (MobBase)SightTarget ).GetAgent();
						List<MobBase> seenBodies = (List<MobBase>)agent.Memory[ "SeenBodies" ];
						if ( IsDeadAI( SightTarget ) && !seenBodies.Contains( (MobBase)SightTarget ) ) {
							int nBodyCount = (int)other.Memory[ "BodyCount" ];

							if ( Squad != null && Squad.GetNumSquadMembers() == 1 ) {
								if ( nBodyCount > 1 ) {
									// "he wiped out the whole squad"
									Bark.Stream = MobSfxCache.SquadWiped;
								} else {
									// "I need backup now!"
								}
							}
							else if ( nBodyCount == 2 ) {
								// "I've got two men down!"
								Bark.Stream = MobSfxCache.ManDown2;
							}
							else if ( nBodyCount == 3 ) {
								// "I've got three men down!"
								Bark.Stream = MobSfxCache.ManDown3;
							}
							else {
								// "MAN DOWN!"
								Bark.Stream = MobSfxCache.ManDown[ RandomFactory.RandiRange( 0, MobSfxCache.ManDown.Count - 1 ) ];
							}
							Bark.Play();
						}
					}

					return;

					if ( sightTarget == null ) {
						SightDetectionAmount -= SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();
						if ( SightDetectionAmount <= 0.0f ) {
							SightDetectionAmount = 0.0f;
						}
						Agent.State[ "Alert" ] = false;
						SetDetectionColor();
					}
					else if ( !(bool)agent.State[ "Alert" ] && sightTarget != null && sightTarget is Player ) {
						// "What the fuck?"
						Bark.Stream = MobSfxCache.Alert[ RandomFactory.RandiRange( 0, MobSfxCache.Alert.Count - 1 ) ];
						Bark.Play();
						SightTarget = (CharacterBody2D)sightTarget;
						agent.State[ "Alert" ] = true;
					}
					if ( SightTarget != null && sightTarget != null ) {
						// we still see a potential target
						SetDetectionColor();
						SightDetectionAmount += SightDetectionSpeed * (float)GetPhysicsProcessDeltaTime();
			
						// we've seen enough, FUCK 'EM UP!
						if ( SightDetectionAmount >= SightDetectionTime ) {
							// "Target sighted!"
							Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.RandiRange( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
							Bark.Play();

							agent.State[ "TargetReached" ] = false;
							agent.State[ "TargetPosition" ] = SightTarget.GlobalPosition;
							agent.State[ "HasTarget" ] = true;
						}
					}
					else if ( SightTarget != null && sightTarget == null ) {
						// we saw something, but it's not confirmed
						SetDetectionColor();

						if ( (float)agent.State[ "Fear" ] > 50.0f ) {
							Bark.Stream = MobSfxCache.Alert[ RandomFactory.RandiRange( 0, MobSfxCache.Alert.Count - 1 ) ];
						} else {
							Bark.Stream = MobSfxCache.Confusion[ RandomFactory.RandiRange( 0, MobSfxCache.Confusion.Count - 1 ) ];
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
					else if ( sightTarget is Player ) {
						// got a player
						SightTarget = (CharacterBody2D)sightTarget;

						if ( !(bool)agent.State[ "Alert" ] ) {
							if ( SightTarget == null && sightTarget != null ) {
								// we haven't seen anything yet...
								SightTarget = (CharacterBody2D)sightTarget;
								SetDetectionColor();

								SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();
								agent.State[ "Alert" ] = true;
							}
						}
						else if ( sightTarget != null ) {
							// we still see a potential target
							SetDetectionColor();
							SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();

							// we've seen enough, FUCK 'EM UP!
							if ( SightDetectionAmount >= SightDetectionTime ) {
								Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.RandiRange( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
								Bark.Play();

								agent.State[ "TargetReached" ] = false;
								agent.State[ "TargetPosition" ] = SightTarget.GlobalPosition;
								agent.State[ "HasTarget" ] = true;
							}
						}
						else {
							// we saw something, but it's not confirmed
							SetDetectionColor();

							if ( (float)agent.State[ "Fear" ] > 50.0f ) {
								Bark.Stream = MobSfxCache.Alert[ RandomFactory.RandiRange( 0, MobSfxCache.Alert.Count - 1 ) ];
							} else {
								Bark.Stream = MobSfxCache.Confusion[ RandomFactory.RandiRange( 0, MobSfxCache.Confusion.Count - 1 ) ];
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
					}
					else if ( IsDeadAI( (CharacterBody2D)sightTarget ) && sightTarget is MobBase ) {
						Agent other = ( (MobBase)sightTarget ).GetAgent();
						int nBodyCount = (int)agent.Memory[ "BodyCount" ];

						// "heavy down!"
						if ( (bool)other.State[ "IsHeavy" ] ) {
							Bark.Stream = MobSfxCache.HeavyDead[ RandomFactory.RandiRange( 0, MobSfxCache.HeavyDead.Count - 1 ) ];
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
							Bark.Stream = MobSfxCache.ManDown[ RandomFactory.RandiRange( 0, MobSfxCache.ManDown.Count - 1 ) ];
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
				{ "Awareness", Awareness.Relaxed },
				{ "SightPosition", Godot.Vector2.Zero }
			},
			null,
			goals,
			actions,
			sensors
		);
		Agent.PlanAsync();
	}

	private void SetDetectionColor() {
		Awareness alertState = Awareness.Invalid;

//		if ( SightDetectionAmount >= SightDetectionTime ) {
//			alertState = Awareness.Alert;
//		} else if ( SightDetectionAmount >= SightDetectionTime / 2.0f ) {
//			alertState = Awareness.Suspicious;
//		} else if ( SightDetectionAmount < 1.0f ) {
//			alertState = Awareness.Relaxed;
//		}

		switch ( (Awareness)Agent.State[ "Awareness" ] ) {
		case Awareness.Relaxed:
			if ( SightDetectionAmount == 0.0f ) {
				DetectionColor.R = 1.0f;
				DetectionColor.G = 1.0f;
				DetectionColor.B = 1.0f;
			} else {
				// blue cuz colorblind people will struggle with the difference between suspicious and alert
				DetectionColor.R = 0.0f;
				DetectionColor.G = 0.0f;
				DetectionColor.B = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
			}
			break;
		case Awareness.Alert:
		case Awareness.Suspicious:
			DetectionColor.R = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
			DetectionColor.G = 0.0f;
			DetectionColor.B = 0.0f;
			break;
		default:
			break;
		};

		DetectionMeter.DefaultColor = DetectionColor;
		Agent.State[ "Awareness" ] = alertState;
	}

	private ExecutionStatus Action_CheckForTargets( Agent agent, MountainGoap.Action action ) {
		if ( (bool)agent.State[ "HasTarget" ] ) {
			return ExecutionStatus.Succeeded;
		}
		return ExecutionStatus.Executing;
	}

	public override void _ExitTree() {
		base._ExitTree();

		Animations.QueueFree();
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		if ( SightTarget != null ) {
			Navigation.TargetPosition = SightTarget.GlobalPosition;
			NextPathPosition = Navigation.GetNextPathPosition();
			MoveAlongPath();
		}
	}
    public override void _Process( double delta ) {
		base._Process( delta );

		if ( (float)Agent.State[ "Fear" ] >= 80.0f ) {
			Bark.Stream = MobSfxCache.Curse[ RandomFactory.RandiRange( 0, MobSfxCache.Curse.Count - 1 ) ];
			Bark.Play();
		}
		Agent.Step();
	}
};