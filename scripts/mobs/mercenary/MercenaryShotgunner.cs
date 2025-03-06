using Godot;
using MountainGoap;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Linq.Expressions;


// TODO: make alertness change based on target renown traits
public partial class MercenaryShotgunner : MobBase {
	private Timer TargetMovedTimer;
	private Timer HealTimer;
	private Timer AimTimer;
	private RayCast2D AimLine;
	private Line2D ShootLine;
	private AudioStreamPlayer2D HealSfx;
	private AudioStream AimSfx;
	
	protected override void OnLoseInterestTimerTimeout() {
		float fear = (float)Agent.State[ "Fear" ];
		
		Agent.State[ "Alert" ] = true;
		fear += 10.0f; // they're a little more on edge
		Agent.State[ "Fear" ] = fear;
		
		if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
			Blackboard.SetAwareness( Awareness.Relaxed );
		}
		// if we're not in power fantasy mode, the ai won't just shrug it off
		
		// SightTarget can ONLY EVER be assigned null here
		SightTarget = null;
		Navigation.TargetPosition = Blackboard.GetGuardPosition();
		Agent.State[ "TargetReached" ] = false;
		Agent.State[ "TargetDistance" ] = GlobalPosition.DistanceTo( Blackboard.GetGuardPosition() );
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
			Bark.Stream = MobSfxCache.Confusion[ RandomFactory.RandiRange( 0, MobSfxCache.Confusion.Count - 1 ) ];
			Bark.Play();
		} else if ( soundLevel >= SoundDetectionLevel ) {
			Blackboard.SetAwareness( Awareness.Alert );
			Bark.Stream = MobSfxCache.Alert[ RandomFactory.RandiRange( 0, MobSfxCache.Alert.Count - 1 ) ];
			Bark.Play();
		}
	}
	
	private void OnTargetMoveTimerTimeout() {
		// "target's pinned!"
		Bark.Stream = MobSfxCache.TargetPinned[ RandomFactory.RandiRange( 0, MobSfxCache.TargetPinned.Count - 1 ) ];
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
		SoundBounds.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnSoundBoundsBodyAreaEntered ) );

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
							if ( (bool)agent.State[ "Scared" ] ) {
								// if we're scared, more JIGGLE
								ChangeInvestigateAngleTimer.WaitTime = 1.0f;
							}
							// occasionally jiggle the look angle
							ChangeInvestigateAngleTimer.Start();
						}
						
						if ( LoseInterestTimer.IsStopped() ) {
							// we can't see them, so begin the interest timer
							LoseInterestTimer.Start();
						} else if ( !Blackboard.GetInvestigating() ) {
							return ExecutionStatus.Failed;
						}
					}
					else {
						if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 0 ) { 
							// FIXME: is this too harsh?
							Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.RandiRange( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
							Bark.Play();
							Blackboard.SetTarget( SightTarget );
							agent.PlanAsync();
							return ExecutionStatus.Succeeded;
						} else {
							Bark.Stream = MobSfxCache.Confusion[ RandomFactory.RandiRange( 0, MobSfxCache.Confusion.Count - 1 ) ];
							Bark.Play();
							
							Blackboard.SetLastTargetPosition( SightTarget.GlobalPosition );
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
					if ( GlobalPosition == (Godot.Vector2)agent.State[ "TargetPosition" ] ) {
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
					if ( (float)agent.State[ "Fear" ] > 80.0f ) {
						agent.State[ "Scared" ] = true;
					}
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

		DetectionMeter.DefaultColor = DetectionColor;
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
		SoundBounds.QueueFree();
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
#endregion

#region Actions
	public ExecutionStatus Action_CheckForTargets( Agent agent, MountainGoap.Action action ) {
		if ( (bool)agent.State[ "HasTarget" ] ) {
			return ExecutionStatus.Succeeded;
		}
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
			
			ShootLine.Points[1] = Blackboard.GetLastTargetPosition();
			ShootLine.Width = 0.0f;
			ShootLine.Show();
		}
		else if ( !(bool)agent.State[ "CanSeeTarget" ] ) {
			// can't see the target, don't waste ammunition
			
			// "he's running away!"
			Bark.Stream = MobSfxCache.TargetRunning[ RandomFactory.RandiRange( 0, MobSfxCache.TargetRunning.Count - 1 ) ];
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
			Bark.Stream = MobSfxCache.OutOfTheWay[ RandomFactory.RandiRange( 0, MobSfxCache.OutOfTheWay.Count - 1 ) ];
			Bark.Play();
		}
		return ExecutionStatus.Failed;
	}
#endregion

#region Sensors
	private void SetAlert( bool bRunning ) {
		if ( bRunning ) {
			// "He's getting away!"
			Bark.Stream = MobSfxCache.TargetRunning[ RandomFactory.RandiRange( 0, MobSfxCache.TargetRunning.Count - 1 ) ];
		} else {
			// "there he is!'
			Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.RandiRange( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
		}
		Bark.Play();
		
		if ( (bool)Agent.State[ "Scared" ] ) {
			// "shut the fuck up"
//			SequencedBark.Stream = MobSfxCache.Quiet[ RandomFactory.RandiRange( 0, MobSfxCache.Quiet.Count - 1 ) ];
		}
		Agent.State[ "HasTarget" ] = true;
		Blackboard.SetTarget( SightTarget );
	}
	private void SetSuspicious() {
		// "what was that?"
//		AISoundManager.RequestAISound( this, BarkType.Confusion );
		Bark.Stream = MobSfxCache.Confusion[ RandomFactory.RandiRange( 0, MobSfxCache.Confusion.Count - 1 ) ];
		Bark.Play();
		
		SequencedBark.Stream = MobSfxCache.CheckItOut[ RandomFactory.RandiRange( 0, MobSfxCache.CheckItOut.Count - 1 ) ];
		
		Agent.State[ "TargetReached" ] = false;
		Agent.State[ "TargetDistance" ] = GlobalPosition.DistanceTo( Blackboard.GetLastTargetPosition() );
	}
	
	private bool Investigate( GodotObject sightTarget ) {
		if ( !Blackboard.GetInvestigating() ) {
			return false;
		}
		
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
		
		return true;
	}
	
	private void Sensor_Sight( Agent agent ) {
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
		
		/*
		if ( sightTarget is Grenade ) {
			Grenade data = (Grenade)sightTarget;
			if ( data.GlobalPosition.DistanceTo( GlobalPosition ) < data.GetRange() ) {
				// "grenade!"
				Bark.Stream = MobSfxCache.Grenade[ RandomFactory.RandiRange( 0, MobSfxCache.Grenade.Count - 1 ) ];
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
		if ( SightDetectionAmount > 0.0f && SightTarget != null && sightTarget == null ) {
			// we still have some suspicion
			switch ( Blackboard.GetAwareness() ) {
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
				Blackboard.SetLastTargetPosition( SightTarget.GlobalPosition );
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
				float fear = (float)agent.State[ "Fear" ];
				fear += 10.0f;
				
				if ( Blackboard.GetAwareness() == Awareness.Relaxed ) {
					// if we're relaxed and suddenly... DEAD BODY!
					// make them a little more scared
					fear += 30.0f;
				}
				
				Blackboard.SetAwareness( Awareness.Alert );
				agent.State[ "Fear" ] = fear;
				
				int nBodyCount = Blackboard.GetBodyCount();
				Blackboard.AddSeenBody( mob );
				
				if ( Squad != null && Squad.GetNumSquadMembers() == 1 ) {
					if ( nBodyCount > 1 ) {
						// "he wiped out the whole squad"
						Bark.Stream = MobSfxCache.SquadWiped;
					} else {
						// "I need backup now!"
						Bark.Stream = MobSfxCache.NeedBackup;
					}
					
					float nAmount = 20.0f;
					if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
						nAmount = 70.0f;
					}
					if ( RandomFactory.RandfRange( 0.0f, 100.0f ) < nAmount ) {
						SequencedBark.Stream = MobSfxCache.Unstoppable;
					}
				}
				else if ( nBodyCount == 2 ) {
					// "I've got two men down!"
					Bark.Stream = MobSfxCache.ManDown2;
					SequencedBark.Stream = MobSfxCache.Curse[ RandomFactory.RandiRange( 0, MobSfxCache.Curse.Count - 1 ) ];
				}
				else if ( nBodyCount == 3 ) {
					// "I've got three men down!"
					Bark.Stream = MobSfxCache.ManDown3;
					SequencedBark.Stream = MobSfxCache.Curse[ RandomFactory.RandiRange( 0, MobSfxCache.Curse.Count - 1 ) ];
				}
				else {
					// "MAN DOWN!"
					Bark.Stream = MobSfxCache.ManDown[ RandomFactory.RandiRange( 0, MobSfxCache.ManDown.Count - 1 ) ];
					SequencedBark.Stream = MobSfxCache.Curse[ RandomFactory.RandiRange( 0, MobSfxCache.Curse.Count - 1 ) ];
				}
				Bark.Play();
			}
		}
	}
#endregion
};