using System;
using Godot;
using Renown.World;

public partial class MercenaryShotgunner : MobBase {
	private Timer TargetMovedTimer;
	private Timer HealTimer;
	private Timer AimTimer;
	private RayCast2D AimLine;
	private Line2D ShootLine;
	private Godot.Vector2 GuardPosition;

	private PointLight2D FlashLight;

	private object LockObject = new object();

	public override void Save() {
		base.Save();
	}
	public override void Load() {
		base.Load();
	}

	protected override void OnLoseInterestTimerTimeout() {
		if ( GameConfiguration.GameDifficulty == GameDifficulty.PowerFantasy ) {
			// "whatevs"
			Awareness = AIAwareness.Relaxed;
		}

		Investigating = false;
		SetNavigationTarget( GuardPosition );

		// a little more on edge
		Fear += 10;
	}
	
	private void OnTargetMoveTimerTimeout() {
		// "target's pinned!"
		Bark( BarkType.TargetPinned );
	}

	protected override void SendPacket() {
		SyncObject.Write( PhysicsPosition.X );
		SyncObject.Write( PhysicsPosition.Y );
		SyncObject.Write( Health );
		SyncObject.Write( Fear );
		SyncObject.Write( (uint)State );
		SyncObject.Sync();
	}
	protected override void ReceivePacket( System.IO.BinaryReader reader ) {
		Godot.Vector2 position = Godot.Vector2.Zero;
		position.X = (float)reader.ReadDouble();
		position.Y = (float)reader.ReadDouble();
		GlobalPosition = position;
	}

	public override void _ExitTree() {
		base._ExitTree();

		DetectionMeter.QueueFree();
		TargetMovedTimer.QueueFree();

		if ( PatrolRoute != null ) {
			PatrolRoute.QueueFree();
		}
		ChangeInvestigateAngleTimer.QueueFree();

		for ( int i = 0; i < SightLines.Length; i++ ) {
			SightLines[i].QueueFree();
		}

		LoseInterestTimer.QueueFree();

		HeadAnimations.QueueFree();
		ArmAnimations.QueueFree();
		BodyAnimations.QueueFree();

		QueueFree();
	}
    public override void _Ready() {
		base._Ready();

		FlashLight = GetNode<PointLight2D>( "Animations/HeadAnimations/FlashLight" );
		WorldTimeManager.Instance.TimeTick += ( day, hour, minute ) => {
			if ( hour >= 20 || hour < 7 ) {
				// turn on flashlights when it gets dark
				FlashLight.Show();
			}
			else if ( hour >= 7 ) {
				FlashLight.Hide();
			}
		};

		if ( SettingsData.GetNetworkingEnabled() ) {
			SyncObject = new NetworkWriter( 256 );
		}

		TargetMovedTimer = new Timer();
		TargetMovedTimer.WaitTime = 10.0f;
		TargetMovedTimer.OneShot = true;
		TargetMovedTimer.Connect( "timeout", Callable.From( OnTargetMoveTimerTimeout ) );
		AddChild( TargetMovedTimer );

		if ( GameConfiguration.GameDifficulty == GameDifficulty.Intended ) {
			DetectionMeter.Hide();
		}

		GuardPosition = GlobalPosition;
		DemonEyeColor = new Color();
	}
	protected override void Think( float delta ) {
		CheckSight( delta );

		if ( State == AIState.Investigating ) {
			Investigate();
		}
		
		// if we've got any suspicion, then start patrolling
		if ( Fear > 0 && PatrolRoute == null ) {
			PatrolRoute = NodeCache.FindClosestRoute( GlobalPosition );
			State = AIState.PatrolStart;
			SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
		}
		if ( Fear > 80 ) {
		}

		if ( SightTarget == null ) {
			switch ( Direction ) {
			case DirType.North:
				LookDir = Godot.Vector2.Up;
				break;
			case DirType.East:
				LookDir = Godot.Vector2.Right;
				break;
			case DirType.South:
				LookDir = Godot.Vector2.Down;
				break;
			case DirType.West:
				LookDir = Godot.Vector2.Left;
				break;
			default:
				GD.PushError( "Invalid direction!" );
				break;
			};
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;
		}
	}

	private void Investigate() {
		if ( GlobalPosition != LastTargetPosition ) {
			return; // not there yet
		}
		if ( ChangeInvestigateAngleTimer.IsStopped() ) {
			if ( Fear >= 60.0f ) {
				ChangeInvestigateAngleTimer.WaitTime = 0.6f;
			}
			ChangeInvestigateAngleTimer.Start();
		}
		if ( LoseInterestTimer.IsStopped() ) {
			LoseInterestTimer.Start();
		}
	}

	private void SetAlert( bool bRunning ) {
		Bark( BarkType.Alert );
		Awareness = AIAwareness.Alert;
	}
	private void SetSuspicious() {
		Bark( BarkType.Confusion );
		Awareness = AIAwareness.Suspicious;
	}
	private void CheckSight( float delta ) {
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

		if ( SightTarget != null && Awareness == AIAwareness.Alert ) {
//			return; // hunt them down
		}

		// we saw something, but it slipped out of view
		if ( SightDetectionAmount > 0.0f && sightTarget == null ) {
			CreateAfterImage();
			switch ( Awareness ) {
			case AIAwareness.Relaxed:
				// "must be nothing"
				SightDetectionAmount -= SightDetectionSpeed * delta;
				if ( SightDetectionAmount <= 0.0f ) {
					SightDetectionAmount = 0.0f;
				}
				break;
			case AIAwareness.Suspicious:
				SetSuspicious();
				break;
			case AIAwareness.Alert:
				SetAlert( true );
				break;
			};
			SetDetectionColor();
			CanSeeTarget = false;
			return;
		}

		AfterImageUpdated = false;

		SightTarget = (Renown.Entity)sightTarget;
		if ( sightTarget is Player || sightTarget is NetworkPlayer ) {
			CanSeeTarget = true;
			LastTargetPosition = SightTarget.GlobalPosition;
			if ( Awareness >= AIAwareness.Suspicious ) {
				SightDetectionAmount += ( SightDetectionSpeed * 2.0f );
				Investigating = true;
				SetNavigationTarget( LastTargetPosition );
			} else if ( Awareness == AIAwareness.Relaxed ) {
				SightDetectionAmount += SightDetectionSpeed;
			}
		}
		if ( SightDetectionAmount < SightDetectionTime * 0.5f ) {
			Awareness = AIAwareness.Relaxed;
		} else if ( SightDetectionAmount < SightDetectionTime ) {
			Awareness = AIAwareness.Suspicious;
		}
		if ( IsAlert() ) {
			SetAlert( false );
		} else if ( IsSuspicious() ) {
			SetSuspicious();
		}
		SetDetectionColor();
	}
};