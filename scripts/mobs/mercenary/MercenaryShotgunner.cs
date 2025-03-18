using Godot;
using MountainGoap;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;

// TODO: make alertness change based on target renown traits
public partial class MercenaryShotgunner : MobBase {
	private Timer TargetMovedTimer;
	private Timer HealTimer;
	private Timer AimTimer;
	private RayCast2D AimLine;
	private Line2D ShootLine;
	private AudioStream AimSfx;

	public void Save() {
	}
	public void Load() {
	}
	
	protected override void OnLoseInterestTimerTimeout() {
//		float fear = Blackboard.GetFear();
		
		//Agent.State[ "Alert" ] = true;
//		fear += 10.0f; // they're a little more on edge
		Blackboard.SetFear( Blackboard.GetFear() + 10.0f );
		
		if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
			Blackboard.SetAwareness( Awareness.Relaxed );
		}
		// if we're not in power fantasy mode, the ai won't just shrug it off
		
		// SightTarget can ONLY EVER be assigned null here
		SightTarget = null;
		Navigation.TargetPosition = Blackboard.GetGuardPosition();
		SetNavigationTarget( Blackboard.GetGuardPosition() );
		Blackboard.SetInvestigating( false );
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
			Bark( BarkType.Confusion );
		} else if ( soundLevel >= SoundDetectionLevel ) {
			Blackboard.SetAwareness( Awareness.Alert );
			Bark( BarkType.Alert );
		}
	}
	
	private void OnTargetMoveTimerTimeout() {
		// "target's pinned!"
		Bark( BarkType.TargetPinned );
	}

	public override void _Ready() {
		base._Ready();

		Init();
		
		TargetMovedTimer = new Timer();
		TargetMovedTimer.WaitTime = 10.0f;
		TargetMovedTimer.OneShot = true;
		TargetMovedTimer.Connect( "timeout", Callable.From( OnTargetMoveTimerTimeout ) );
		AddChild( TargetMovedTimer );

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
			/*
			new Goal(
				name: "EliminateThreats",
				weight: 88.0f,
				desiredState: new Dictionary<string, object>{
					{ "TargetIsDead", true }
				}
			),
			new Goal(
				name: "Investigate",
				weight: 75.0f,
				desiredState: new Dictionary<string, object>{
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
			*/
		};
		List<MountainGoap.Action> actions = new List<MountainGoap.Action>{
			/*
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
//						HealSfx.Play();
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
					{ "CanSeeTarget", true },
					{ "IsAttackingTarget", true }
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
			*/
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
							Bark( BarkType.TargetSpotted );

							agent.State[ "Target" ] = SightTarget;
							agent.PlanAsync();
							return ExecutionStatus.Succeeded;
						} else {
							Bark( BarkType.Confusion );
							
							agent.State[ "LastTargetPosition" ] = SightTarget.GlobalPosition;
						}
					}
					return ExecutionStatus.Executing;
				},
				20.0f,
				null,
				preconditions: new Dictionary<string, object>{
					{ "CanSeeTarget", false },
					{ "TargetReached", false }
				},
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "TargetDistance", new ComparisonValuePair{ Value = 1.0f, Operator = ComparisonOperator.LessThanOrEquals } }
				},
				postconditions: new Dictionary<string, object>{
					{ "CanSeeTarget", true },
					{ "TargetReached", true }
				}
			),
			/*
			new MountainGoap.Action(
				name: "RunAway",
				null,
				executor: ( Agent agent, MountainGoap.Action action ) => {
					if ( Navigation.TargetPosition == GlobalPosition ) {
						SetNavigationTarget( (Godot.Vector2)agent.State[ "GotoPosition" ] );
					}
					return (bool)agent.State[ "TargetReached" ] ? ExecutionStatus.Succeeded : ExecutionStatus.Executing;
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
			*/
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
					if ( SightTarget == null || (bool)agent.State[ "CanSeeTarget" ] ) {
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
				{ "TargetReached", true },
				{ "Fear", 0.0f },
				{ "CanSeeTarget", false },
				{ "Stims", 2 },
				{ "SightPosition", Godot.Vector2.Zero },
				{ "Awareness", Awareness.Relaxed },
				{ "Investigating", false },
				{ "LastTargetPosition", Godot.Vector2.Zero }
			},
			null,
			goals,
			actions,
			sensors
		);

		Blackboard.SetGuardPosition( GlobalPosition );
		Blackboard.SetStims( 2 );
	}

	private void SetDetectionColor() {
		Awareness alertState = Blackboard.GetAwareness();
		
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
		Blackboard.SetAwareness( alertState );
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
		MoveChannel.QueueFree();
		BarkChannel.QueueFree();
		
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
		
		PhysicsPosition = GlobalPosition;
		if ( !Blackboard.GetTargetReached() ) {
			MoveAlongPath();
		}
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
			return;
		}

		ProcessAnimations();
		
		Agent.State[ "HasTarget" ] = Blackboard.GetHasTarget();
		Agent.State[ "TargetDistance" ] = Blackboard.GetTargetDistance();
		Agent.State[ "TargetReached" ] = Blackboard.GetTargetReached();
		Agent.State[ "Fear" ] = Blackboard.GetFear();
		Agent.State[ "CanSeeTarget" ] = Blackboard.GetCanSeeTarget();
		Agent.State[ "Stims" ] = Blackboard.GetStims();
		Agent.State[ "SightPosition" ] = Blackboard.GetSightPosition();
		Agent.State[ "Awareness" ] = Blackboard.GetAwareness();
		Agent.State[ "Investigating" ] = Blackboard.GetInvestigating();
		Agent.State[ "LastTargetPosition" ] = Blackboard.GetLastTargetPosition();

		Agent.Step( StepMode.OneAction );
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
			
			BarkChannel.Stream = AimSfx;
			BarkChannel.Play();
			
			ShootLine.Points[1] = (Godot.Vector2)agent.State[ "LastTargetPosition" ];
			ShootLine.Width = 0.0f;
			ShootLine.Show();
		}
		else if ( !(bool)agent.State[ "CanSeeTarget" ] ) {
			// can't see the target, don't waste ammunition
			
			// "he's running away!"
			Bark( BarkType.TargetRunning );
			
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
//			ActionSfx.Stream = AttackFirearmSfx;
//			ActionSfx.Play();
			return ExecutionStatus.Failed;
		}
		if ( collider == SightTarget ) {
//			ActionSfx.Stream = AttackFirearmSfx;
//			ActionSfx.Play();
			
//			SightTarget.Damage( this, FirearmDamage );
			return ExecutionStatus.Succeeded;
		} else if ( collider is MobBase ) {
			// "GET OUT OF THE WAY!"
			Bark( BarkType.OutOfTheWay );
		}
		return ExecutionStatus.Failed;
	}
