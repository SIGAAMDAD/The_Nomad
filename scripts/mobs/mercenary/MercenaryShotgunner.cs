using Godot;
using MountainGoap;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;

// TODO: make alertness change based on target renown traits
public partial class MercenaryShotgunner : MobBase {
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
			name: "FindThreats",
			weight: 80.0f,
			desiredState: new Dictionary<string, object>{
				{ "HasTarget", true }
			}
		),
		/*
		new Goal(
			name: "InvestigateDisturbance",
			weight: 82.0f,
			desiredState: new Dictionary<string, object>{
				{ "HasTarget", true },
				{ "CanSeeTarget", true },
				{ "TargetReached", true }
			}
		),
		new Goal(
			name: "AttackThreats",
			weight: 90.0f,
			desiredState: new Dictionary<string, object>{
				{ "HasTarget", false }
			}
		),
		new Goal(
			name: "GetToCover",
			weight: 84.0f,
			desiredState: new Dictionary<string, object>{
				{ "InCover", true },
				{ "TargetReached", true }
			}
		),
		*/
	};
	
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

	private void SendPacket() {
	}
	private void ReceivePacket( System.IO.BinaryReader reader ) {
		Godot.Vector2 position = Godot.Vector2.Zero;
		position.X = reader.ReadInt32();
		GlobalPosition = position;
	}
	
#region API
	private Stopwatch profiler = new Stopwatch();

	public override void _Ready() {
		base._Ready();

		Init();
		
		TargetMovedTimer = new Timer();
		TargetMovedTimer.WaitTime = 10.0f;
		TargetMovedTimer.OneShot = true;
		TargetMovedTimer.Connect( "timeout", Callable.From( OnTargetMoveTimerTimeout ) );
		AddChild( TargetMovedTimer );
		
		MountainGoap.Action[] actions = {
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
				permutationSelectors: null,
				executor: ( Agent agent, MountainGoap.Action action ) => Action_InvestigateNode( agent, action ),
				cost: 20.0f,
				costCallback: ( MountainGoap.Action action, Dictionary<string, object> currentState ) => {
					return Blackboard.GetAwareness() > Awareness.Suspicious ? 10.0f : 30.0f;
				},
				preconditions: new Dictionary<string, object>{
					{ "CanSeeTarget", false },
					{ "TargetReached", true }
				},
				comparativePreconditions: new Dictionary<string, ComparisonValuePair>{
					{ "TargetDistance", new ComparisonValuePair{ Value = 10.0f, Operator = ComparisonOperator.LessThanOrEquals } }
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
					if ( PatrolRoute != null ) {
						return;
					}
					if ( PhysicsPosition.DistanceTo( Blackboard.GetGotoPosition() ) <= 10.0f ) {
						Blackboard.SetTargetReached( true );
					}
				},
				name: "NavPositionReached"
			),
			new Sensor(
				runCallback: ( Agent agent ) => {
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

		if ( PatrolRoute != null ) {
			SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
		}
		
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
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );
		
		PhysicsPosition = GlobalPosition;
		if ( !Blackboard.GetTargetReached() ) {
			MoveAlongPath();
		}

		Agent.State[ "HasTarget" ] = Blackboard.GetHasTarget();
		Agent.State[ "TargetDistance" ] = Blackboard.GetTargetDistance();
		Agent.State[ "TargetReached" ] = Blackboard.GetTargetReached();
		Agent.State[ "Fear" ] = Blackboard.GetFear();
		Agent.State[ "CanSeeTarget" ] = Blackboard.GetCanSeeTarget();
		Agent.State[ "Stims" ] = Blackboard.GetStims();
		Agent.State[ "SightPosition" ] = Blackboard.GetSightPosition();
		Agent.State[ "Awareness" ] = Blackboard.GetAwareness();
		Agent.State[ "Investigating" ] = Blackboard.GetInvestigating();
		Agent.State[ "InCover" ] = Blackboard.GetInCover();
		Agent.State[ "LastTargetPosition" ] = Blackboard.GetLastTargetPosition();

		Agent.Step();
	}

	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 20 ) != 0 ) {
			return;
		}
		
		if ( (bool)GetNode( "/root/GameConfiguration" ).Get( "_demon_eye" ) ) {
			HeadAnimations.Modulate = DemonEyeColor;
			ArmAnimations.Modulate = DemonEyeColor;
			BodyAnimations.Modulate = DemonEyeColor;
		} else {
			HeadAnimations.Modulate = DefaultColor;
		}

		ProcessAnimations();
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
	private ExecutionStatus Action_CheckForTargets( Agent agent, MountainGoap.Action action ) {
		if ( Blackboard.GetTarget() != null || Blackboard.GetAwareness() > Awareness.Relaxed ) {
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
	private ExecutionStatus Action_ShootAtTarget( Agent agent, MountainGoap.Action action ) {
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
	private ExecutionStatus Action_InvestigateNode( Agent agent, MountainGoap.Action action ) {
		if ( !Blackboard.GetCanSeeTarget() ) {
			if ( ChangeInvestigateAngleTimer.IsStopped() ) {
				if ( Blackboard.GetFear() > 80.0f ) {
					// if we're scared, more JIGGLE
					ChangeInvestigateAngleTimer.WaitTime = 1.0f;
				}
				// occasionally jiggle the look angle
				ChangeInvestigateAngleTimer.Start();
			}
			
			if ( LoseInterestTimer.IsStopped() ) {
				// we can't see them, so begin the interest timer
				LoseInterestTimer.Start();
			} else if ( LoseInterestTimer.TimeLeft == 0.0f ) {
				return ExecutionStatus.Failed;
			}
		}
		else {
			if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_difficulty" ) == 0 ) { 
				// FIXME: is this too harsh?
				Bark( BarkType.TargetSpotted );
				
				Blackboard.SetAwareness( Awareness.Alert );
				Blackboard.SetTarget( SightTarget );
				
				agent.PlanAsync();
				
				return ExecutionStatus.Succeeded;
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

		// make them investigate
		SetNavigationTarget( Blackboard.GetLastTargetPosition() );
		if ( PhysicsPosition.DistanceTo( Blackboard.GetLastTargetPosition() ) <= 10.0f ) {
			Velocity = Godot.Vector2.Zero;
			if ( ChangeInvestigateAngleTimer.TimeLeft == ChangeInvestigateAngleTimer.WaitTime ) {
				if ( Blackboard.GetFear() > 80.0f ) {
					// if we're scared, more JIGGLE
					ChangeInvestigateAngleTimer.WaitTime = 1.0f;
				}
				// occasionally jiggle the look angle
				ChangeInvestigateAngleTimer.Start();
			}
			if ( LoseInterestTimer.IsStopped() ) {
				LoseInterestTimer.Start();
			}
		}

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