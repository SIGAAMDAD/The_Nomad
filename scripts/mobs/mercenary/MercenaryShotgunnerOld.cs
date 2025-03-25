/*
using Godot;
using MountainGoap;
using System.Collections.Generic;

// TODO: make alertness change based on target renown traits
public partial class MercenaryShotgunnerOld : MobBase {
	[Export]
	private NavigationLink2D PatrolRoute;

	private Timer TargetMovedTimer;
	private Timer HealTimer;
	private Timer AimTimer;
	private RayCast2D AimLine;
	private Line2D ShootLine;
	private AudioStream AimSfx;

	private static readonly BaseGoal[] Goals = {
		new ExtremeGoal(
			name: "Survive",
			weight: 100.0f,
			desiredState: new Dictionary<string, bool>{
				{ "Health", true }
			}
		),
		new Goal(
			name: "GuardLocation",
			weight: 80.0f,
			desiredState: new Dictionary<string, object>{
				{ "CanSeeTarget", false },
				{ "HasTarget", false }
			}
		),
		new Goal(
			name: "InvestigateDisturbance",
			weight: 82.0f,
			desiredState: new Dictionary<string, object>{
				{ "CanSeeTarget", true },
				{ "HasTarget", true },
				{ "Investigating", true }
			}
		)
	};
	
	public void Save() {
	}
	public void Load() {
	}
	
	protected override void OnLoseInterestTimerTimeout() {
		Blackboard.SetLostInterest( true );
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

	private void SendPacket() {
	}
	private void ReceivePacket( System.IO.BinaryReader reader ) {
		Godot.Vector2 position = Godot.Vector2.Zero;
		position.X = (float)reader.ReadDouble();
		position.Y = (float)reader.ReadDouble();
		GlobalPosition = position;
	}
	
#region API
	public override void _Ready() {
		base._Ready();

		Init();
		
		TargetMovedTimer = new Timer();
		TargetMovedTimer.WaitTime = 10.0f;
		TargetMovedTimer.OneShot = true;
		TargetMovedTimer.Connect( "timeout", Callable.From( OnTargetMoveTimerTimeout ) );
		AddChild( TargetMovedTimer );
		
		Action[] actions = {
			new Action(
				name: "GuardNode",
				permutationSelectors: null,
				executor: ( Agent agent, Action action ) => {
					if ( Blackboard.GetCanSeeTarget() ) {
						GD.Print( "INVESTIGATING" );
						Blackboard.SetInvestigating( true );
						Blackboard.SetHasTarget( true );
						SetNavigationTarget( Blackboard.GetLastTargetPosition() );
						SetSuspicious();
						return ExecutionStatus.Succeeded;
					}
					Blackboard.SetHasTarget( false );
					Blackboard.SetCanSeeTarget( false );
					return ExecutionStatus.Executing;
				},
				cost: 0.0f,
				costCallback: null,
				preconditions: new Dictionary<string, object>{
					{ "HasTarget", false },
					{ "Investigating", false },
					{ "CanSeeTarget", false }
				},
				postconditions: new Dictionary<string, object>{
					{ "Investigating", true },
					{ "CanSeeTarget", false },
					{ "HasTarget", false }
				},
				arithmeticPostconditions: new Dictionary<string, object>{
					{ "Awareness", Awareness.Suspicious }
				}
			),
			new Action(
				name: "GotoNode",
				permutationSelectors: null,
				executor: ( Agent agent, Action action ) => {
					GD.Print( "Moving to node..." );
					return Navigation.IsTargetReached() ? ExecutionStatus.Succeeded : Navigation.IsTargetReachable() ? ExecutionStatus.Executing : ExecutionStatus.NotPossible;
				},
				cost: 0.0f,
				costCallback: null,
				preconditions: new Dictionary<string, object>{
					{ "TargetReached", false }
				},
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "TargetDistance", new ComparisonValuePair{ Value = 10.0f, Operator = ComparisonOperator.GreaterThanOrEquals } }
				},
				postconditions: new Dictionary<string, object>{
					{ "TargetReached", true }
				}
			),
			new Action(
				name: "InvestigateNode",
				permutationSelectors: null,
				executor: ( Agent agent, Action action ) => {
					GD.Print( "Investigating..." );
					Blackboard.SetInvestigating( true );
					if ( Blackboard.GetCanSeeTarget() ) {
						GD.Print( "...Target in sight" );
						LoseInterestTimer.Stop();
						Blackboard.SetLostInterest( false );
						SightDetectionAmount += SightDetectionSpeed * (float)GetProcessDeltaTime();
						Blackboard.SetHasTarget( true );
						return IsAlert() ? ExecutionStatus.Succeeded : ExecutionStatus.Executing;
					} else {
						if ( LoseInterestTimer.IsStopped() ) {
							GD.Print( "...Losing interest" );
							LoseInterestTimer.Start();
						}
						if ( Blackboard.GetLostInterest() ) {
							GD.Print( "...Lost interest" );
							Blackboard.SetInvestigating( false );
							return ExecutionStatus.Failed;
						}
						if ( ChangeInvestigateAngleTimer.IsStopped() ) {
							ChangeInvestigateAngleTimer.Start();
						}
					}
					return ExecutionStatus.Executing;
				},
				cost: 0.0f,
				costCallback: ( Action action, Dictionary<string, object> currentState ) => {
					return Blackboard.GetAwareness() >= Awareness.Suspicious ? 10.0f : 30.0f;
				},
				preconditions: new Dictionary<string, object>{
					{ "Investigating", true },
					{ "TargetReached", true },
					{ "CanSeeTarget", true }
				},
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "Awareness", new ComparisonValuePair{ Value = Awareness.Suspicious, Operator = ComparisonOperator.Equals } }
				},
				postconditions: new Dictionary<string, object>{
					{ "CanSeeTarget", true },
					{ "HasTarget", true }
				}
			)
		};
		Sensor[] sensors = {
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
					if ( SightTarget == null || Blackboard.GetCanSeeTarget() ) {
						return;
					}
					TargetMovedTimer.Start();
				},
				name: "TargetMoved"
			),
			new Sensor(
				runCallback: ( Agent agent ) => {
					return;
					if ( PatrolRoute == null ) {
						return;
					}
					if ( PhysicsPosition.DistanceTo( PatrolRoute.GetGlobalEndPosition() ) <= 10.0f ) {
						SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
					} else if ( PhysicsPosition.DistanceTo( PatrolRoute.GetGlobalStartPosition() ) <= 10.0f ) {
						SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
					}
				},
				name: "Patrol"
			)
		};
		
		Blackboard = new AIBlackboard();

		Agent = new Agent(
			"MercenaryShotgunner",
			new Dictionary<string, object>{
				{ "HasTarget", false },
				{ "TargetDistance", 0.0f },
				{ "TargetReached", true },
				{ "Fear", 0.0f },
				{ "CanSeeTarget", false },
				{ "Stims", 2 },
				{ "SightPosition", Godot.Vector2.Zero },
				{ "Awareness", Awareness.Relaxed },
				{ "Investigating", false },
				{ "LoseInterest", true },
				{ "LastTargetPosition", Godot.Vector2.Zero },
				{ "PatrolRoute", PatrolRoute != null }
			},
			null,
			Goals,
			actions,
			sensors
		);

		Blackboard.SetGuardPosition( GlobalPosition );
		Blackboard.SetStims( 2 );
		Blackboard.SetGotoPosition( GlobalPosition );

//		if ( PatrolRoute != null ) {
//			SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
//		}
		
		DemonEyeColor = new Color();
		DemonEyeColor.R = 0.0f;
		DemonEyeColor.G = 0.0f;
		DemonEyeColor.B = 1.0f;
		DemonEyeColor.A = 1.0f;
		DemonEyeColor.R8 = 0;
		DemonEyeColor.G8 = 0;
		DemonEyeColor.B8 = 2;

		SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, SendPacket, ReceivePacket ) );
	}
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
		
		for ( int i = 0; i < SightLines.Length; i++ ) {
			SightLines[i].QueueFree();
		}
	}
#endregion

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
				DetectionColor.G = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
				DetectionColor.B = 0.0f;
			}
			break;
		case Awareness.Suspicious:
			DetectionColor.R = 0.0f;
			DetectionColor.G = 0.0f;
			DetectionColor.B = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
			break;
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

#region Actions
	private ExecutionStatus Action_CheckForTargets( Agent agent, Action action ) {
		 if ( Blackboard.GetTarget() != null || Blackboard.GetAwareness() > Awareness.Relaxed ) {
			return ExecutionStatus.Succeeded;
		}
		return ExecutionStatus.Executing;
	}
	private ExecutionStatus Action_BlindFire( Agent agent, Action action ) {
		ArmAnimations.Play( "blind_fire" );
		return ExecutionStatus.Succeeded;
	}
	protected ExecutionStatus Action_AimAtTarget( Agent agent, Action action ) {
		if ( AimTimer.IsStopped() ) {
			AimTimer.Start();
			
			ArmAnimations.Play( "aim" );
			BodyAnimations.Play( "aim" );
			
			AudioChannel.Stream = AimSfx;
			AudioChannel.Play();
			
			ShootLine.Points[1] = SightTarget.GlobalPosition;
			ShootLine.Width = 0.0f;
			ShootLine.Show();
		}
		else if ( !Blackboard.GetCanSeeTarget() ) {
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
	private ExecutionStatus Action_ShootAtTarget( Agent agent, Action action ) {
		GodotObject collider = AimLine.GetCollider();
		if ( collider == SightTarget ) {
			AudioChannel.Stream = AttackFirearmSfx;
			AudioChannel.Play();
			
			SightTarget.Call( "Damage", this, FirearmDamage );
			return ExecutionStatus.Succeeded;
		} else if ( collider is MobBase ) {
			// "GET OUT OF THE WAY!"
			Bark( BarkType.OutOfTheWay );
		}
		return ExecutionStatus.Failed;
	}
	private ExecutionStatus Action_InvestigateNode( Agent agent, Action action ) {
		GD.Print( "Investigating" );
		if ( LoseInterestTimer.IsStopped() ) {
			LoseInterestTimer.Start();
		}
		if ( !Blackboard.GetCanSeeTarget() ) {
			if ( ChangeInvestigateAngleTimer.IsStopped() ) {
				if ( Blackboard.GetFear() > 80.0f ) {
					// if we're scared, more JIGGLE
					ChangeInvestigateAngleTimer.WaitTime = 0.6f;
				}
				// occasionally jiggle the look angle
				ChangeInvestigateAngleTimer.Start();
			}
			
			if ( LoseInterestTimer.TimeLeft == 0.0f ) {
				return ExecutionStatus.Failed;
			}
		}
		else {
			if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 0 ) { 
				// FIXME: is this too harsh?
//				Bark( BarkType.TargetSpotted );
//				
//				Blackboard.SetAwareness( Awareness.Alert );
//				Blackboard.SetTarget( SightTarget );
				
				if ( IsAlert() ) {
					return ExecutionStatus.Succeeded;
				}
				return ExecutionStatus.Executing;
			} else {
				Bark( BarkType.Confusion );
				CreateAfterImage();
				
				LoseInterestTimer.Stop();
			}
		}
		return ExecutionStatus.Executing;
	} 
#endregion

#region Sensors
	private void SetAlert( bool bRunning ) {
		// "He's getting away!" or "Target spotted!"
		Bark( bRunning ? BarkType.TargetRunning : BarkType.TargetSpotted,
			Blackboard.GetFear() > 80.0f ? BarkType.Quiet : BarkType.Count
		);
		SetNavigationTarget( SightTarget.GlobalPosition );
		Blackboard.SetTarget( SightTarget );
		Blackboard.SetHasTarget( true );
		Blackboard.SetAwareness( Awareness.Alert );
	}
	private void SetSuspicious() {
		// "what was that?"
		Bark( BarkType.Confusion );
		Blackboard.SetAwareness( Awareness.Suspicious );
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
		} else if ( Blackboard.GetTargetReached() && GlobalPosition == Blackboard.GetLastTargetPosition()
			&& sightTarget == null )
		{
			// we're there, but no one's here... meh
			SightDetectionAmount -= SightDetectionSpeed * (float)GetProcessDeltaTime();
		}
		SetDetectionColor();

		if ( Blackboard.GetCanSeeTarget() ) {
			return false;
		}
		
		return true;
	}
	
	private void Sensor_Sight( Agent agent ) {
		RecalcSight();

		GodotObject sightTarget = null;
		for ( int i = 0; i < SightLines.Length; i++ ) {
			sightTarget = SightLines[i].GetCollider();
			if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
				break;
			} else {
				sightTarget = null;
			}
		}
		
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

		Investigate( sightTarget );
		
		// we got something, but they slipped out of view
		if ( SightDetectionAmount > 0.0f && sightTarget == null ) {
			Blackboard.SetCanSeeTarget( true );
			
			CreateAfterImage();

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
		
		SightTarget = sightTarget as Player;
		if ( SightTarget != null ) {
			SightDetectionAmount += SightDetectionSpeed;
			Blackboard.SetLastTargetPosition( SightTarget.GlobalPosition );
//			Blackboard.SetCanSeeTarget( true );
			
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
					float nAmount = 20.0f;
					if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 1 ) {
						nAmount = 70.0f;
					}
					
					BarkType sequenced = Randf( 0.0f, 100.0f ) < nAmount ? BarkType.Unstoppable : BarkType.Count;
					if ( nBodyCount > 1 ) {
						// "he wiped out the whole squad"
						Bark( BarkType.SquadWiped, sequenced );
					} else {
						// "I need backup now!"
						Bark( BarkType.NeedBackup, sequenced );
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
*/