using Godot;
using System.Collections.Generic;
using Renown.World;
using Renown.World.Buildings;
using System;
using ChallengeMode;

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
	
	// most thinkers except for politicians will most likely never get the chance nor the funds
	// to hire a personal mercenary
	public partial class Thinker : Entity {
		protected Random Random = new Random();

		protected AnimatedSprite2D BodyAnimations;

		[Export]
		protected TileMapFloor Floor;
		
		protected NodePath InitialPath;
		
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

		protected StringName FirstName;

		[ExportCategory("Start")]
		[Export]
		protected DirType Direction;
		
		[ExportCategory("Stats")]
		
		/// <summary>
		/// physical power
		/// </summary>
		[Export]
		protected int Strength;
		
		/// <summary>
		/// manuverability, reflexes
		/// </summary>
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
		protected float MovementSpeed = 200.0f;

		protected Node2D Animations;

		protected NavigationAgent2D NavAgent;
		protected Godot.Vector2 LookDir = Godot.Vector2.Zero;

		protected Godot.Vector2 PhysicsPosition = Godot.Vector2.Zero;

		protected ThinkerFlags Flags;
		protected ThinkerGroup Squad;

		protected bool Initialized = false;

		// memory
		protected bool TargetReached = false;
		protected Godot.Vector2 GotoPosition = Godot.Vector2.Zero;
		protected float LookAngle = 0.0f;
		protected float AimAngle = 0.0f;

		protected static readonly Color DefaultColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		protected Color DemonEyeColor;

		// networking
		protected NetworkWriter SyncObject = null;

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

		public virtual void Alert( Entity source ) {
		}

		public override void Damage( Entity source, float nAmount ) {
			BloodParticleFactory.CreateDeferred( source.GlobalPosition, GlobalPosition );
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
		public override void SetLocation( WorldArea location ) {
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
			channel ??= AudioChannel;
			channel.SetDeferred( "stream", stream );
			channel.CallDeferred( "play" );
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
				if ( relation.Key is Entity entity && entity != null ) {
					writer.SaveBool( string.Format( "{0}RelationIsEntity{1}", key, count ), true );
				} else {
					writer.SaveBool( string.Format( "{0}RelationIsEntity{1}", key, count ), false );
				}
				writer.SaveString( string.Format( "{0}RelationNode{1}", key, count ), relation.Key.GetObjectName() );
				writer.SaveFloat( string.Format( "{0}RelationValue{1}", key, count ), relation.Value );
				count++;
			}

			writer.SaveInt( key + "DebtCount", DebtCache.Count );
			count = 0;
			foreach ( var debt in DebtCache ) {
				writer.SaveString( string.Format( "{0}DebtNode{1}", key, count ), debt.Key.GetObjectName() );
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
			RelationCache = new Dictionary<Object, float>( relationCount );
			for ( int i = 0; i < relationCount; i++ ) {
				RelationCache.Add(
					(Object)GetTree().Root.GetNode( reader.LoadString( string.Format( "{0}RelationNode{1}", key, i ) ) ),
					reader.LoadFloat( string.Format( "{0}RelationValue{1}", key, i ) )
				);
			}

			int debtCount = reader.LoadInt( key + "DebtCount" );
			DebtCache = new Dictionary<Object, float>( debtCount );
			for ( int i = 0; i < debtCount; i++ ) {
				DebtCache.Add(
					(Object)GetTree().Root.GetNode( reader.LoadString( string.Format( "{0}DebtNode{1}", key, i ) ) ),
					reader.LoadFloat( string.Format( "{0}DebtValue{1}", key, i ) )
				);
			}

			int traitCount = reader.LoadInt( key + "TraitCount" );
			TraitCache = new HashSet<Trait>( traitCount );
			for ( int i = 0; i < traitCount; i++ ) {
				TraitCache.Add( Trait.Create( (TraitType)reader.LoadUInt( string.Format( "{0}TraitType{1}", key, i ) ) ) );
			}
		}

		public virtual void StopMoving() {
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
				PlaySound( AudioChannel, ResourceCache.MoveGravelSfx[ Random.Next( 0, ResourceCache.MoveGravelSfx.Length - 1 ) ] );
			}
		}

		private void OnDie( Entity source, Entity target ) {
			if ( GameConfiguration.GameMode == GameMode.ChallengeMode ) {
				if ( IsInGroup( "Enemies" ) ) {
					RemoveFromGroup( "Enemies" );
					ChallengeLevel.SetObjectiveState( "AllEnemiesDead", GetTree().GetNodesInGroup( "Enemies" ).Count == 0 );
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
			if ( GameConfiguration.GameMode != GameMode.ChallengeMode ) {
				InitRenownStats();
			}

			Die += OnDie;

			Animations = GetNode<Node2D>( "Animations" );
			BodyAnimations = GetNode<AnimatedSprite2D>( "Animations/BodyAnimations" );

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
			NavAgent.Radius = 60.0f;
			NavAgent.MaxNeighbors = 20;
			NavAgent.MaxSpeed = 200.0f;
			NavAgent.ProcessMode = ProcessModeEnum.Pausable;
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

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 ) {
				if ( Velocity == Godot.Vector2.Zero ) {
					Flags &= ~ThinkerFlags.Pushed;
				} else {
					return;
				}
			}
			CallDeferred( "MoveAlongPath" );
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % (ulong)ThreadSleep ) != 0 ) {
				return;
			}

			base._Process( delta );

			SetAnimationsColor( GameConfiguration.DemonEyeActive ? DemonEyeColor : DefaultColor );
			ProcessAnimations();

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 || Health <= 0.0f ) {
				return;
			}

			RenownProcess();
			Think();
		}

		protected void CheckContracts() {
			foreach ( var relation in RelationCache ) {
				if ( GetRelationStatus( relation.Key ) > RelationStatus.Dislikes ) {
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
			if ( NavAgent.IsTargetReached() ) {
				Velocity = Godot.Vector2.Zero;
				return true;
			}
			Godot.Vector2 nextPathPosition = NavAgent.GetNextPathPosition();
			LookDir = GlobalPosition.DirectionTo( nextPathPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			Velocity = LookDir * MovementSpeed;
			GlobalPosition += Velocity * (float)GetPhysicsProcessDeltaTime();
			return true;
		}
		protected virtual void SetNavigationTarget( Godot.Vector2 target ) {
			NavAgent.TargetPosition = target;
			TargetReached = false;
			GotoPosition = target;
		}
		protected virtual void OnTargetReached() {
			TargetReached = true;
			GotoPosition = GlobalPosition;
			Velocity = Godot.Vector2.Zero;
		}
		
		public void GenerateRelations() {
			Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Thinkers" );

			for ( int i = 0; i < nodes.Count; i++ ) {
				Thinker thinker = nodes[i] as Thinker;

				float meetChance = 0.0f;
				if ( thinker.GetLocation() == Location ) {
					meetChance += 40.0f;
				}
			}
		}

		protected void InitBaseStats() {
			if ( !IsPremade ) {
				Godot.Collections.Array<Node> locations = GetTree().GetNodesInGroup( "Settlements" );
				BirthPlace = locations[ Random.Next( 0, locations.Count - 1 ) ] as Settlement;
				Family = FamilyCache.GetFamily( BirthPlace, SocietyRank.Middle );
			}

			Strength = Random.Next( 3, Family.MaxStrength ) + Family.StrengthBonus;
			Dexterity = Random.Next( 3, Family.MaxDexterity ) + Family.DexterityBonus;
			Intelligence = Random.Next( 3, Family.MaxIntelligence ) + Family.IntelligenceBonus;
			Wisdom = Random.Next( 3, Family.MaxWisdom ) + Family.WisdomBonus;
			Constitution = Random.Next( 3, Family.MaxConstitution ) + Family.ConstitutionBonus;
			Charisma = Random.Next( 3, Family.MaxCharisma ) + Family.CharismaBonus;

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
				return string.Format( "{0}{1}", begin[ random.Next( 0, begin.Length - 1 ) ], end[ random.Next( 0, end.Length - 1 ) ] );
			}
			public static string GenerateName() {
				return NameScramble( FirstNameScramble_Begin, FirstNameScramble_End );
			}
		};
	};
};
