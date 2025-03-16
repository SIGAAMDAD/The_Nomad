using Godot;
using MountainGoap;
using System.Collections.Generic;
using System.Collections.Concurrent;

// TODO: make alertness change based on target renown traits
public partial class MercenaryShotgunner : MobBase {
	private Timer TargetMovedTimer;
	private Timer HealTimer;
	private Timer AimTimer;
	private RayCast2D AimLine;
	private Line2D ShootLine;
	private AudioStreamPlayer2D HealSfx;
	private AudioStream AimSfx;

	public void Save() {
	}
	public void Load() {
	}
	
	protected override void OnLoseInterestTimerTimeout() {
		float fear = (float)Agent.State[ "Fear" ];
		
		Agent.State[ "Alert" ] = true;
		fear += 10.0f; // they're a little more on edge
		Agent.State[ "Fear" ] = fear;
		
		if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
			Agent.State[ "Awarensss" ] = Awareness.Relaxed;
		}
		// if we're not in power fantasy mode, the ai won't just shrug it off
		
		// SightTarget can ONLY EVER be assigned null here
		SightTarget = null;
		Navigation.TargetPosition = (Godot.Vector2)Agent.State[ "GuardPosition" ];
		Agent.State[ "TargetReached" ] = false;
		Agent.State[ "TargetDistance" ] = GlobalPosition.DistanceTo( Blackboard.GetGuardPosition() );
		Agent.State[ "Investigating" ] = false;
	}
	private void OnSoundBoundsBodyAreaEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return; // ignore
		}

		Player player = (Player)body;
		float soundLevel = player.GetSoundLevel();

		soundLevel -= GlobalPosition.DistanceTo( body.GlobalPosition );
		if ( soundLevel >= SoundDetectionLevel / 2.0f ) {
			Blackboard.SetAwareness( Awareness.Suspicious );
			Bark.Stream = AudioCache.Confusion[ RandomFactory.Next( 0, AudioCache.Confusion.Count - 1 ) ];
			Bark.Play();
		} else if ( soundLevel >= SoundDetectionLevel ) {
			Blackboard.SetAwareness( Awareness.Alert );
			Bark.Stream = AudioCache.Alert[ RandomFactory.Next( 0, AudioCache.Alert.Count - 1 ) ];
			Bark.Play();
		}
	}
	
	private void OnTargetMoveTimerTimeout() {
		// "target's pinned!"
		Bark.Stream = AudioCache.TargetPinned[ RandomFactory.Next( 0, AudioCache.TargetPinned.Count - 1 ) ];
		Bark.Play();
	}

	public override void _Ready() {
		base._Ready();

		Init();
		
		TargetMovedTimer = new Timer();
		TargetMovedTimer.WaitTime = 10.0f;
		TargetMovedTimer.OneShot = true;
		TargetMovedTimer.Connect( "timeout", Callable.From( OnTargetMoveTimerTimeout ) );

		Navigation.Connect( "target_reached", Callable.From( OnTargetReached ) );

		List<BaseGoal> goals = new List<BaseGoal>{
			new ExtremeGoal(
				name: "Survive",
				weight: 100.0f,
				desiredState: new Dictionary<string, bool>{
					{ "Health", true },
					{ "Fear", true }
				}
			),
			new Goal(
				name: "FindThreats",
				weight: 80.0f,
				desiredState: new Dictionary<string, object>{
					{ "HasTarget", true }
				}
			),
			new Goal(
				name: "EliminateThreats",
				weight: 88.0f,
				desiredState: new Dictionary<string, object>{
					{ "HasTarget", false },
					{ "TargetIsDead", true }
				}
			),
			new Goal(
				name: "Investigate",
				weight: 75.0f,
				desiredState: new Dictionary<string, object>{
					{ "HasTarget", true },
					{ "CanSeeTarget", true }
				}
			),
			new Goal(
				name: "GetToCover",
				weight: 90.0f,
				desiredState: new Dictionary<string, object>{
					{ "InCover", true }
				}
			)
		};
		List<MountainGoap.Action> actions = new List<MountainGoap.Action>{
			new MountainGoap.Action(
				name: "Stim",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					int nStims = (int)agent.State[ "Stims" ];
					if ( nStims == 0 ) {
						return ExecutionStatus.NotPossible;
					}
					if ( HealTimer.IsStopped() ) {
						HealTimer.Start();
						HealSfx.Play();
					} else if ( HealTimer.TimeLeft == 0.0f ) {
						agent.State[ "Stims" ] = nStims - 1;
						Health += 30.0f;
						return ExecutionStatus.Succeeded;
					}
					return ExecutionStatus.Executing;
				},
				cost: 10.0f,
				costCallback: null,
				preconditions: null,
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "Health", new ComparisonValuePair{ Value = 50.0f, Operator = ComparisonOperator.LessThan } },
					{ "Stims", new ComparisonValuePair{ Value = 0, Operator = ComparisonOperator.GreaterThan } }
				}
			),
			new MountainGoap.Action(
				name: "AimAtTarget",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => Action_AimAtTarget( agent, action ),
				cost: 30.0f,
				costCallback: ( MountainGoap.Action action, ConcurrentDictionary<string, object> currentState ) => {
					return GlobalPosition.DistanceTo( SightTarget.GlobalPosition );
				},
				preconditions: new Dictionary<string, object>{
					{ "HasTarget", true },
					{ "CanSeeTarget", true }
				},
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "TargetDistance", new ComparisonValuePair{ Value = MaxViewDistance, Operator = ComparisonOperator.LessThanOrEquals } }
				},
				postconditions: new Dictionary<string, object>{
					{ "HasTarget", false },
					{ "TargetIsDead", true }
				}
			),
			new MountainGoap.Action(
				name: "ShootAtTarget",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => Action_ShootAtTarget( agent, action ),
				cost: 28.0f,
				costCallback: null,
				preconditions: new Dictionary<string, object>{
					{ "HasTarget", true },
					{ "CanSeeTarget", true }
				},
				comparativePreconditions: null,
				postconditions: new Dictionary<string, object>{
					{ "HasTarget", false },
					{ "TargetIsDead", true }
				}
			),
			new MountainGoap.Action(
				name: "BlindFire",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => Action_BlindFire( agent, action ),
				cost: 35.0f,
				costCallback: null,
				preconditions: new Dictionary<string, object>{
					{ "InCover", true },
					{ "HasTarget", true }
				},
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "TargetDistance", new ComparisonValuePair{ Value = MaxViewDistance, Operator = ComparisonOperator.LessThanOrEquals } }
				},
				postconditions: null
			),
			new MountainGoap.Action(
				name: "ThrowGrenade",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					return ExecutionStatus.Succeeded;
				},
				cost: 20.0f,
				costCallback: null,
				preconditions: null,
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "Grenades", new ComparisonValuePair{ Value = 0, Operator = ComparisonOperator.GreaterThan } }
				},
				postconditions: null
			),
