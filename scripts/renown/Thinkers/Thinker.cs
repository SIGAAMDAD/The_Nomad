using Godot;
using System.Collections.Generic;
using Renown.World;
using Renown.World.Buildings;
using System;
using ChallengeMode;
using DialogueManagerRuntime;

namespace Renown.Thinkers {
	public enum ThinkerFlags : uint {
		// physics slapped
		Pushed		= 0x00000001,
		
		// ...duh
		Dead		= 0x00000002,
		
		Intoxicated	= 0x00000004,
		Pregnant	= 0x00000008,
		Sterile		= 0x00000010,
	};
	public enum Sex : uint {
		Male,
		Female,

		Count
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
	
	// most thinkers except for politicians will most likely never get the chance nor the funds
	// to hire a personal mercenary
	public partial class Thinker : Entity {
		[Export]
		protected TileMapFloor Floor;

		[Export]
		protected Resource DialogueResource;

		[Export]
		protected bool IsPremade = false;
		[Export]
		protected Building Home;
		[Export]
		protected StringName BotName;
		[Export]
		protected int Age = 0; // in years
		[Export]
		protected Family Family;
		[Export]
		protected Settlement BirthPlace = null;
		[Export]
		protected Sex Sex;

		[Export]
		protected StringName FirstName;

		[Export]
		public Node AnimationStateMachine;
		[Export]
		public Node BehaviourTree;

		[ExportCategory( "Start" )]
		[Export]
		protected DirType Direction;

		[ExportCategory( "Stats" )]

		/// <summary>
		/// physical power
		/// </summary>
		[Export]
		protected int Strength;

		/// <summary>
		/// manuverability, reflexes
		/// </summary>
		[Export]
		protected int Dexterity;

		/// <summary>
		/// quantity of data, not how to use it
		/// </summary>
		[Export]
		protected int Intelligence;

		/// <summary>
		/// resistances to poisons, illnesses, etc. overall health
		/// </summary>
		[Export]
		protected int Constitution;

		/// <summary>
		/// "common sense"
		/// </summary>
		[Export]
		protected int Wisdom;

		[Export]
		protected int Charisma;

		[Export]
		protected bool HasMetPlayer = false;
		[Export]
		public float MovementSpeed {
			get;
			protected set;
		} = 200.0f;

		protected Node2D Animations;

		public NavigationAgent2D NavAgent {
			get;
			private set;
		}
		public Godot.Vector2 LookDir {
			get;
			protected set;
		} = Godot.Vector2.Zero;

		protected NodePath InitialPath;

		protected Godot.Vector2 PhysicsPosition = Godot.Vector2.Zero;

		protected ThinkerFlags Flags;
		protected ThinkerGroup Squad;

		protected bool Initialized = false;

		// memory
		public bool TargetReached {
			get;
			protected set;
		} = false;
		protected Godot.Vector2 GotoPosition = Godot.Vector2.Zero;
		public float LookAngle {
			get;
			protected set;
		}
		public float AimAngle {
			get;
			protected set;
		}

		protected static readonly Color DefaultColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		protected Color DemonEyeColor;

		// networking
		protected NetworkSyncObject SyncObject = null;

		public AnimatedSprite2D BodyAnimations {
			get;
			private set;
		}

		protected AudioStreamPlayer2D AudioChannel;

		protected VisibleOnScreenNotifier2D VisibilityNotifier;
		protected bool OnScreen = false;

		protected SocietyRank SocietyRank;

		protected object LockObject = new object();
		protected System.Threading.Thread ThinkThread = null;
		protected bool Quit = false;

		protected int ThreadSleep = Constants.THREADSLEEP_THINKER_PLAYER_IN_AREA;

		[Signal]
		public delegate void HaveChildEventHandler( Thinker parent );
		[Signal]
		protected delegate void ActionFinishedEventHandler();

		public SocietyRank GetSocietyRank() => SocietyRank;
		public int GetAge() => Age;
		public StringName GetFirstName() => FirstName;

		public void SetHome( Building building ) => Home = building;
		public Building GetHome() => Home;

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
			}
			;
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

