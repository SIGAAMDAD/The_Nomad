using System.Collections.Generic;
using System.Collections.Concurrent;
using Godot;
using MountainGoap;
using System.ComponentModel;

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

	Count
};

public partial class MobBase : CharacterBody2D {
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
	public enum Awareness : int {
		Invalid = -1,		// invalid
		Relaxed,
		Suspicious,
		Alert,
		Count
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
	protected Godot.Vector2 PhysicsPosition;
	protected Godot.Vector2 AngleDir = Godot.Vector2.Zero;
	protected Godot.Vector2 NextPathPosition = Godot.Vector2.Zero;
	protected Godot.Vector2 LookDir = Godot.Vector2.Zero;
	protected float LookAngle = 0.0f;
	protected float AimAngle = 0.0f;
	protected PointLight2D FlashLight;

	protected AudioStreamPlayer2D MoveChannel;
	protected AudioStreamPlayer2D BarkChannel;
	protected BarkType LastBark;
	protected AudioStream SequencedBark;
	protected Timer ThinkerTimer;
	protected Timer LoseInterestTimer;
	protected Timer ChangeInvestigateAngleTimer;
	protected Node2D SightDetector;
	protected Line2D DetectionMeter;
	protected AnimatedSprite2D HeadAnimations;
	protected AnimatedSprite2D BodyAnimations;
	protected AnimatedSprite2D ArmAnimations;
	protected NavigationAgent2D Navigation;
	protected Color DetectionColor;
	protected AIBlackboard Blackboard;

	protected List<RayCast2D> SightLines;

	protected Agent Agent;
	protected Squad Squad;

	public Agent GetAgent() {
		return Agent;
	}
	public float GetHealth() {
		return Health;
	}
	public void SetBlackboard( string key, object value ) {
		Agent.Memory[ key ] = value;
	}
	public object GetBlackboard( string key ) {
		return Agent.Memory[ key ];
	}

	protected void Bark( BarkType bark ) {
		if ( LastBark == bark ) {
			return;
		}
		LastBark = bark;
		switch ( bark ) {
		case BarkType.ManDown:
			BarkChannel.Stream = AudioCache.ManDown[ RandomFactory.Next( 0, AudioCache.ManDown.Length - 1 ) ];
			break;
		case BarkType.MenDown2:
			BarkChannel.Stream = AudioCache.ManDown2;
			break;
		case BarkType.MenDown3:
			BarkChannel.Stream = AudioCache.ManDown3;
			break;
		case BarkType.TargetSpotted:
			BarkChannel.Stream = AudioCache.TargetSpotted[ RandomFactory.Next( 0, AudioCache.TargetSpotted.Length - 1 ) ];
			break;
		case BarkType.TargetPinned:
			BarkChannel.Stream = AudioCache.TargetPinned[ RandomFactory.Next( 0, AudioCache.TargetPinned.Length - 1 ) ];
			break;
		case BarkType.TargetRunning:
			BarkChannel.Stream = AudioCache.TargetRunning[ RandomFactory.Next( 0, AudioCache.TargetRunning.Length - 1 ) ];
			break;
		case BarkType.Confusion:
			BarkChannel.Stream = AudioCache.Confusion[ RandomFactory.Next( 0, AudioCache.Confusion.Length - 1 ) ];
			break;
		case BarkType.Alert:
			BarkChannel.Stream = AudioCache.Alert[ RandomFactory.Next( 0, AudioCache.Alert.Length - 1 ) ];
			break;
		case BarkType.OutOfTheWay:
			BarkChannel.Stream = AudioCache.OutOfTheWay[ RandomFactory.Next( 0, AudioCache.OutOfTheWay.Length - 1 ) ];
			break;
		case BarkType.NeedBackup:
			BarkChannel.Stream = AudioCache.NeedBackup[ RandomFactory.Next( 0, AudioCache.NeedBackup.Length - 1 ) ];
			break;
		case BarkType.SquadWiped:
			BarkChannel.Stream = AudioCache.SquadWiped;
			break;
		case BarkType.Curse:
			BarkChannel.Stream = AudioCache.Curse[ RandomFactory.Next( 0, AudioCache.Curse.Length - 1 ) ];
			break;
		};
		BarkChannel.Play();
	}
	protected void GenerateRaycasts() {
		int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
		SightLines = new List<RayCast2D>();

		for ( int i = 0; i < rayCount; i++ ) {
			RayCast2D ray = new RayCast2D();
			float angle = AngleBetweenRays * ( i - rayCount / 2.0f );
			ray.TargetPosition = LookDir.Rotated( angle ) * MaxViewDistance;
			ray.Enabled = true;
			ray.CollisionMask = 1 | 2;
			SightDetector.AddChild( ray );
			SightLines.Add( ray );
		}
	}
	protected void RecalcSight() {
		Agent.State[ "SightPosition" ] = GlobalPosition;
		for ( int i = 0; i < SightLines.Count; i++ ) {
			RayCast2D ray = SightLines[i];
			float angle = AngleBetweenRays * ( i - SightLines.Count / 2.0f );
			ray.TargetPosition = LookDir.Rotated( angle ) * MaxViewDistance;
		}
	}

#region Utility
	protected bool IsDeadAI( CharacterBody2D bot ) {
		return (float)bot.Get( "health" ) <= 0.0f;
	}
	protected bool IsValidTarget( GodotObject target ) {
		return target is Player || target is MobBase;
	}
	protected bool IsAlert() {
		return Blackboard.GetAwareness() == Awareness.Alert || SightDetectionAmount >= SightDetectionTime;
	}
	protected bool IsSuspicious() {
		return ( !IsAlert() && Blackboard.GetAwareness() == Awareness.Suspicious ) || SightDetectionAmount >= SightDetectionTime * 0.5f;
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
	}
	
