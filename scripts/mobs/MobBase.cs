using System.Runtime.CompilerServices;
using Godot;

public enum BarkType : uint {
	TargetSpotted,
	TargetPinned,
	TargetRunning,
	Stuck,
	ManDown,
	MenDown2,
	MenDown3,
	Confusion,
	Alert,
	OutOfTheWay,
	NeedBackup,
	SquadWiped,
	Curse,
	Quiet,
	CheckItOut,
	Unstoppable,

	Count
};

public enum AIAwareness : sbyte {
	Invalid = -1,
	Relaxed,
	Suspicious,
	Alert,

	Count
};

public enum AIState : sbyte {
	Invalid = -1,
	Guarding, // guarding a node

	// moving along a patrol link chain
	PatrolStart,
	Patrolling,

	Scared, // Fear > 80
	Investigating, // investigating a node

	Dead, // U R DED, NO BIG SOOPRIZE
	
	Count
};

public partial class MobBase : Renown.Thinker {
	public enum DirType {
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest
	};

	protected Node2D BloodSplatterTree;
	protected PackedScene BloodSplatter;

	protected Node2D BulletShellTree;
	protected PackedScene BulletShell;

	protected float AngleBetweenRays = Mathf.DegToRad( 2.0f );
	
	[ExportCategory("Sounds")]
	[Export]
	protected AudioStream AttackFirearmSfx;
	[Export]
	protected AudioStream AttackMeleeSfx;

	[ExportCategory("Detection")]
	[Export]
	protected float SoundDetectionLevel;
	[Export]
	protected float ViewAngleAmount = 45.0f;
	[Export]
	protected float MaxViewDistance;
	[Export]
	protected float SightDetectionSpeed = 0.1f;
	[Export]
	protected float SightDetectionTime = 1.0f;
	[Export]
	protected float LoseInterestTime = 10.5f;

	[ExportCategory("Start")]
	[Export]
	protected DirType Direction;
	[Export]
	protected AINodeCache NodeCache;
	[Export]
	protected NavigationLink2D PatrolRoute;

	[ExportCategory("Stats")]
	[Export]
	protected float Health = 0.0f;
	[Export]
	protected float MovementSpeed = 1.0f;
	[Export]
	protected float FirearmDamage = 0.0f;
	[Export]
	protected float BluntDamage = 0.0f;
	[Export]
	protected float BladedDamage = 0.0f;

	protected System.Random RandomFactory;

	protected CharacterBody2D SightTarget;
	protected float SightDetectionAmount = 0.0f;
	protected Godot.Vector2 AngleDir = Godot.Vector2.Zero;
	protected Godot.Vector2 NextPathPosition = Godot.Vector2.Zero;
	protected Godot.Vector2 LookDir = Godot.Vector2.Zero;
	protected PointLight2D FlashLight;

	// memory
	protected AIAwareness Awareness = AIAwareness.Relaxed;
	protected Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;
	protected Godot.Vector2 SightPosition = Godot.Vector2.Zero;
	protected byte Fear = 0;
	protected bool Investigating = false;
	protected CharacterBody2D Target = null;
	protected bool CanSeeTarget = false;
	protected bool AfterImageUpdated = false;

	protected AIState State = AIState.Guarding;

	protected AudioStreamPlayer2D BarkChannel;
	protected AudioStreamPlayer2D AudioChannel;
	protected BarkType LastBark;
	protected BarkType SequencedBark;
	protected Timer ThinkerTimer;
	protected Timer LoseInterestTimer;
	protected Timer ChangeInvestigateAngleTimer;
	protected Node2D SightDetector;
	protected Line2D DetectionMeter;
	protected Color DetectionColor;
	protected RayCast2D[] SightLines;
	protected PlayerSystem.AfterImage AfterImage;

	protected Squad Squad;