			System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_FACTION_PLAYER_IN_AREA );
			ProcessThreadGroupOrder = Constants.THREAD_GROUP_THINKERS;

			Visible = true;

			if ( SettingsData.GetNetworkingEnabled() ) {
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

			Visible = false;

			if ( SettingsData.GetNetworkingEnabled() ) {
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
				AudioChannel.SetDeferred( "stream", stream );
				AudioChannel.CallDeferred( "play" );
			} else {
				channel.SetDeferred( "stream", stream );
				channel.CallDeferred( "play" );
			}
		}

		public override StringName GetObjectName() => Name;

		public void Save( SaveSystem.SaveSectionWriter writer, int nIndex ) {
			string key = "Thinker" + nIndex;
			int count;

			writer.SaveBool( key + nameof( IsPremade ), IsPremade );
			writer.SaveString( key + nameof( Family ), Family.GetFamilyName() );

			writer.SaveVector2( key + nameof( GlobalPosition ), GlobalPosition );
			writer.SaveFloat( key + nameof( Health ), Health );
			writer.SaveInt( key + nameof( Age ), Age );
			writer.SaveString( key + nameof( BotName ), BotName );

			// stats
			writer.SaveInt( key + nameof( Strength ), Strength );
			writer.SaveInt( key + nameof( Dexterity ), Dexterity );
			writer.SaveInt( key + nameof( Wisdom ), Wisdom );
			writer.SaveInt( key + nameof( Intelligence ), Intelligence );
			writer.SaveInt( key + nameof( Constitution ), Constitution );

			writer.SaveUInt( key + nameof( Flags ), (uint)Flags );
			writer.SaveFloat( key + nameof( MovementSpeed ), MovementSpeed );
			writer.SaveBool( key + nameof( HasMetPlayer ), HasMetPlayer );

			writer.SaveInt( key + "RelationCount", RelationCache.Count );
			count = 0;
			foreach ( var relation in RelationCache ) {
				if ( relation.Object is Entity entity && entity != null ) {
					writer.SaveBool( string.Format( "{0}RelationIsEntity{1}", key, count ), true );
				} else {
					writer.SaveBool( string.Format( "{0}RelationIsEntity{1}", key, count ), false );
				}
				writer.SaveString( string.Format( "{0}RelationNode{1}", key, count ), relation.Object.GetObjectName() );
				writer.SaveFloat( string.Format( "{0}RelationValue{1}", key, count ), relation.Value );
				count++;
			}

			writer.SaveInt( key + "DebtCount", DebtCache.Count );
			count = 0;
			foreach ( var debt in DebtCache ) {
				writer.SaveString( string.Format( "{0}DebtNode{1}", key, count ), debt.Object.GetObjectName() );
				writer.SaveFloat( string.Format( "{0}DebtValue{1}", key, count ), debt.Value );
				count++;
			}

			writer.SaveInt( key + "TraitCount", TraitCache.Count );
			count = 0;
			foreach ( var trait in TraitCache ) {
				writer.SaveUInt( string.Format( "{0}TraitType{1}", key, count ), (uint)trait.GetTraitType() );
				count++;
			}
		}
		protected void SetLocationDeferred( string locationId ) {
			Location = ( (Node)Engine.GetMainLoop().Get( "root" ) ).GetNode<WorldArea>( locationId );
		}
		public void Load( SaveSystem.SaveSectionReader reader, int nIndex ) {
			string key = "Thinker" + nIndex;

			GlobalPosition = reader.LoadVector2( key + nameof( GlobalPosition ) );
			Health = reader.LoadFloat( key + nameof( Health ) );
			Age = reader.LoadInt( key + nameof( Age ) );
			BotName = reader.LoadString( key + nameof( BotName ) );

			if ( !IsPremade ) {
				Family = FamilyCache.GetFamily( reader.LoadString( key + nameof( Family ) ) );
				Home = Family.GetHome();
			}

			Flags = (ThinkerFlags)reader.LoadUInt( key + nameof( Flags ) );
			MovementSpeed = reader.LoadFloat( key + nameof( MovementSpeed ) );
			HasMetPlayer = reader.LoadBoolean( key + nameof( HasMetPlayer ) );

			Strength = reader.LoadInt( key + nameof( Strength ) );
			Dexterity = reader.LoadInt( key + nameof( Dexterity ) );
			Wisdom = reader.LoadInt( key + nameof( Wisdom ) );
			Intelligence = reader.LoadInt( key + nameof( Intelligence ) );
			Constitution = reader.LoadInt( key + nameof( Constitution ) );

			int relationCount = reader.LoadInt( key + "RelationCount" );
			RelationCache = new HashSet<RenownValue>( relationCount );
			for ( int i = 0; i < relationCount; i++ ) {
				RelationCache.Add( new RenownValue(
					(Object)GetTree().Root.GetNode( reader.LoadString( string.Format( "{0}RelationNode{1}", key, i ) ) ),
					reader.LoadFloat( string.Format( "{0}RelationValue{1}", key, i ) )
				) );
			}

			int debtCount = reader.LoadInt( key + "DebtCount" );
			DebtCache = new HashSet<RenownValue>( debtCount );
			for ( int i = 0; i < debtCount; i++ ) {
				DebtCache.Add( new RenownValue(
					(Object)GetTree().Root.GetNode( reader.LoadString( string.Format( "{0}DebtNode{1}", key, i ) ) ),
					reader.LoadFloat( string.Format( "{0}DebtValue{1}", key, i ) )
				) );
			}

			int traitCount = reader.LoadInt( key + "TraitCount" );
			TraitCache = new HashSet<Trait>( traitCount );
			for ( int i = 0; i < traitCount; i++ ) {
				TraitCache.Add( Trait.Create( (TraitType)reader.LoadUInt( string.Format( "{0}TraitType{1}", key, i ) ) ) );
			}
		}