	protected void OnSequencedBarkFinished() {
		SequencedBark = null;
	}
	protected void OnBarkFinished() {
		BarkChannel.Stream = null;
		if ( SequencedBark != null ) {
			// play another bark right after the first
			BarkChannel.Stream = SequencedBark;
			BarkChannel.Play();
		}
	}
	protected void OnBodyAnimationFinished() {
		if ( BodyAnimations.Animation == "move" ) {
			MoveChannel.Stream = AudioCache.MoveGravelSfx[ RandomFactory.Next( 0, AudioCache.MoveGravelSfx.Length - 1 ) ];
			MoveChannel.Play();
		}
	}

	protected void Init() {
		RandomFactory = new System.Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day );

		ViewAngleAmount = Mathf.DegToRad( ViewAngleAmount );

		SightDetector = GetNode<Node2D>( "Animations/HeadAnimations/SightCheck" );
		DetectionMeter = GetNode<Line2D>( "DetectionMeter" );

		Navigation = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
		Navigation.Connect( "target_reached", Callable.From( OnTargetReached ) );
		
		HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
		ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );
		BodyAnimations = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations" );
		BodyAnimations.Connect( "animation_looped", Callable.From( OnBodyAnimationFinished ) );
		
		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );
		MoveChannel.SetProcess( false );
		MoveChannel.SetProcessInternal( false );

		BarkChannel = GetNode<AudioStreamPlayer2D>( "BarkChannel" );
		BarkChannel.SetProcess( false );
		BarkChannel.SetProcessInternal( false );
		
