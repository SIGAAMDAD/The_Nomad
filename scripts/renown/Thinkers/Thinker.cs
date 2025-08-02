using Godot;
using System.Collections.Generic;
using Renown.World;
using DialogueManagerRuntime;

namespace Renown.Thinkers {
	public enum ThinkerFlags : uint {
		// physics slapped
		Pushed = 0x00000001,

		// ...duh
		Dead = 0x00000002,
	};
	public enum DirType : uint {
		North,
		East,
		South,
		West,

		Count
	};

	public enum DialogueType : uint {
		Friendly,
		Catious,
		Hostile,

		Count
	};

	public partial class Thinker : Entity {
		[Export]
		protected TileMapFloor Floor;

		[Export]
		protected Resource DialogueResource = ResourceLoader.Load( "res://resources/dialogue/thinker.dialogue" );

		[Export]
		protected bool IsPremade = false;
		
		[ExportCategory( "Start" )]
		[Export]
		protected DirType Direction;

		[ExportCategory( "Stats" )]

		[Export]
		protected bool HasMetPlayer = false;
		[Export]
		public float MovementSpeed { get; protected set; } = 200.0f;

		protected Node2D Animations;

		protected NavigationAgent2D NavAgent;
		protected Rid NavAgentRID { get; private set; }
		protected Godot.Vector2 LookDir = Godot.Vector2.Zero;

		protected NodePath InitialPath;

		protected Godot.Vector2 PhysicsPosition = Godot.Vector2.Zero;

		protected ThinkerFlags Flags;
		protected ThinkerGroup Squad;

		protected bool Initialized = false;

		// memory
		public bool TargetReached { get; protected set; } = false;
		protected Godot.Vector2 GotoPosition = Godot.Vector2.Zero;
		public float LookAngle = 0.0f;
		public float AimAngle = 0.0f;

		protected static readonly Color DefaultColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		protected Color DemonEyeColor;

		// networking
		protected NetworkSyncObject SyncObject = null;

		public AnimatedSprite2D BodyAnimations { get; protected set; }
		public AnimatedSprite2D ArmAnimations { get; protected set; }
		public AnimatedSprite2D HeadAnimations { get; protected set; }

		protected AudioStreamPlayer2D AudioChannel;

		protected VisibleOnScreenNotifier2D VisibilityNotifier;
		protected bool OnScreen = false;

		protected int ThreadSleep = Constants.THREADSLEEP_THINKER_PLAYER_IN_AREA;

		public void SetTileMapFloor( TileMapFloor floor ) => Floor = floor;
		public TileMapFloor GetTileMapFloor() => Floor;

		public virtual void MeetPlayer() {
			switch ( GetRelationStatus( LevelData.Instance.ThisPlayer ) ) {
			case RelationStatus.Neutral:
				if ( HasMetPlayer ) {
					DialogueManager.ShowDialogueBalloon( DialogueResource, "meet_neutral" );
				} else {
					DialogueManager.ShowDialogueBalloon( DialogueResource, "talk_neutral" );
				}
				break;
			case RelationStatus.Friends:
				if ( HasMetPlayer ) {
					DialogueManager.ShowDialogueBalloon( DialogueResource, "meet_friendly" );
				} else {
					DialogueManager.ShowDialogueBalloon( DialogueResource, "talk_friendly" );
				}
				break;
			case RelationStatus.GoodFriends:
				break;
			};
		}

		public virtual void Alert( Entity source ) {
		}

		public override void Damage( in Entity source, float nAmount ) {
			BloodParticleFactory.CreateDeferred( source != null ? source.GlobalPosition : Godot.Vector2.Zero, GlobalPosition );
			base.Damage( source, nAmount );
		}

		protected void SendPacket() {
			if ( !OnScreen ) {
				return;
			}

			//			SyncObject.Write( GlobalPosition );
			//			SyncObject.Write( BodyAnimations.Animation );
			//			SyncObject.Write( HeadAnimations != null );
			//			SyncObject.Write( ArmAnimations != null );
			//			SyncObject.Sync();
		}

		public void Notify( GroupEvent nEventType, Thinker source ) {
		}
		protected void FindGroup() {
		}

		protected virtual void OnPlayerEnteredArea() {
			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				return;
			}
			SetProcess( true );
			SetPhysicsProcess( true );