	public float GetHealth() => Health;
	private AudioStream GetBarkResource( BarkType bark ) {
		switch ( bark ) {
		case BarkType.ManDown:
			return AudioCache.ManDown[ RandomFactory.Next( 0, AudioCache.ManDown.Length - 1 ) ];
		case BarkType.MenDown2:
			return AudioCache.ManDown2;
		case BarkType.MenDown3:
			return AudioCache.ManDown3;
		case BarkType.TargetSpotted:
			return AudioCache.TargetSpotted[ RandomFactory.Next( 0, AudioCache.TargetSpotted.Length - 1 ) ];
		case BarkType.TargetPinned:
			return AudioCache.TargetPinned[ RandomFactory.Next( 0, AudioCache.TargetPinned.Length - 1 ) ];
		case BarkType.TargetRunning:
			return AudioCache.TargetRunning[ RandomFactory.Next( 0, AudioCache.TargetRunning.Length - 1 ) ];
		case BarkType.Confusion:
			return AudioCache.Confusion[ RandomFactory.Next( 0, AudioCache.Confusion.Length - 1 ) ];
		case BarkType.Alert:
			return AudioCache.Alert[ RandomFactory.Next( 0, AudioCache.Alert.Length - 1 ) ];
		case BarkType.OutOfTheWay:
			return AudioCache.OutOfTheWay[ RandomFactory.Next( 0, AudioCache.OutOfTheWay.Length - 1 ) ];
		case BarkType.NeedBackup:
			return AudioCache.NeedBackup[ RandomFactory.Next( 0, AudioCache.NeedBackup.Length - 1 ) ];
		case BarkType.SquadWiped:
			return AudioCache.SquadWiped;
		case BarkType.Curse:
			return AudioCache.Curse[ RandomFactory.Next( 0, AudioCache.Curse.Length - 1 ) ];
		case BarkType.CheckItOut:
			return AudioCache.CheckItOut[ RandomFactory.Next( 0, AudioCache.CheckItOut.Length - 1 ) ];
		case BarkType.Quiet:
			return AudioCache.Quiet[ RandomFactory.Next( 0, AudioCache.Quiet.Length - 1 ) ];
		case BarkType.Unstoppable:
			return AudioCache.Unstoppable;
		case BarkType.Count:
		default:
			break;
		};
		return null;
	}
	protected void Bark( BarkType bark, BarkType sequenced = BarkType.Count ) {
		if ( LastBark == bark ) {
			return;
		}
		LastBark = bark;
		SequencedBark = sequenced;

		BarkChannel.Stream = GetBarkResource( bark );
		BarkChannel.Play();
	}
	protected void GenerateRaycasts() {
		int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
		SightLines = new RayCast2D[ rayCount ];

		for ( int i = 0; i < rayCount; i++ ) {
			RayCast2D ray = new RayCast2D();
			float angle = AngleBetweenRays * ( i - rayCount / 2.0f );
			ray.TargetPosition = LookDir.Rotated( angle ) * MaxViewDistance;
			ray.Enabled = true;
			ray.CollisionMask = 2;
			SightDetector.AddChild( ray );
			SightLines[i] = ray;
		}
	}
	protected void RecalcSight() {
		SightPosition = GlobalPosition;
		for ( int i = 0; i < SightLines.Length; i++ ) {
			RayCast2D ray = SightLines[i];
			float angle = AngleBetweenRays * ( i - SightLines.Length / 2.0f );
			ray.SetDeferred( "target_position", LookDir.Rotated( angle ) * MaxViewDistance );
		}
	}

#region Utility
	protected bool IsDeadAI( CharacterBody2D bot ) {
		return (float)bot.Get( "health" ) <= 0.0f;
	}
	protected bool IsValidTarget( GodotObject target ) {
		return target is Player || target is NetworkPlayer || target is MobBase;
	}
	protected bool IsAlert() {
		return Awareness == AIAwareness.Alert || SightDetectionAmount >= SightDetectionTime;
	}
	protected bool IsSuspicious() {
		return ( !IsAlert() && Awareness == AIAwareness.Suspicious ) || SightDetectionAmount >= SightDetectionTime * 0.5f;
	}
	protected float Randf( float min, float max ) {
		return (float)( min + RandomFactory.NextDouble() * ( min - max ) );
	}
#endregion

	protected virtual void OnLoseInterestTimerTimeout() {
	}
	protected virtual void OnChangeInvestigateAngleTimerTimeout() {
		float angle = Randf( 0.0f, 360.0f );
		AimAngle = angle;
		LookAngle = angle;

		ArmAnimations.SetDeferred( "global_rotation", AimAngle );
		HeadAnimations.SetDeferred( "global_rotation", LookAngle );

		RecalcSight();
	}
	