//			new MountainGoap.Action(
//				name: "FlankEnemy",
//				null,
//				executor: ( Agent agent, MountainGoap.Action action ) => Action_FlankEnemy( agent, action ),
//				cost: 0.0f,
//				costCallback: ()
//			),
			new MountainGoap.Action(
				name: "GuardNode",
				permutationSelectors: null,
				executor: ( Agent agent, MountainGoap.Action action ) => Action_CheckForTargets( agent, action ),
				cost: 0.0f,
				costCallback: null,
				preconditions: new Dictionary<string, object>{
					{ "HasTarget", false }
				},
				comparativePreconditions: null,
				postconditions: new Dictionary<string, object>{
					{ "HasTarget", true }
				}
			),
			new MountainGoap.Action(
				name: "GotoNode",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => Action_GotoNode( agent, action ),
				cost: 10.0f,
				null,
				preconditions: new Dictionary<string, object>{
					{ "TargetReached", false },
				},
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "TargetDistance", new ComparisonValuePair{ Value = 1.0f, Operator = ComparisonOperator.GreaterThan } }
				},
				postconditions: new Dictionary<string, object>{
					{ "TargetReached", true }
				}
			),
			new MountainGoap.Action(
				name: "InvestigateNode",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if ( !(bool)agent.State[ "CanSeeTarget" ] ) {
						if ( ChangeInvestigateAngleTimer.IsStopped() ) {
							if ( (float)agent.State[ "Fear" ] > 80.0f ) {
								// if we're scared, more JIGGLE
								ChangeInvestigateAngleTimer.WaitTime = 1.0f;
							}
							// occasionally jiggle the look angle
							ChangeInvestigateAngleTimer.Start();
						}
						
						if ( LoseInterestTimer.IsStopped() ) {
							// we can't see them, so begin the interest timer
							LoseInterestTimer.Start();
						} else if ( !(bool)agent.State[ "Investigating" ] ) {
							return ExecutionStatus.Failed;
						}
					}
					else {
						if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 0 ) { 
							// FIXME: is this too harsh?
							Bark.Stream = AudioCache.TargetSpotted[ RandomFactory.Next( 0, AudioCache.TargetSpotted.Count - 1 ) ];
							Bark.Play();
							agent.State[ "Target" ] = SightTarget;
							agent.PlanAsync();
							return ExecutionStatus.Succeeded;
						} else {
							Bark.Stream = AudioCache.Confusion[ RandomFactory.Next( 0, AudioCache.Confusion.Count - 1 ) ];
							Bark.Play();
							
							agent.State[ "LastTargetPosition" ] = SightTarget.GlobalPosition;
						}
					}
					return ExecutionStatus.Executing;
				},
				20.0f,
				null,
				preconditions: new Dictionary<string, object>{
					{ "HasTarget", false },
					{ "CanSeeTarget", false },
					{ "TargetReached", true }
				},
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "TargetDistance", new ComparisonValuePair{ Value = 1.0f, Operator = ComparisonOperator.LessThanOrEquals } }
				},
				postconditions: new Dictionary<string, object>{
					{ "HasTarget", true },
					{ "CanSeeTarget", true }
				}
			),
			new MountainGoap.Action(
				name: "RunAway",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if ( GlobalPosition == (Godot.Vector2)agent.State[ "GotoPosition" ] ) {
						return ExecutionStatus.Succeeded;
					}
					Action_GotoNode( agent, action );
					return ExecutionStatus.Executing;
				},
				cost: 2.0f,
				costCallback: null,
				preconditions: null,
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "Health", new ComparisonValuePair{ Value = 20.0f, Operator = ComparisonOperator.LessThan } }
				},
				postconditions: new Dictionary<string, object>{
					{ "Scared", false }
				}
			)
		};
		List<Sensor> sensors = new List<Sensor>{
			new Sensor(
				runCallback: ( Agent agent ) => {
				},
				name: "Scared"
			),
			new Sensor(
				runCallback: ( Agent agent ) => Sensor_Sight( agent ),
				name: "SeeDisturbance"
			),
			new Sensor(
				runCallback: ( Agent agent ) => {
					if ( SightTarget == null ) {
						return;
					} else if ( !(bool)agent.State[ "CanSeeTarget" ] ) {
						return;
					}
					TargetMovedTimer.Start();
				},
				name: "TargetMoved"
			)
		};
		
		Blackboard = new AIBlackboard();

		Agent = new Agent(
			"MercenaryShotgunner",
			new ConcurrentDictionary<string, object>{
				{ "HasTarget", false },
				{ "TargetDistance", 0.0f },
				{ "TargetReached", false },
				{ "Fear", 0.0f },
				{ "Alert", false },
				{ "Scared", false },
				{ "CanSeeTarget", false },
				{ "Stims", 2 },
				{ "SightPosition", Godot.Vector2.Zero },
				{ "Awareness", Awareness.Relaxed }
			},
			null,
			goals,
			actions,
			sensors
		);
		Agent.PlanAsync();
	}

	private void SetDetectionColor() {
		Awareness alertState = (Awareness)Agent.State[ "Awareness" ];
		
		if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
			// allow relaxation if we're in a power fantasy
			if ( SightDetectionAmount < SightDetectionTime * 0.5f ) {
				alertState = Awareness.Relaxed;
			} else if ( SightDetectionAmount < SightDetectionTime ) {
				alertState = Awareness.Suspicious;
			}
		}
		else {
			// once we're alert, no going back
			if ( alertState == Awareness.Suspicious || alertState == Awareness.Relaxed ) {
				if ( SightDetectionAmount < SightDetectionTime * 0.5f ) {
					alertState = Awareness.Relaxed;
				} else if ( SightDetectionAmount < SightDetectionTime ) {
					alertState = Awareness.Suspicious;
				}
			}
		}
		
		switch ( alertState ) {
		case Awareness.Relaxed:
			if ( SightDetectionAmount == 0.0f ) {
				DetectionColor.R = 1.0f;
				DetectionColor.G = 1.0f;
				DetectionColor.B = 1.0f;
			}
			else {
				// blue cuz colorblind people will struggle with the difference between suspicious and alert
				DetectionColor.R = 0.0f;
				DetectionColor.G = 0.0f;
				DetectionColor.B = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
			}
			break;
		case Awareness.Suspicious:
		case Awareness.Alert:
			DetectionColor.R = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
			DetectionColor.G = 0.0f;
			DetectionColor.B = 0.0f;
			break;
		default:
			break;
		};

		DetectionMeter.SetDeferred( "default_color", DetectionColor );
		Agent.State[ "Awareness" ] = alertState;
	}
	
