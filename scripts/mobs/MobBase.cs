using System.Collections.Generic;
using System.Collections.Concurrent;
using Godot;
using MountainGoap;

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
	protected float ThinkInterval = 0.2f;
	[Export]
	protected float FirearmDamage = 0.0f;
	[Export]
	protected float BluntDamage = 0.0f;
	[Export]
	protected float BladedDamage = 0.0f;

	protected System.Random RandomFactory = new System.Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day );

	protected CharacterBody2D SightTarget;
	protected float SightDetectionAmount = 0.0f;
	protected Godot.Vector2 AngleDir = Godot.Vector2.Zero;
	protected Godot.Vector2 NextPathPosition = Godot.Vector2.Zero;
	protected float LookAngle = 0.0f;
	protected float AimAngle = 0.0f;
	protected PointLight2D FlashLight;

	protected AudioStreamPlayer2D Bark;
	protected AudioStreamPlayer2D SequencedBark;
	protected AudioStreamPlayer2D ActionSfx;
	protected AudioStreamPlayer2D MoveSfx;
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

	protected void GenerateRaycasts() {
		int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
		SightLines = new List<RayCast2D>();

		for ( int i = 0; i < rayCount; i++ ) {
			RayCast2D ray = new RayCast2D();
			float angle = AngleBetweenRays * ( i - rayCount / 2.0f );
			ray.TargetPosition = AngleDir.Rotated( angle ) * MaxViewDistance;
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
			ray.TargetPosition = AngleDir.Rotated( angle ) * 140.0f;
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
		SequencedBark.Stream = null;
	}
	protected void OnBarkFinished() {
		Bark.Stream = null;
		if ( SequencedBark.Stream != null ) {
			// play another bark right after the first
			SequencedBark.Play();
		}
	}
	protected void OnBodyAnimationFinished() {
		if ( BodyAnimations.Animation == "move" ) {
			MoveSfx.CallDeferred( "play" );
		}
	}

	protected void Init() {
		ViewAngleAmount = Mathf.DegToRad( ViewAngleAmount );

		SightDetector = GetNode<Node2D>( "Animations/HeadAnimations/SightCheck" );
//		SightDetector = GetNode<Node2D>( "SightCheck" );
		DetectionMeter = GetNode<Line2D>( "DetectionMeter" );
		Navigation = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
		
		HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
		ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );
		BodyAnimations = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations" );
		BodyAnimations.Connect( "animation_looped", Callable.From( OnBodyAnimationFinished ) );
		
		MoveSfx = GetNode<AudioStreamPlayer2D>( "MoveSfx" );
		
//		ActionSfx = GetNode<AudioStreamPlayer2D>( "ActionSfx" );

		ThinkerTimer = new Timer();
		ThinkerTimer.WaitTime = 0.5f;
		ThinkerTimer.Autostart = true;
		ThinkerTimer.Timeout += () => { Think( (float)GetProcessDeltaTime() ); };
		AddChild( ThinkerTimer );
		
		Bark = new AudioStreamPlayer2D();
		Bark.VolumeDb = 10.0f;
		Bark.GlobalPosition = GlobalPosition;
		Bark.Connect( "finished", Callable.From( OnBarkFinished ) );
		AddChild( Bark );
		
		SequencedBark = new AudioStreamPlayer2D();
		SequencedBark.VolumeDb = 10.0f;
		SequencedBark.GlobalPosition = GlobalPosition;
		SequencedBark.Stream = null;
		SequencedBark.Connect( "finished", Callable.From( OnSequencedBarkFinished ) );
		AddChild( SequencedBark );
		
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
			AngleDir = Godot.Vector2.Up;
			break;
		case DirType.East:
			AngleDir = Godot.Vector2.Right;
			break;
		case DirType.South:
			AngleDir = Godot.Vector2.Down;
			break;
		case DirType.West:
			AngleDir = Godot.Vector2.Left;
			break;
		};
		LookAngle = Mathf.Atan2( AngleDir.Y, AngleDir.X );
		AimAngle = LookAngle;

		GenerateRaycasts();
	}
	public override void _Process( double delta ) {
	}

	protected void ProcessAnimations()  {
		ArmAnimations.SetDeferred( "global_position", AimAngle );
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
	public virtual void Think( float delta ) {
	}
	
	protected bool MoveAlongPath() {
		Godot.Vector2 lookDir = GlobalPosition.DirectionTo( Navigation.GetNextPathPosition() );
		LookAngle = Mathf.Atan2( lookDir.Y, lookDir.X );
		AngleDir = lookDir * MovementSpeed;
		Velocity = AngleDir;
		return MoveAndSlide();
	}
	protected void SetNavigationTarget( Godot.Vector2 target ) {
		Navigation.TargetPosition = target;
		Agent.State[ "TargetReached" ] = false;
	}
	protected void OnTargetReached() {
		Agent.State[ "TargetReached" ] = true;
	}

#region Actions
	protected ExecutionStatus Action_GotoNode( Agent agent, MountainGoap.Action action ) {
		if ( (bool)Agent.State[ "TargetReached" ] ) {
			return ExecutionStatus.Succeeded;
		}
		MoveAlongPath();
		return ExecutionStatus.Executing;
	}
	protected ExecutionStatus Action_RunAway( Agent agent, MountainGoap.Action action ) {
		if ( Health <= 0.0f ) {
			return ExecutionStatus.Failed;
		} else if ( (float)agent.State[ "TargetDistance" ] > 1500.0f ) {
			return ExecutionStatus.Succeeded;
		}
		return ExecutionStatus.Executing;
	}
	protected ExecutionStatus Action_InvestigateNode( Agent agent, MountainGoap.Action action ) {
		if ( agent.State[ "SightTarget" ] != null ) {
			Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.Next( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
			Bark.Play();

			agent.State[ "HasTarget " ] = true;
			return ExecutionStatus.Succeeded;
		}

		return ExecutionStatus.Executing;
	}
#endregion
};