		public virtual void StopMoving() {
			NavigationServer2D.AgentSetVelocityForced( NavAgent.GetRid(), Godot.Vector2.Zero );
			Velocity = Godot.Vector2.Zero;
			GotoPosition = GlobalPosition;

			if ( Visible ) {
				BodyAnimations.CallDeferred( "play", "idle" );
			}
		}

		protected void SetAnimationsColor( Color color ) {
			Animations?.SetDeferred( "modulate", color );
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

		public override void _ExitTree() {
			base._ExitTree();

			Quit = true;
		}
		public override void _Ready() {
			base._Ready();

			// TODO: make renown system operate in some challenge modes
			if ( GameConfiguration.GameMode != GameMode.ChallengeMode && GameConfiguration.GameMode != GameMode.JohnWick ) {
				InitRenownStats();
			}

			Die += OnDie;

			Animations = GetNode<Node2D>( "Animations" );
			BodyAnimations = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations" );

			ProcessMode = ProcessModeEnum.Inherit;
			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = Constants.THREAD_GROUP_THINKERS;

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
			if ( GameConfiguration.GameMode == GameMode.JohnWick ) {
				NavAgent.Radius = 30.0f;
			} else {
				NavAgent.Radius = 80.0f;
			}
			NavAgent.MaxNeighbors = 30;
			NavAgent.MaxSpeed = MovementSpeed;
			NavAgent.TimeHorizonAgents = 2.0f;
			NavAgent.MaxSpeed = MovementSpeed;
			NavAgent.ProcessMode = ProcessModeEnum.Pausable;
			NavAgent.Connect( "velocity_computed", Callable.From<Godot.Vector2>( ( safeVelocity ) => {
				Velocity = safeVelocity;
				CallDeferred( "MoveAlongPath" );
			} ) );
			NavAgent.Connect( "target_reached", Callable.From( OnTargetReached ) );
			AddChild( NavAgent );

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

			SetMeta( "Faction", Faction );
		}
		public override void _PhysicsProcess( double delta ) {
			if ( Health <= 0.0f ) {
				return;
			}

			base._PhysicsProcess( delta );

			//NavigationServer2D.AgentSetVelocity( NavAgent.GetRid(), LookDir * MovementSpeed );

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 ) {
				if ( Velocity == Godot.Vector2.Zero ) {
					Flags &= ~ThinkerFlags.Pushed;
				} else {
					return;
				}
			}
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % (ulong)ThreadSleep ) != 0 ) {
				return;
			}

			base._Process( delta );

			SetAnimationsColor( GameConfiguration.DemonEyeActive ? DemonEyeColor : DefaultColor );
			//			ProcessAnimations();

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 || Health <= 0.0f ) {
				return;
			}

			RenownProcess();
			Think();
		}

		protected void CheckContracts() {
			foreach ( var relation in RelationCache ) {
				if ( GetRelationStatus( relation.Object ) > RelationStatus.Dislikes ) {
					// we've got a grudge

				}
			}
		}

		/// <summary>
		/// Processes various relations, debts, etc. to run the renown system per entity.
		/// expensive, but not animations and very little godot engine calls will be present.
		/// </summary>
		protected void RenownProcess() {
			SetAnimationsColor( GameConfiguration.DemonEyeActive ? DemonEyeColor : DefaultColor );

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 || Health <= 0.0f ) {
				return;
			}
			CheckContracts();
		}
		protected virtual void Think() {
		}
		/// <summary>
		/// sets the current frame's animations based on thinker subclass
		/// </summary>
		protected virtual void ProcessAnimations() {
		}

		protected virtual bool MoveAlongPath() {
			if ( NavAgent.IsTargetReached() || ( Flags & ThinkerFlags.Dead ) != 0 ) {
				Velocity = Godot.Vector2.Zero;
				return true;
			}
			Godot.Vector2 nextPathPosition = NavAgent.GetNextPathPosition();
			LookDir = GlobalPosition.DirectionTo( nextPathPosition );
			//			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			GlobalPosition += Velocity * (float)GetPhysicsProcessDeltaTime();
			return true;
		}
		protected virtual void SetNavigationTarget( Godot.Vector2 target ) {
			NavAgent.TargetPosition = target;
			TargetReached = false;
			GotoPosition = target;
			AnimationStateMachine.Call( "fire_event", "start_moving" );
		}
		protected virtual void OnTargetReached() {
			TargetReached = true;
			GotoPosition = GlobalPosition;
			Velocity = Godot.Vector2.Zero;
			AnimationStateMachine.Call( "fire_event", "stop_moving" );
		}

		public void GenerateRelations() {
			Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Thinkers" );

			for ( int i = 0; i < nodes.Count; i++ ) {
				Thinker thinker = nodes[ i ] as Thinker;

				float meetChance = 0.0f;
				if ( thinker.GetLocation() == Location ) {
					meetChance += 40.0f;
				}
			}
		}

		protected void InitBaseStats() {
			if ( !IsPremade ) {
				Godot.Collections.Array<Node> locations = GetTree().GetNodesInGroup( "Settlements" );
				BirthPlace = locations[ RNJesus.IntRange( 0, locations.Count - 1 ) ] as Settlement;
				Family = FamilyCache.GetFamily( BirthPlace, SocietyRank.Middle );
			}

			Strength = RNJesus.IntRange( 3, Family.MaxStrength ) + Family.StrengthBonus;
			Dexterity = RNJesus.IntRange( 3, Family.MaxDexterity ) + Family.DexterityBonus;
			Intelligence = RNJesus.IntRange( 3, Family.MaxIntelligence ) + Family.IntelligenceBonus;
			Wisdom = RNJesus.IntRange( 3, Family.MaxWisdom ) + Family.WisdomBonus;
			Constitution = RNJesus.IntRange( 3, Family.MaxConstitution ) + Family.ConstitutionBonus;
			Charisma = RNJesus.IntRange( 3, Family.MaxCharisma ) + Family.CharismaBonus;

			MovementSpeed += Dexterity * 10.0f;
			if ( Strength > 12 ) {
				Health += Strength * 2.0f + ( Constitution * 10.0f );
			}
		}
		protected virtual void InitRenownStats() {
		}

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
	};
};