#region API
	public override void _ExitTree() {
		base._ExitTree();

		HeadAnimations.QueueFree();
		ArmAnimations.QueueFree();
		BodyAnimations.QueueFree();
		DetectionMeter.QueueFree();
		LoseInterestTimer.QueueFree();
		Navigation.QueueFree();
		ThinkerTimer.QueueFree();
		Bark.QueueFree();
		
		for ( int i = 0; i < BloodSplatterTree.GetChildCount(); i++ ) {
			BloodSplatterTree.GetChild( i ).QueueFree();
			BloodSplatterTree.RemoveChild( SightDetector.GetChild( i ) );
		}
		BloodSplatterTree.QueueFree();
		
		for ( int i = 0; i < BulletShellTree.GetChildCount(); i++ ) {
			BulletShellTree.GetChild( i ).QueueFree();
			BulletShellTree.RemoveChild( SightDetector.GetChild( i ) );
		}
		BulletShellTree.QueueFree();
		
		for ( int i = 0; i < SightDetector.GetChildCount(); i++ ) {
			SightDetector.GetChild( i ).QueueFree();
			SightDetector.RemoveChild( SightDetector.GetChild( i ) );
		}
		SightDetector.QueueFree();
		
		for ( int i = 0; i < SightLines.Count; i++ ) {
			SightLines[i].QueueFree();
		}
		SightLines.Clear();
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		if ( SightTarget != null ) {
			MoveAlongPath();
			Navigation.TargetPosition = SightTarget.GlobalPosition;
		}
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 2 ) != 0 ) {
			return;
		}

		ProcessAnimations();

