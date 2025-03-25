using System.Threading.Tasks;
using Godot;

public partial class MercenaryShotgunner : MobBase {
	[Export]
	private NavigationLink2D PatrolRoute;

	private Timer TargetMovedTimer;
	private Timer HealTimer;
	private Timer AimTimer;
	private RayCast2D AimLine;
	private Line2D ShootLine;
	private Godot.Vector2 GuardPosition;

	protected override void OnLoseInterestTimerTimeout() {
		Investigating = false;
		SetNavigationTarget( GuardPosition );
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

	public override void _Ready() {
		base._Ready();

		Init();

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

		if ( Investigating ) {
			Investigate();
		}
	}

	private void Investigate() {
		if ( ChangeInvestigateAngleTimer.IsStopped() ) {
			if ( Fear >= 60.0f ) {
				ChangeInvestigateAngleTimer.WaitTime = 0.6f;
			}
			ChangeInvestigateAngleTimer.Start();
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
		if ( SightPosition != GlobalPosition ) {
			RecalcSight();
		}

		GodotObject sightTarget = null;
		for ( int i = 0; i < SightLines.Length; i++ ) {
			sightTarget = SightLines[i].GetCollider();
			if ( sightTarget != null && IsValidTarget( sightTarget ) ) {
				break; 
			} else {
				sightTarget = null;
			}
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

		SightTarget = sightTarget as Player;
		if ( SightTarget != null ) {
			CanSeeTarget = true;
			LastTargetPosition = SightTarget.GlobalPosition;
			if ( Awareness >= AIAwareness.Suspicious ) {
				SightDetectionAmount += ( SightDetectionSpeed * 2.0f );
				Investigating = true;
				SetNavigationTarget( SightTarget.GlobalPosition );
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