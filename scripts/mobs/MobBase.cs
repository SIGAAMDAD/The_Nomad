using System;
using System.Runtime.CompilerServices;
using Godot;
using Renown;
using Renown.World;

namespace Renown.Thinkers {
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
	
		Attacking,
	
		Scared, // Fear > 80
		Investigating, // investigating a node
	
		Dead, // U R DED, NO BIG SOOPRIZE
		
		Count
	};
	
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
	
	public partial class MobBase : Thinker {
		protected float AngleBetweenRays = Mathf.DegToRad( 2.0f );

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
		[Export]
		protected float SoundTolerance = 0.0f;

		[ExportCategory("Start")]
		[Export]
		protected AINodeCache NodeCache;
		[Export]
		protected NavigationLink2D PatrolRoute;

		[ExportCategory("Stats")]
		[Export]
		protected float FirearmDamage = 0.0f;
		[Export]
		protected float BluntDamage = 0.0f;
		[Export]
		protected float BladedDamage = 0.0f;

		protected System.Random RandomFactory;

		protected Entity SightTarget;
		protected float SightDetectionAmount = 0.0f;
	
		protected AnimatedSprite2D BodyAnimations;
		protected AnimatedSprite2D ArmAnimations;
		protected AnimatedSprite2D HeadAnimations;

		private AudioStreamPlayer2D BarkChannel;

		// memory
		protected AIAwareness Awareness = AIAwareness.Relaxed;
		protected Godot.Vector2 LastTargetPosition = Godot.Vector2.Zero;
		protected Godot.Vector2 SightPosition = Godot.Vector2.Zero;
		protected byte Fear = 0;
		protected CharacterBody2D Target = null;
		protected bool CanSeeTarget = false;
		protected bool AfterImageUpdated = false;

		protected AIState State = AIState.Guarding;

		protected BarkType LastBark;
		protected BarkType SequencedBark;
		protected Timer LoseInterestTimer;
		protected Timer ChangeInvestigateAngleTimer;
		protected Node2D SightDetector;
		protected Line2D DetectionMeter;
		protected Color DetectionColor;
		protected RayCast2D[] SightLines;
		protected PlayerSystem.AfterImage AfterImage;

		protected Tween Tweener;

		public override void Save() {
			base.Save();
		}
		public override void Load() {
			base.Load();

			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				BodyAnimations.Play( "dead" );

				OnDeath( null, this );
			}
		}

		public AIPatrolRoute GetPatrolRoute() => PatrolRoute as AIPatrolRoute;
		public float GetSoundTolerance() => SoundTolerance;
		public Godot.Vector2 GetLastTargetPosition() => LastTargetPosition;
		public Entity GetSightTarget() => SightTarget;

		private void OnDeath( Entity source, Entity target ) {
			Tweener?.CallDeferred( "free" );

			for ( int i = 0; i < SightLines.Length; i++ ) {
				SightDetector.CallDeferred( "remove_child", SightLines[i] );
				SightLines[i].QueueFree();
			}
			LoseInterestTimer.QueueFree();
			CallDeferred( "remove_child", LoseInterestTimer );

			ChangeInvestigateAngleTimer.QueueFree();
			CallDeferred( "remove_child", ChangeInvestigateAngleTimer );

			ArmAnimations.QueueFree();
			CallDeferred( "remove_child", ArmAnimations );

			HeadAnimations.QueueFree();
			CallDeferred( "remove_child", HeadAnimations );

			BarkChannel.QueueFree();
			CallDeferred( "remove_child", BarkChannel );

			SightDetector.QueueFree();
			CallDeferred( "remove_child", SightDetector );

			DetectionMeter.QueueFree();
			CallDeferred( "remove_child", DetectionMeter );

			NavAgent.QueueFree();
			CallDeferred( "remove_child", NavAgent );

			CollisionShape2D shape = GetNode<CollisionShape2D>( "BodyShape" );
			shape.QueueFree();
			CallDeferred( "remove_child", shape );

			if ( AudioChannel != null ) {
				AudioChannel.QueueFree();
				CallDeferred( "remove_child", AudioChannel );
			}
		}

		public virtual void Alert( Entity target ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			LastTargetPosition = target.GlobalPosition;
			LookDir = GlobalPosition.DirectionTo( LastTargetPosition );
			if ( Fear > 60 ) {
				State = AIState.Investigating;
				Bark( BarkType.Curse, RandomFactory.Next( 0, 100 ) > 60 ? BarkType.Quiet : BarkType.Count );
			} else {
				if ( Awareness == AIAwareness.Relaxed ) {
					Awareness = AIAwareness.Suspicious;
				}
				State = AIState.Investigating;
				PatrolRoute = null;
				Bark( BarkType.Confusion );
			}
			SetNavigationTarget( LastTargetPosition );
			Fear += 30;

			// TODO: make everyone else suspicious
		}
	
		public override void Damage( Renown.Entity source, float nAmount ) {
			BloodParticleFactory.Create( source.GlobalPosition, GlobalPosition );

			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			base.Damage( source, nAmount );
		}

		private AudioStream GetBarkResource( BarkType bark ) {
			switch ( bark ) {
			case BarkType.ManDown:
				return ResourceCache.ManDown[ RandomFactory.Next( 0, ResourceCache.ManDown.Length - 1 ) ];
			case BarkType.MenDown2:
				return ResourceCache.ManDown2;
			case BarkType.MenDown3:
				return ResourceCache.ManDown3;
			case BarkType.TargetSpotted:
				return ResourceCache.TargetSpotted[ RandomFactory.Next( 0, ResourceCache.TargetSpotted.Length - 1 ) ];
			case BarkType.TargetPinned:
				return ResourceCache.TargetPinned[ RandomFactory.Next( 0, ResourceCache.TargetPinned.Length - 1 ) ];
			case BarkType.TargetRunning:
				return ResourceCache.TargetRunning[ RandomFactory.Next( 0, ResourceCache.TargetRunning.Length - 1 ) ];
			case BarkType.Confusion:
				return ResourceCache.Confusion[ RandomFactory.Next( 0, ResourceCache.Confusion.Length - 1 ) ];
			case BarkType.Alert:
				return ResourceCache.Alert[ RandomFactory.Next( 0, ResourceCache.Alert.Length - 1 ) ];
			case BarkType.OutOfTheWay:
				return ResourceCache.OutOfTheWay[ RandomFactory.Next( 0, ResourceCache.OutOfTheWay.Length - 1 ) ];
			case BarkType.NeedBackup:
				return ResourceCache.NeedBackup[ RandomFactory.Next( 0, ResourceCache.NeedBackup.Length - 1 ) ];
			case BarkType.SquadWiped:
				return ResourceCache.SquadWiped;
			case BarkType.Curse:
				return ResourceCache.Curse[ RandomFactory.Next( 0, ResourceCache.Curse.Length - 1 ) ];
			case BarkType.CheckItOut:
				return ResourceCache.CheckItOut[ RandomFactory.Next( 0, ResourceCache.CheckItOut.Length - 1 ) ];
			case BarkType.Quiet:
				return ResourceCache.Quiet[ RandomFactory.Next( 0, ResourceCache.Quiet.Length - 1 ) ];
			case BarkType.Unstoppable:
				return ResourceCache.Unstoppable;
			case BarkType.Count:
			default:
				break;
			};
			return null;
		}
		protected void Bark( BarkType bark, BarkType sequenced = BarkType.Count ) {
			if ( Health <= 0.0f || LastBark == bark ) {
				return;
			}
			LastBark = bark;
			SequencedBark = sequenced;

			PlaySound( BarkChannel, GetBarkResource( bark ) );
		}
		protected void GenerateRaycasts() {
			int rayCount = (int)( ViewAngleAmount / AngleBetweenRays );
			SightLines = new RayCast2D[ rayCount ];
			for ( int i = 0; i < rayCount; i++ ) {
				RayCast2D ray = new RayCast2D();
				float angle = AngleBetweenRays * ( i - rayCount / 2.0f );
				ray.TargetPosition = Godot.Vector2.Right.Rotated( angle ) * MaxViewDistance;
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
				ray.TargetPosition = Godot.Vector2.Right.Rotated( angle ) * MaxViewDistance;
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
			if ( LookAngle == angle ) {
				angle = Randf( 0.0f, 360.0f );
			}
			LookDir = GlobalPosition.Rotated( angle );
			AimAngle = angle;
			LookAngle = angle;
			
			RecalcSight();
		}
		
		protected void OnBarkFinished() {
			BarkChannel.Stream = null;
			if ( SequencedBark != BarkType.Count ) {
				// play another bark right after the first
				BarkChannel.Stream = GetBarkResource( SequencedBark );
				BarkChannel.Play();
				SequencedBark = BarkType.Count;
			}
		}
		protected void OnMoveTimerTimeout() {
			if ( LinearVelocity != Godot.Vector2.Zero ) {
				MoveTimer.Start();
			} else if ( !ResourceCache.Initialized ) {
				return;
			}
			PlaySound( AudioChannel, ResourceCache.MoveGravelSfx[ RandomFactory.Next( 0, ResourceCache.MoveGravelSfx.Length - 1 ) ] );
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void CreateAfterImage() {
			if ( AfterImageUpdated ) {
				return;
			}
			AfterImageUpdated = true;
			AfterImage.Update( SightTarget as Player );
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
		protected override void OnScreenEnter() {
			base.OnScreenEnter();
			
			OnScreen = true;
		}
		protected override void OnScreenExit() {
			base.OnScreenExit();
	
			OnScreen = false;
		}
	
		protected override void ProcessAnimations()  {
			if ( Floor != null ) {
				if ( Floor.GetUpper() != null && Floor.GetUpper().GetPlayerStatus() ) {
					Visible = true;
				} else if ( Floor.IsInside() ) {
					Visible = Floor.GetPlayerStatus();
				}
			} else {
				Visible = true;
			}
	
			ArmAnimations.GlobalRotation = AimAngle;
			HeadAnimations.GlobalRotation = LookAngle;
			
			if ( LookAngle > 135.0f ) {
				HeadAnimations.FlipV = true;
			} else if ( LookAngle < 225.0f ) {
				HeadAnimations.FlipV = false;
			}
			if ( AimAngle > 135.0f ) {
				ArmAnimations.FlipV = true;
			} else if ( AimAngle < 225.0f ) {
				ArmAnimations.FlipV = false;
			}
			if ( LinearVelocity.X > 0.0f ) {
				BodyAnimations.FlipH = false;
			} else if ( LinearVelocity.X < 0.0f ) {
				BodyAnimations.FlipH = true;
			}

			if ( LinearVelocity != Godot.Vector2.Zero ) {
				BodyAnimations.Play( "move" );
				ArmAnimations.Play( "move" );
				HeadAnimations.Play( "move" );
			} else {
				if ( Awareness == AIAwareness.Relaxed ) {
					BodyAnimations.Play( "calm" );
					ArmAnimations.Hide();
					HeadAnimations.Hide();
				} else {
					ArmAnimations.Show();
					HeadAnimations.Show();
					BodyAnimations.Play( "idle" );
					ArmAnimations.Play( "idle" );
					HeadAnimations.Play( "idle" );
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			Die += OnDeath;

			RandomFactory = new System.Random();

			ViewAngleAmount = Mathf.DegToRad( ViewAngleAmount );

			SightDetector = GetNode<Node2D>( "Animations/HeadAnimations/SightCheck" );
	
			DetectionMeter = GetNode<Line2D>( "DetectionMeter" );
			DetectionMeter.SetProcess( false );
			DetectionMeter.SetProcessInternal( false );

			BarkChannel = new AudioStreamPlayer2D();
			BarkChannel.Name = "BarkChannel";
			BarkChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AddChild( BarkChannel );

			HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
			ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );
			BodyAnimations = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations" );

			MoveTimer = new Timer();
			MoveTimer.Name = "MoveTimer";
			MoveTimer.OneShot = true;
			MoveTimer.WaitTime = 0.40f;
			MoveTimer.Connect( "timeout", Callable.From( OnMoveTimerTimeout ) );
			MoveTimer.SetProcess( false );
			MoveTimer.SetProcessInternal( false );
			AddChild( MoveTimer );
		
			DetectionColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

			AfterImage = new PlayerSystem.AfterImage();
			AfterImage.SetProcess( false );
			AfterImage.SetProcessInternal( false );
			GetTree().CurrentScene.CallDeferred( "add_child", AfterImage );
		
			ChangeInvestigateAngleTimer = new Timer();
			ChangeInvestigateAngleTimer.Name = "ChangeInvestigateAngleTimer";
			ChangeInvestigateAngleTimer.WaitTime = 1.5f;
			ChangeInvestigateAngleTimer.OneShot = true;
			ChangeInvestigateAngleTimer.SetProcess( false );
			ChangeInvestigateAngleTimer.SetProcessInternal( false );
			ChangeInvestigateAngleTimer.Connect( "timeout", Callable.From( OnChangeInvestigateAngleTimerTimeout ) );
			AddChild( ChangeInvestigateAngleTimer );
		
			LoseInterestTimer = new Timer();
			LoseInterestTimer.Name = "LoseInterestTimer";
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
			default:
				Console.PrintLine( "MobBase._Ready: Invalid direction!" );
				break;
			};
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			AimAngle = LookAngle;

			GenerateRaycasts();

			if ( PatrolRoute != null ) {
				SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
				State = AIState.Patrolling;
			}
		}
		
		protected override void OnTargetReached() {
			TargetReached = true;
			
			switch ( State ) {
			case AIState.PatrolStart:
				SetNavigationTarget( PatrolRoute.GetGlobalEndPosition() );
				State = AIState.Patrolling;
				break;
			case AIState.Patrolling:
				PatrolRoute = ( (AIPatrolRoute)PatrolRoute ).GetNext();
				SetNavigationTarget( PatrolRoute.GetGlobalStartPosition() );
				break;
			default:
				GotoPosition = GlobalPosition;
				LinearVelocity = Godot.Vector2.Zero;
				break;
			};
		}

		protected void SetAlert( bool bRunning ) {
			if ( Awareness != AIAwareness.Alert ) {
				Bark( BarkType.TargetSpotted );
			}
			Awareness = AIAwareness.Alert;
		}
		protected void SetSuspicious() {
			if ( Awareness != AIAwareness.Suspicious ) {
				Bark( BarkType.Confusion );
			}
			Awareness = AIAwareness.Suspicious;
		}
		protected void CheckSight( float delta ) {
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
			
			if ( sightTarget is Entity entity && entity != null ) {
				if ( entity.GetHealth() <= 0.0f ) {
					// dead?
				}
				else if ( entity.GetFaction() != Faction ) {
					// not in the same faction, evaluate hostility
					// TODO: make this more abstract/complex
					SightTarget = entity;
					CanSeeTarget = true;
					LastTargetPosition = entity.GlobalPosition;
					
					if ( Awareness >= AIAwareness.Suspicious ) {
						// if we're already suspicious, then detection rate increases as we're more alert
						SightDetectionAmount += ( SightDetectionSpeed * 2.0f ) * delta;
					} else {
						SightDetectionAmount += SightDetectionSpeed * delta;
					}
				}
			}
			if ( SightDetectionAmount >= SightDetectionTime * 0.5f ) {
				Awareness = AIAwareness.Suspicious;
				State = AIState.Investigating;
				if ( LoseInterestTimer.IsStopped() ) {
					LoseInterestTimer.Start();
				}
			}
			if ( IsAlert() ) {
				SetAlert( false );
			} else if ( IsSuspicious() ) {
				SetSuspicious();
			}
			SetDetectionColor();
		}
	};
};