//		Agent.State[ "CanSeeTarget" ] = Blackboard.GetCanSeeTarget();
//		Agent.State[ "HasTarget" ] = Blackboard.GetTarget() != null;
//		Agent.State[ "Fear" ] = Blackboard.GetFear();
//		Agent.State[ "SightPosition" ] = Blackboard.GetSightPosition();
//		Agent.State[ "TargetReached" ] = Blackboard.GetTargetReached();
//		Agent.State[ "TargetDistance" ] = Blackboard.GetTargetDistance();

		Agent.Step();
	}
#endregion

#region Actions
	public ExecutionStatus Action_CheckForTargets( Agent agent, MountainGoap.Action action ) {
//		if ( (CharacterBody2D)agent.State[ "Target" ] != null ) {
//			return ExecutionStatus.Succeeded;
//		}
		return ExecutionStatus.Executing;
	}
	private ExecutionStatus Action_BlindFire( Agent agent, MountainGoap.Action action ) {
		ArmAnimations.Play( "blind_fire" );
		return ExecutionStatus.Succeeded;
	}
	protected ExecutionStatus Action_AimAtTarget( Agent agent, MountainGoap.Action action ) {
		if ( AimTimer.IsStopped() ) {
			AimTimer.Start();
			
			ArmAnimations.Play( "aim" );
			
			ActionSfx.Stream = AimSfx;
			ActionSfx.Play();
			
			ShootLine.Points[1] = (Godot.Vector2)agent.State[ "LastTargetPosition" ];
			ShootLine.Width = 0.0f;
			ShootLine.Show();
		}
		else if ( !(bool)agent.State[ "CanSeeTarget" ] ) {
			// can't see the target, don't waste ammunition
			
			// "he's running away!"
			Bark.Stream = AudioCache.TargetRunning[ RandomFactory.Next( 0, AudioCache.TargetRunning.Count - 1 ) ];
			Bark.Play();
			
			ShootLine.Hide();
			
			return ExecutionStatus.Failed;
		}
		else if ( AimTimer.TimeLeft == 0.0f ) {
			return ExecutionStatus.Succeeded;
		}
		
		ShootLine.Width = (float)Mathf.Lerp( 0.0f, 10.0f, AimTimer.TimeLeft );
		ShootLine.DefaultColor = new Color( (float)Mathf.Lerp( 1.0f, 0.05f, AimTimer.TimeLeft ), 0.0f, 0.0f, 1.0f );
		
		return ExecutionStatus.Executing;
	}
	private ExecutionStatus Action_ShootAtTarget( Agent agent, MountainGoap.Action action ) {
		GodotObject collider = AimLine.GetCollider();
		if ( collider == null ) {
			ActionSfx.Stream = AttackFirearmSfx;
			ActionSfx.Play();
			return ExecutionStatus.Failed;
		}
		if ( collider == SightTarget ) {
			ActionSfx.Stream = AttackFirearmSfx;
			ActionSfx.Play();
			
//			SightTarget.Damage( this, FirearmDamage );
			return ExecutionStatus.Succeeded;
		} else if ( collider is MobBase ) {
			// "GET OUT OF THE WAY!"
			Bark.Stream = AudioCache.OutOfTheWay[ RandomFactory.Next( 0, AudioCache.OutOfTheWay.Count - 1 ) ];
			Bark.Play();
		}
		return ExecutionStatus.Failed;
	}
