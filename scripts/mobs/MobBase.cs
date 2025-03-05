using System;
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

	protected PackedScene BloodSplatter;

	protected float AngleBetweenRays = Mathf.DegToRad( 2.0f );

	[ExportCategory("Detection")]
	[Export]
	protected float SoundDetectionLevel;
	[Export]
	protected float ViewAngleAmount = 45.0f;
	[Export]
	protected float MaxViewDistance = 0.0f;
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

	protected RandomNumberGenerator RandomFactory = new RandomNumberGenerator();

	protected CharacterBody2D SightTarget;
	protected float SightDetectionAmount = 0.0f;
	protected Godot.Vector2 AngleDir = Godot.Vector2.Zero;
	protected Godot.Vector2 NextPathPosition;

	protected Area2D SoundBounds;
	protected AudioStreamPlayer2D Bark;
	protected Timer ThinkerTimer;
	protected Timer LoseInterestTimer;
	protected Node2D SightDetector;
	protected Line2D DetectionMeter;
	protected AnimatedSprite2D Animations;
	protected NavigationAgent2D Navigation;
	protected Color DetectionColor;

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
			SightDetector.AddChild( ray );
			SightLines.Add( ray );
		}
	}
	protected void RecalcSight() {
		Agent.State[ "SightPosition" ] = GlobalPosition;
		SightDetector.GlobalTransform = GlobalTransform;
		for ( int i = 0; i < SightLines.Count; i++ ) {
			RayCast2D ray = SightLines[i];
			float angle = AngleBetweenRays * ( i - SightLines.Count / 2.0f );
			ray.TargetPosition = AngleDir.Rotated( angle ) * MaxViewDistance;
		}
	}

#region Utility
	protected bool IsDeadAI( CharacterBody2D bot ) {
		return (float)bot.Get( "health" ) <= 0.0f;
	}
	protected bool IsValidTarget( GodotObject target ) {
		return target is Player || target is MobBase;
	}
#endregion

	protected virtual void OnLoseInterestTimerTimeout() {
	}

	protected void Init() {
		ViewAngleAmount = Mathf.DegToRad( ViewAngleAmount );

		SoundBounds = GetNode<Area2D>( "Node2D/SoundBounds" );
		Animations = GetNode<AnimatedSprite2D>( "Animations/AnimatedSprite2D" );
		SightDetector = GetNode<Node2D>( "SightCheck" );
		DetectionMeter = GetNode<Line2D>( "DetectionMeter" );
		Navigation = GetNode<NavigationAgent2D>( "NavigationAgent2D" );

		Bark = new AudioStreamPlayer2D();
		Bark.VolumeDb = 10.0f;
		Bark.GlobalPosition = GlobalPosition;
		AddChild( Bark );

		DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

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

		GenerateRaycasts();
	}

	protected bool MoveAlongPath() {
		Godot.Vector2 nextPathPosition = Navigation.GetNextPathPosition();
		AngleDir = GlobalPosition.DirectionTo( nextPathPosition ) * MovementSpeed;
		Animations.Play( "move" );
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
			Bark.Stream = MobSfxCache.TargetSpotted[ RandomFactory.RandiRange( 0, MobSfxCache.TargetSpotted.Count - 1 ) ];
			Bark.Play();

			agent.State[ "HasTarget " ] = true;
			return ExecutionStatus.Succeeded;
		}

		return ExecutionStatus.Executing;
	}
#endregion
};