//		ActionSfx = GetNode<AudioStreamPlayer2D>( "ActionSfx" );

		/*
		Bark = new AudioStreamPlayer2D();
		Bark.VolumeDb = 10.0f;
		Bark.Bus = "SFX";
		Bark.GlobalPosition = GlobalPosition;
		Bark.Connect( "finished", Callable.From( OnBarkFinished ) );
		AddChild( Bark );
		
		SequencedBark = new AudioStreamPlayer2D();
		SequencedBark.VolumeDb = 10.0f;
		SequencedBark.GlobalPosition = GlobalPosition;
		SequencedBark.Stream = null;
		SequencedBark.Connect( "finished", Callable.From( OnSequencedBarkFinished ) );
		AddChild( SequencedBark );
		*/
		
		BulletShellTree = new Node2D();
		GetTree().CurrentScene.AddChild( BulletShellTree );
		
		BloodSplatterTree = new Node2D();
		GetTree().CurrentScene.AddChild( BloodSplatterTree );
		
		DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		
		ChangeInvestigateAngleTimer = new Timer();
		ChangeInvestigateAngleTimer.WaitTime = 1.5f;
		ChangeInvestigateAngleTimer.OneShot = true;
		ChangeInvestigateAngleTimer.Connect( "timeout", Callable.From( OnChangeInvestigateAngleTimerTimeout ) );
		AddChild( ChangeInvestigateAngleTimer );
		
		LoseInterestTimer = new Timer();
		LoseInterestTimer.WaitTime = LoseInterestTime;
		LoseInterestTimer.OneShot = true;
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
	}

	protected void ProcessAnimations()  {
		ArmAnimations.SetDeferred( "global_rotation", AimAngle );
		HeadAnimations.SetDeferred( "global_rotation", LookAngle );
		
		if ( LookAngle > 0.0f ) {
			HeadAnimations.SetDeferred( "flip_v", true );
		} else if ( LookAngle < 0.0f ) {
			HeadAnimations.SetDeferred( "flip_v", false );
		}
		if ( AimAngle > 0.0f ) {
			ArmAnimations.SetDeferred( "flip_v", true );
		} else if ( AimAngle < 0.0f ) {
			ArmAnimations.SetDeferred( "flip_v", false );
		}
		if ( Velocity.X > 0.0f ) {
			BodyAnimations.SetDeferred( "flip_h", false );
			ArmAnimations.SetDeferred( "flip_h", false );
		} else if ( Velocity.X < 0.0f ) {
			BodyAnimations.SetDeferred( "flip_h", true );
			ArmAnimations.SetDeferred( "flip_h", true );
		}

		if ( Velocity != Godot.Vector2.Zero ) {
			BodyAnimations.CallDeferred( "play", "move" );
			ArmAnimations.CallDeferred( "play", "move" );
			HeadAnimations.CallDeferred( "play", "move" );
		} else {
			BodyAnimations.CallDeferred( "play", "idle" );
			ArmAnimations.CallDeferred( "play", "idle" );
			HeadAnimations.CallDeferred( "play", "idle" );
		}

//		FlashLight.GlobalRotation = LookAngle;
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );
		PhysicsPosition = GlobalPosition;
	}

    protected bool MoveAlongPath() {
		Godot.Vector2 nextPathPosition = Navigation.GetNextPathPosition();
		AngleDir = GlobalPosition.DirectionTo( nextPathPosition );
		LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
		Velocity = LookDir * MovementSpeed;
		return MoveAndSlide();
	}
	protected void SetNavigationTarget( Godot.Vector2 target ) {
		Navigation.TargetPosition = target;
		Blackboard.SetTargetReached( false );
		Blackboard.SetTargetDistance( PhysicsPosition.DistanceTo( target ) );
	}
	protected void OnTargetReached() {
		Blackboard.SetTargetReached( true );
		Blackboard.SetTargetDistance( 0.0f );
	}

#region Actions
	protected ExecutionStatus Action_RunAway( Agent agent, MountainGoap.Action action ) {
		if ( Health <= 0.0f ) {
			return ExecutionStatus.Failed;
		} else if ( (float)Blackboard.GetTargetDistance() > 1500.0f ) {
			return ExecutionStatus.Succeeded;
		}
		return ExecutionStatus.Executing;
	}
#endregion
};