#endregion

#region Sensors
	private void SetAlert( bool bRunning ) {
		if ( bRunning ) {
			// "He's getting away!"
			Bark.Stream = AudioCache.TargetRunning[ RandomFactory.Next( 0, AudioCache.TargetRunning.Count - 1 ) ];
		} else {
			// "there he is!'
			Bark.Stream = AudioCache.TargetSpotted[ RandomFactory.Next( 0, AudioCache.TargetSpotted.Count - 1 ) ];
		}
		Bark.Play();
		
		if ( (float)Agent.State[ "Fear" ] > 80.0f ) {
			// "shut the fuck up"
//			SequencedBark.Stream = AudioCache.Quiet[ RandomFactory.Next( 0, AudioCache.Quiet.Count - 1 ) ];
		}
		Agent.State[ "Target" ] = SightTarget;
	}
	private void SetSuspicious() {
		// "what was that?"
//		AISoundManager.RequestAISound( this, BarkType.Confusion );
		Bark.Stream = AudioCache.Confusion[ RandomFactory.Next( 0, AudioCache.Confusion.Count - 1 ) ];
		Bark.Play();
		
		SequencedBark.Stream = AudioCache.CheckItOut[ RandomFactory.Next( 0, AudioCache.CheckItOut.Count - 1 ) ];
		
		Agent.State[ "TargetReached" ] = false;
		Agent.State[ "TargetDistance" ] = GlobalPosition.DistanceTo( Blackboard.GetLastTargetPosition() );
	}
	
	private bool Investigate( GodotObject sightTarget ) {
		if ( sightTarget != null ) {
			// if we're running power fantasy mode, don't be as harsh
			if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
				SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();
			} else {
				// they're already suspicious, so double the alertness
				SightDetectionAmount += ( SightDetectionSpeed * 2.0f ) * (float)GetProcessDeltaTime();
			}
		} else if ( (bool)Agent.State[ "TargetReached" ] && GlobalPosition == Blackboard.GetLastTargetPosition()
			&& sightTarget == null )
		{
			// we're there, but no one's here... meh
			SightDetectionAmount -= SightDetectionSpeed * (float)GetProcessDeltaTime();
		}
		SetDetectionColor();

		if ( IsAlert() ) {
			SetAlert( sightTarget == null );
		} else if ( IsSuspicious() ) {
			SetSuspicious();
		}
		
		return true;
	}
	
	private void Sensor_Sight( Agent agent ) {
		GodotObject sightTarget = null;
		for ( int i = 0; i < SightLines.Count; i++ ) {
			sightTarget = SightLines[i].GetCollider();
			if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
				break;
			} else {
				sightTarget = null;
			}
		}
		
		/*
		if ( sightTarget is Grenade ) {
			Grenade data = (Grenade)sightTarget;
			if ( data.GlobalPosition.DistanceTo( GlobalPosition ) < data.GetRange() ) {
				// "grenade!"
				Bark.Stream = AudioCache.Grenade[ RandomFactory.Next( 0, AudioCache.Grenade.Count - 1 ) ];
				Bark.Play();
				
				if ( !(bool)agent.State[ "InCover" ] ) {
					agent.State[ "NeedCover" ] = true;
					agent.PlanAsync();
					return;
				}
			}
		}
		*/
		
		if ( Investigate( sightTarget ) ) {
			return;
		}
		
		// we got something, but they slipped out of view
		if ( SightDetectionAmount > 0.0f && sightTarget == null ) {
			// we still have some suspicion
			switch ( (Awareness)agent.State[ "Awareness" ] ) {
			case Awareness.Relaxed:
				// "must be nothing"
				if ( sightTarget == null ) {
					SightDetectionAmount -= SightDetectionSpeed * (float)GetProcessDeltaTime();
					if ( SightDetectionAmount <= 0.0f ) {
						SightDetectionAmount = 0.0f;
					}
				}
				break;
			case Awareness.Suspicious:
				SetSuspicious();
				break;
			case Awareness.Alert:
				SetAlert( true );
				break;
			};
			SetDetectionColor();
			
			// replan
			agent.PlanAsync();

			return;
		}
		
		SightTarget = (CharacterBody2D)sightTarget;
		if ( sightTarget is Player ) {
			if ( sightTarget != null ) {
				agent.State[ "LastTargetPosition" ] = SightTarget.GlobalPosition;
			}
			SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();
			
			if ( IsAlert() ) {
				SetAlert( false );
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			}
			SetDetectionColor();
			
			agent.PlanAsync();
		}
		else if ( sightTarget is MobBase ) {
			MobBase mob = (MobBase)SightTarget;
			Agent other = mob.GetAgent();
			
			// corpse?
			if ( IsDeadAI( SightTarget ) && !Blackboard.GetSeenBodies().Contains( mob ) ) {
				float fear = Blackboard.GetFear();
				fear += 10.0f;
				
				if ( Blackboard.GetAwareness() == Awareness.Relaxed ) {
					// if we're relaxed and suddenly... DEAD BODY!
					// make them a little more scared
					fear += 30.0f;
				}
				
				Blackboard.SetAwareness( Awareness.Alert );
				Blackboard.SetFear( fear );
				
				int nBodyCount = Blackboard.GetBodyCount();
				Blackboard.AddSeenBody( mob );
				
				if ( Squad != null && Squad.GetNumSquadMembers() == 1 ) {
					if ( nBodyCount > 1 ) {
						// "he wiped out the whole squad"
						Bark.Stream = AudioCache.SquadWiped;
					} else {
						// "I need backup now!"
						Bark.Stream = AudioCache.NeedBackup[ RandomFactory.Next( 0, AudioCache.NeedBackup.Count - 1 ) ];
					}
					
					float nAmount = 20.0f;
					if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
						nAmount = 70.0f;
					}
					if ( Randf( 0.0f, 100.0f ) < nAmount ) {
						SequencedBark.Stream = AudioCache.Unstoppable;
					}
				}
				else if ( nBodyCount == 2 ) {
					// "I've got two men down!"
					Bark.Stream = AudioCache.ManDown2;
					SequencedBark.Stream = AudioCache.Curse[ RandomFactory.Next( 0, AudioCache.Curse.Count - 1 ) ];
				}
				else if ( nBodyCount == 3 ) {
					// "I've got three men down!"
					Bark.Stream = AudioCache.ManDown3;
					SequencedBark.Stream = AudioCache.Curse[ RandomFactory.Next( 0, AudioCache.Curse.Count - 1 ) ];
				}
				else {
					// "MAN DOWN!"
					Bark.Stream = AudioCache.ManDown[ RandomFactory.Next( 0, AudioCache.ManDown.Count - 1 ) ];
					SequencedBark.Stream = AudioCache.Curse[ RandomFactory.Next( 0, AudioCache.Curse.Count - 1 ) ];
				}
				Bark.Play();
			}
		}
	}
#endregion
};