	protected void OnBarkFinished() {
		BarkChannel.Stream = null;
		if ( SequencedBark != BarkType.Count ) {
			// play another bark right after the first
			GD.Print( "Sequencing bark " + SequencedBark.ToString() );

			BarkChannel.Stream = GetBarkResource( SequencedBark );
			BarkChannel.Play();
			SequencedBark = BarkType.Count;
		}
	}
	protected void OnMoveTimerTimeout() {
		if ( Velocity != Godot.Vector2.Zero ) {
			MoveTimer.Start();
		} else if ( !AudioCache.Initialized ) {
			return;
		}
		MoveChannel.Stream = AudioCache.MoveGravelSfx[ RandomFactory.Next( 0, AudioCache.MoveGravelSfx.Length - 1 ) ];
		MoveChannel.Play();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void CreateAfterImage() {
		if ( AfterImageUpdated ) {
			return;
		}
		AfterImageUpdated = true;
		AfterImage.CallDeferred( "Update", (Player)SightTarget );
	}

	protected void SetDetectionColor() {
		switch ( Awareness ) {
		case AIAwareness.Relaxed:
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
		case AIAwareness.Suspicious:
			DetectionColor.R = 0.0f;
			DetectionColor.G = 0.0f;
			DetectionColor.B = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
			break;
		case AIAwareness.Alert:
			DetectionColor.R = Mathf.Lerp( 0.05f, 1.0f, SightDetectionAmount );
			DetectionColor.G = 0.0f;
			DetectionColor.B = 0.0f;
			break;
		default:
			break;
		};

		DetectionMeter.SetDeferred( "default_color", DetectionColor );
	}

	public override void _Ready() {
		base._Ready();

		RandomFactory = new System.Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day + System.DateTime.Now.Second + System.DateTime.Now.Millisecond );

		ViewAngleAmount = Mathf.DegToRad( ViewAngleAmount );

		SightDetector = GetNode<Node2D>( "Animations/HeadAnimations/SightCheck" );
		SightDetector.SetProcess( false );
		SightDetector.SetProcessInternal( false );

		DetectionMeter = GetNode<Line2D>( "DetectionMeter" );
		DetectionMeter.SetProcess( false );
		DetectionMeter.SetProcessInternal( false );
		
		HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
		ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );
		BodyAnimations = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations" );

		MoveTimer = new Timer();
		MoveTimer.OneShot = true;
		MoveTimer.WaitTime = 0.40f;
		MoveTimer.Connect( "timeout", Callable.From( OnMoveTimerTimeout ) );
		MoveTimer.SetProcess( false );
		MoveTimer.SetProcessInternal( false );
		CallDeferred( "add_child", MoveTimer );

		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );
		MoveChannel.SetProcess( false );
		MoveChannel.SetProcessInternal( false );

		BarkChannel = GetNode<AudioStreamPlayer2D>( "BarkChannel" );
		BarkChannel.SetProcess( false );
		BarkChannel.SetProcessInternal( false );
		BarkChannel.Connect( "finished", Callable.From( OnBarkFinished ) );
		
		BulletShellTree = new Node2D();
		GetTree().CurrentScene.CallDeferred( "add_child", BulletShellTree );
		
		BloodSplatterTree = new Node2D();
		GetTree().CurrentScene.CallDeferred( "add_child", BloodSplatterTree );
		
		DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		AfterImage = new PlayerSystem.AfterImage();
		AfterImage.SetProcess( false );
		AfterImage.SetProcessInternal( false );
		GetTree().CurrentScene.CallDeferred( "add_child", AfterImage );
		
		ChangeInvestigateAngleTimer = new Timer();
		ChangeInvestigateAngleTimer.WaitTime = 1.5f;
		ChangeInvestigateAngleTimer.OneShot = false;
		ChangeInvestigateAngleTimer.SetProcess( false );
		ChangeInvestigateAngleTimer.SetProcessInternal( false );
		ChangeInvestigateAngleTimer.Connect( "timeout", Callable.From( OnChangeInvestigateAngleTimerTimeout ) );
		AddChild( ChangeInvestigateAngleTimer );
		
		LoseInterestTimer = new Timer();
		LoseInterestTimer.WaitTime = LoseInterestTime;
		LoseInterestTimer.OneShot = true;
		LoseInterestTimer.SetProcess( false );
		LoseInterestTimer.SetProcessInternal( false );
		LoseInterestTimer.Connect( "timeout", Callable.From( OnLoseInterestTimerTimeout ) );
		AddChild( LoseInterestTimer );

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
		};
		LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
		AimAngle = LookAngle;

		GenerateRaycasts();

		if ( PatrolRoute != null ) {
			SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
			State = AIState.PatrolStart;
		}
	}

	protected override void OnTargetReached() {
		TargetReached = true;

		switch ( State ) {
		case AIState.PatrolStart:
			SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
			break;
		case AIState.Patrolling:
			PatrolRoute = ( (AIPatrolRoute)PatrolRoute ).GetNext();
			SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
			break;
		default:
			GotoPosition = GlobalPosition;
			Velocity = Godot.Vector2.Zero;
			break;
		};
	}
};