#endregion

#region Sensors
	private void SetAlert( bool bRunning ) {
		if ( Blackboard.GetAwareness() != Awareness.Alert ) {
			if ( bRunning ) {
				// "He's getting away!"
				Bark( BarkType.TargetRunning );
			} else  {
				// "there he is!'
				Bark( BarkType.TargetSpotted );
			}
		}
		if ( Blackboard.GetFear() > 80.0f ) {
			// "shut the fuck up"
//			SequencedBarkChannel.Stream.Stream = AudioCache.Quiet[ RandomFactory.Next( 0, AudioCache.Quiet.Count - 1 ) ];
		}
		Blackboard.SetTarget( SightTarget );
		Blackboard.SetHasTarget( true );
		Blackboard.SetAwareness( Awareness.Alert );
	}
	private void SetSuspicious() {
		// "what was that?"
//		AISoundManager.RequestAISound( this, BarkChannel.StreamType.Confusion );
		if ( Blackboard.GetAwareness() != Awareness.Suspicious ) {
			Bark( BarkType.Confusion );
		}
		
//		SequencedBarkChannel.Stream.Stream = AudioCache.CheckItOut[ RandomFactory.Next( 0, AudioCache.CheckItOut.Count - 1 ) ];

		Blackboard.SetTargetReached( false );
		Blackboard.SetTargetDistance( PhysicsPosition.DistanceTo( Blackboard.GetLastTargetPosition() ) );
		Blackboard.SetAwareness( Awareness.Suspicious );
	}
	
	private bool Investigate( GodotObject sightTarget ) {
		if ( !Blackboard.GetInvestigating() ) {
//			return false;
		}

		if ( sightTarget != null ) {
			// if we're running power fantasy mode, don't be as harsh
			if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
				SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();
			} else {
				// they're already suspicious, so double the alertness
				SightDetectionAmount += ( SightDetectionSpeed * 2.0f ) * (float)GetProcessDeltaTime();
			}
		} else if ( Blackboard.GetTargetReached() && GlobalPosition == Blackboard.GetLastTargetPosition()
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

		if ( Blackboard.GetCanSeeTarget() ) {
			return false;
		}
		
		return true;
	}
	
	private void Sensor_Sight( Agent agent ) {
		if ( Blackboard.GetSightPosition() != GlobalPosition ) {
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
		
		/*
		if ( sightTarget is Grenade ) {
			Grenade data = (Grenade)sightTarget;
			if ( data.GlobalPosition.DistanceTo( GlobalPosition ) < data.GetRange() ) {
				// "grenade!"
				BarkChannel.Stream.Stream = AudioCache.Grenade[ RandomFactory.Next( 0, AudioCache.Grenade.Count - 1 ) ];
				BarkChannel.Stream.CallDeferred( "play" );
				
				if ( !(bool)agent.State[ "InCover" ] ) {
					agent.State[ "NeedCover" ] = true;
					agent.PlanAsync();
					return;
				}
			}
		}
		*/

		/*
		if ( IsSuspicious() ) {
			SetSuspicious();
			agent.State[ "Awareness" ] = Awareness.Suspicious;
		} else if ( IsAlert() ) {
			SetAlert( sightTarget == null );
			agent.State[ "Awareness" ] = Awareness.Alert;
		}
		*/

		if ( Investigate( sightTarget ) ) {
//			return;
		}
		
		// we got something, but they slipped out of view
		if ( SightDetectionAmount > 0.0f && sightTarget == null ) {
			agent.State[ "CanSeeTarget" ] = false;

			// we still have some suspicion
			switch ( Blackboard.GetAwareness() ) {
			case Awareness.Relaxed:
				// "must be nothing"
				if ( sightTarget == null ) {
					SightDetectionAmount -= SightDetectionSpeed;
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

			return;
		}
		
		SightTarget = (CharacterBody2D)sightTarget;
		if ( sightTarget is Player ) {
			if ( sightTarget != null ) {
				agent.State[ "LastTargetPosition" ] = SightTarget.GlobalPosition;
			}
			SightDetectionAmount += SightDetectionSpeed;

			agent.State[ "CanSeeTarget" ] = true;
			
			if ( IsAlert() ) {
				SetAlert( false );
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			}
			SetDetectionColor();
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
						Bark( BarkType.SquadWiped );
					} else {
						// "I need backup now!"
						Bark( BarkType.NeedBackup );
					}
					
					float nAmount = 20.0f;
					if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
						nAmount = 70.0f;
					}
					if ( Randf( 0.0f, 100.0f ) < nAmount ) {
						SequencedBark = AudioCache.Unstoppable;
//						BarkChannel.Play();
					}
				}
				else if ( nBodyCount == 2 ) {
					// "I've got two men down!"
					Bark( BarkType.MenDown2 );
				}
				else if ( nBodyCount == 3 ) {
					// "I've got three men down!"
					Bark( BarkType.MenDown3 );
				}
				else {
					// "MAN DOWN!"
					Bark( BarkType.ManDown );
				}
			}
		}
	}
#endregion
};