			System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_THINKER_PLAYER_IN_AREA );
			ProcessThreadGroupOrder = Constants.THREAD_GROUP_THINKERS;

			if ( SettingsData.GetNetworkingEnabled() && GameConfiguration.GameMode == GameMode.Multiplayer ) {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, SendPacket, null ) );
			}

			if ( ( Flags & ThinkerFlags.Dead ) == 0 ) {
				AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
				Animations.ProcessMode = ProcessModeEnum.Pausable;
			}
		}
		protected void OnPlayerExitedArea() {
			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				return;
			}
			ProcessThreadGroupOrder = Constants.THREAD_GROUP_THINKERS_AWAY;

			if ( Location.GetBiome().IsPlayerHere() ) {
				System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_THINKER_PLAYER_IN_BIOME );
			} else {
				System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_THINKER_PLAYER_AWAY );
			}

			if ( SettingsData.GetNetworkingEnabled() && GameConfiguration.GameMode == GameMode.Multiplayer ) {
				SteamLobby.Instance.RemoveNetworkNode( GetPath() );
			}

			if ( ( Flags & ThinkerFlags.Dead ) == 0 ) {
				AudioChannel.ProcessMode = ProcessModeEnum.Disabled;
				Animations.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
		public override void SetLocation( in WorldArea location ) {
			if ( Location != null ) {
				Location.PlayerEntered -= OnPlayerEnteredArea;
				Location.PlayerExited -= OnPlayerExitedArea;
			}

			if ( location == null ) {
				Location = null;
				return;
			}

			Location = location;

			Location.PlayerEntered += OnPlayerEnteredArea;
			Location.PlayerExited += OnPlayerExitedArea;
		}

		public override void PlaySound( AudioStreamPlayer2D channel, AudioStream stream ) {
			if ( channel == null ) {
				AudioChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, stream );
				AudioChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
			} else {
				channel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, stream );
				channel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
			}
		}

		public override StringName GetObjectName() => Name;

		public virtual void Save( SaveSystem.SaveSectionWriter writer, int nIndex ) {
			string key = "Thinker" + nIndex;

			writer.SaveBool( key + nameof( IsPremade ), IsPremade );

			writer.SaveVector2( key + nameof( GlobalPosition ), GlobalPosition );
			writer.SaveFloat( key + nameof( Health ), Health );

			writer.SaveUInt( key + nameof( Flags ), (uint)Flags );
			writer.SaveFloat( key + nameof( MovementSpeed ), MovementSpeed );
			writer.SaveBool( key + nameof( HasMetPlayer ), HasMetPlayer );
		}
		public virtual void Load( SaveSystem.SaveSectionReader reader, int nIndex ) {
			string key = "Thinker" + nIndex;

			GlobalPosition = reader.LoadVector2( key + nameof( GlobalPosition ) );
			Health = reader.LoadFloat( key + nameof( Health ) );

			Flags = (ThinkerFlags)reader.LoadUInt( key + nameof( Flags ) );
			MovementSpeed = reader.LoadFloat( key + nameof( MovementSpeed ) );
			HasMetPlayer = reader.LoadBoolean( key + nameof( HasMetPlayer ) );
		}

		public virtual void StopMoving() {
			NavigationServer2D.AgentSetVelocityForced( NavAgentRID, Godot.Vector2.Zero );
			Velocity = Godot.Vector2.Zero;
			GotoPosition = GlobalPosition;
		}

		protected void SetAnimationsColor( Color color ) {
			Animations?.SetDeferred( PropertyName.Modulate, color );
		}

		protected void OnScreenEnter() {
			OnScreen = true;
		}
		protected void OnScreenExit() {
			OnScreen = false;
		}

		public Thinker() {
			if ( !IsInGroup( "Thinkers" ) ) {
				AddToGroup( "Thinkers" );
			}
		}

		protected void OnBodyAnimationFinished() {
			if ( BodyAnimations.Animation == "move" ) {
				PlaySound( AudioChannel, ResourceCache.MoveGravelSfx[ RNJesus.IntRange( 0, ResourceCache.MoveGravelSfx.Length - 1 ) ] );
			}
		}

		protected virtual void OnDie( Entity source, Entity target ) {
			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				if ( IsInGroup( "Enemies" ) ) {
					RemoveFromGroup( "Enemies" );
					ChallengeLevel.SetObjectiveState( "EnemyKillCount", (int)ChallengeLevel.GetObjectiveState( "EnemyKillCount" ) + 1 );
					ChallengeLevel.SetObjectiveState( "AllEnemiesDead", GetTree().GetNodesInGroup( "Enemies" ).Count < 1 );
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			Die += OnDie;

			Animations = GetNode<Node2D>( "Animations" );

			BodyAnimations = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations" );
			BodyAnimations.ProcessThreadGroup = ProcessThreadGroupEnum.MainThread;

			if ( Animations.FindChild( "ArmAnimations" ) != null ) {
				ArmAnimations = GetNode<AnimatedSprite2D>( "Animations/ArmAnimations" );
				ArmAnimations.ProcessThreadGroup = ProcessThreadGroupEnum.MainThread;
			}
			if ( Animations.FindChild( "HeadAnimations" ) != null ) {
				HeadAnimations = GetNode<AnimatedSprite2D>( "Animations/HeadAnimations" );
				HeadAnimations.ProcessThreadGroup = ProcessThreadGroupEnum.MainThread;
			}

			ProcessMode = ProcessModeEnum.Pausable;
			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = (int)GetRid().Id;

			AudioChannel = new AudioStreamPlayer2D();
			AudioChannel.Name = "AudioChannel";
			AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
			AddChild( AudioChannel );

			NavAgent = new NavigationAgent2D();
			NavAgent.Name = "NavAgent";
			NavAgent.PathMaxDistance = 10000.0f;
			NavAgent.AvoidanceEnabled = true;
			NavAgent.AvoidanceLayers = 1;
			NavAgent.AvoidanceMask = 1;
			NavAgent.NeighborDistance = 1024.0f;
			NavAgent.Radius = 20.0f;
			NavAgent.MaxNeighbors = 1024;
			NavAgent.MaxSpeed = MovementSpeed;
			NavAgent.PathPostprocessing = NavigationPathQueryParameters2D.PathPostProcessing.Edgecentered;
			NavAgent.TimeHorizonAgents = 2.0f;
			NavAgent.MaxSpeed = MovementSpeed;
			NavAgent.ProcessMode = ProcessModeEnum.Pausable;
			NavAgent.Connect( NavigationAgent2D.SignalName.VelocityComputed, Callable.From<Godot.Vector2>( ( safeVelocity ) => {
				Velocity = safeVelocity;// * (float)GetPhysicsProcessDeltaTime();
				CallDeferred( MethodName.MoveAlongPath );
			} ) );
			NavAgent.Connect( NavigationAgent2D.SignalName.TargetReached, Callable.From( OnTargetReached ) );
			AddChild( NavAgent );

			NavAgentRID = NavAgent.GetRid();

			/*
			VisibilityNotifier = GetNode<VisibleOnScreenNotifier2D>( "VisibleOnScreenNotifier2D" );
			VisibilityNotifier.Connect( "screen_entered", Callable.From( OnScreenEnter ) );
			VisibilityNotifier.Connect( "screen_exited", Callable.From( OnScreenExit ) );
			AddChild( VisibilityNotifier );
			*/

			if ( IsPremade ) {
				InitialPath = GetPath();
			}

			ProcessMode = ProcessModeEnum.Pausable;
			GotoPosition = GlobalPosition;

			if ( ArchiveSystem.Instance.IsLoaded() ) {
				Load();
			}
			AddToGroup( "Archive" );
		}
		public override void PhysicsUpdate( double delta ) {
			if ( Health <= 0.0f ) {
				return;
			}

			base._PhysicsProcess( delta );

			NavigationServer2D.AgentSetVelocity( NavAgentRID, LookDir * MovementSpeed );

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 ) {
				if ( Velocity == Godot.Vector2.Zero ) {
					Flags &= ~ThinkerFlags.Pushed;
				} else {
					return;
				}
			}
		}
		public override void Update( double delta ) {
			ProcessAnimations();

			if ( ( Engine.GetProcessFrames() % 15 ) != 0 ) {
				return;
			}

			base._Process( delta );

			SetAnimationsColor( GameConfiguration.DemonEyeActive ? DemonEyeColor : DefaultColor );

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 || Health <= 0.0f ) {
				return;
			}

			Think();
		}

		protected virtual void Think() {
		}

		private static float CalcAngle( float animationAngle, AnimatedSprite2D animation ) {
			float angle = Mathf.RadToDeg( animationAngle );
			if ( angle < -80.0f ) {
				angle = 260.0f;
			} else if ( angle > 260.0f ) {
				angle = -80.0f;
			} else if ( angle > 80.0f && angle < 100.0f ) {
				angle -= 20.0f;
			} else if ( angle < 100.0f && angle > 80.0f ) {
				angle += 20.0f;
			}
			if ( angle > 80.0f ) {
				animation.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, true );
			} else if ( angle < 100.0f ) {
				animation.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, false );
			}
			animationAngle = Mathf.DegToRad( angle );
			animation.SetDeferred( PropertyName.GlobalRotation, animationAngle );
			return animationAngle;
		}
		/// <summary>
		/// sets the current frame's animations based on thinker subclass
		/// </summary>
		protected virtual void ProcessAnimations() {
			if ( !Visible || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}

			if ( HeadAnimations != null ) {
				HeadAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipV, Mathf.RadToDeg( LookAngle ) > 90.0f );
				HeadAnimations.SetDeferred( PropertyName.GlobalRotation, LookAngle );
			}
			if ( ArmAnimations != null ) {
				AimAngle = CalcAngle( AimAngle, ArmAnimations );
			}
			if ( Velocity == Godot.Vector2.Zero ) {
				BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
				ArmAnimations?.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
				HeadAnimations?.CallDeferred( AnimatedSprite2D.MethodName.Play, "idle" );
			} else {
				if ( Velocity.X < 0.0f ) {
					BodyAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipH, true );
				} else if ( Velocity.X > 0.0f ) {
					BodyAnimations.SetDeferred( AnimatedSprite2D.PropertyName.FlipH, false );
				}
			}
		}

		protected virtual bool MoveAlongPath() {
			if ( NavAgent.IsTargetReached() || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				Velocity = Godot.Vector2.Zero;
				return true;
			}
			Godot.Vector2 nextPathPosition = NavAgent.GetNextPathPosition();
			LookDir = GlobalPosition.DirectionTo( nextPathPosition );
			MoveAndSlide();
			//			GlobalPosition += Velocity * (float)GetPhysicsProcessDeltaTime();

			return true;
		}
		protected virtual void SetNavigationTarget( Godot.Vector2 target ) {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			NavAgent.TargetPosition = target;
			TargetReached = false;
			GotoPosition = target;

			BodyAnimations.CallDeferred( AnimatedSprite2D.MethodName.Play, "move" );
			ArmAnimations?.CallDeferred( AnimatedSprite2D.MethodName.Play, "move" );
			HeadAnimations?.CallDeferred( AnimatedSprite2D.MethodName.Play, "move" );
		}
		protected virtual void OnTargetReached() {
			if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
				return;
			}
			TargetReached = true;
			GotoPosition = GlobalPosition;
			Velocity = Godot.Vector2.Zero;
		}

		/*
		protected class NameGenerator {
			protected static System.Random random = new System.Random();

			protected static readonly string[] FirstNameScramble_Begin = [
				"Mess",
				"Olan",
				"Rea",
				"Baf",
				"Fern",
				"Hala",
				"Rewea",
				"Ego",
				"Quare",
				"Wedne",
				"Offa",
				"Firo"
			];
			protected static readonly string[] FirstNameScramble_End = [
				"ga",
				"ka",
				"ro",
				"ed",
				"va",
				"fen",
				"o",
				"a",
				"i",
				"ira",
				""
			];

			// TODO: make regional name generation a thing
			private static string NameScramble( string[] begin, string[] end ) {
				return string.Format( "{0}{1}", begin[ RNJesus.IntRange( 0, begin.Length - 1 ) ], end[ RNJesus.IntRange( 0, end.Length - 1 ) ] );
			}
			public static string GenerateName() {
				return NameScramble( FirstNameScramble_Begin, FirstNameScramble_End );
			}
		};
		*/
	};
};
