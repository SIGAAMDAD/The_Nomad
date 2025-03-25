using System.Collections.Generic;
using System.Collections.Concurrent;
using Godot;
using MountainGoap;
using System;
using Steamworks;

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

public enum AIAwareness : int {
	Invalid = -1,		// invalid
	Relaxed,
	Suspicious,
	Alert,
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

	protected Node2D BloodSplatterTree;
	protected PackedScene BloodSplatter;

	protected Node2D BulletShellTree;
	protected PackedScene BulletShell;

	protected Color DemonEyeColor;
	protected static readonly Color DefaultColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
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

	// memory
	protected AIAwareness Awareness = AIAwareness.Relaxed;
	protected Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;
	protected Godot.Vector2 GotoPosition = Godot.Vector2.Zero;
	protected Godot.Vector2 SightPosition = Godot.Vector2.Zero;
	protected float Fear = 0.0f;
	protected bool Investigating = false;
	protected CharacterBody2D Target = null;
	protected bool CanSeeTarget = false;
	protected bool TargetReached = false;

	protected AudioStreamPlayer2D MoveChannel;
	protected AudioStreamPlayer2D BarkChannel;
	protected AudioStreamPlayer2D AudioChannel;
	protected BarkType LastBark;
	protected BarkType SequencedBark;
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
	protected RayCast2D[] SightLines;
	protected PlayerSystem.AfterImage AfterImage;
	protected Timer MoveTimer;

	protected Squad Squad;

	public float GetHealth() {
		return Health;
	}
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
		return target is Player || target is MobBase;
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

		GD.Print( "Changing investigation angle" );

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
	protected void CreateAfterImage() {
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

	protected void Init() {
		RandomFactory = new System.Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day + System.DateTime.Now.Second + System.DateTime.Now.Millisecond );

		ViewAngleAmount = Mathf.DegToRad( ViewAngleAmount );

		SightDetector = GetNode<Node2D>( "Animations/HeadAnimations/SightCheck" );
		SightDetector.SetProcess( false );
		SightDetector.SetProcessInternal( false );

		DetectionMeter = GetNode<Line2D>( "DetectionMeter" );
		DetectionMeter.SetProcess( false );
		DetectionMeter.SetProcessInternal( false );

		Navigation = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
		Navigation.Connect( "target_reached", Callable.From( OnTargetReached ) );
		
		HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
		ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );
		BodyAnimations = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations" );

		MoveTimer = new Timer();
		MoveTimer.OneShot = true;
		MoveTimer.WaitTime = 0.40f;
		MoveTimer.Connect( "timeout", Callable.From( OnMoveTimerTimeout ) );
		MoveTimer.SetProcess( false );
		MoveTimer.SetProcessInternal( false );
		AddChild( MoveTimer );

		MoveChannel = GetNode<AudioStreamPlayer2D>( "MoveChannel" );
		MoveChannel.SetProcess( false );
		MoveChannel.SetProcessInternal( false );

		BarkChannel = GetNode<AudioStreamPlayer2D>( "BarkChannel" );
		BarkChannel.SetProcess( false );
		BarkChannel.SetProcessInternal( false );
		BarkChannel.Connect( "finished", Callable.From( OnBarkFinished ) );
		
		BulletShellTree = new Node2D();
		GetTree().CurrentScene.AddChild( BulletShellTree );
		
		BloodSplatterTree = new Node2D();
		GetTree().CurrentScene.AddChild( BloodSplatterTree );
		
		DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		AfterImage = new PlayerSystem.AfterImage();
		AfterImage.SetProcess( false );
		AfterImage.SetProcessInternal( false );
		GetTree().Root.AddChild( AfterImage );
		
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
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );
		
		PhysicsPosition = GlobalPosition;
		if ( MoveTimer.IsStopped() && Velocity != Godot.Vector2.Zero ) {
			MoveTimer.Start();
		}
		MoveAlongPath();
	}

	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 24 ) != 0 ) {
			return;
		}

		base._Process( delta );

		if ( GameConfiguration.DemonEyeActive ) {
			HeadAnimations.SetDeferred( "modulate", DemonEyeColor );
			ArmAnimations.SetDeferred( "modulate", DemonEyeColor );
			BodyAnimations.SetDeferred( "modulate", DemonEyeColor );
		} else {
			HeadAnimations.SetDeferred( "modulate", DefaultColor );
		}

		ProcessAnimations();

		/*
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
		Agent.State[ "LoseInterest" ] = Blackboard.GetLostInterest();

		Agent.Step();
		*/
		Think( (float)delta );
	}
	protected virtual void Think( float delta ) {
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

	protected bool MoveAlongPath() {
		if ( GlobalPosition.DistanceTo( GotoPosition ) <= 10.0f ) {
			Velocity = Godot.Vector2.Zero;
			return true;
		}
		Godot.Vector2 nextPathPosition = Navigation.GetNextPathPosition();
		AngleDir = GlobalPosition.DirectionTo( nextPathPosition );
		LookAngle = Mathf.Atan2( AngleDir.Y, AngleDir.X );
		Velocity = AngleDir * MovementSpeed;
		return MoveAndSlide();
	}
	protected void SetNavigationTarget( Godot.Vector2 target ) {
		Navigation.TargetPosition = target;
		TargetReached = false;
		GotoPosition = target;
	}
	protected void OnTargetReached() {
		TargetReached = true;
		GotoPosition = GlobalPosition;
		Velocity = Godot.Vector2.Zero;
	}
};