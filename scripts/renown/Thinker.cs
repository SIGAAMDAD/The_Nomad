using Godot;
using System.Collections.Generic;
using Renown.World;
using Renown.Thinkers;
using Renown.World.Buildings;

namespace Renown {
	public enum ThinkerFlags : uint {
		// physics slapped
		Pushed		= 0x00000001,
		
		// ...duh
		Dead		= 0x00000002,
		
		Intoxicated	= 0x00000004,
		Pregnant	= 0x00000008,
		Sterile		= 0x00000010,
	};
	
	// most thinkers except for politicians will most likely never get the chance nor the funds
	// to hire a personal mercenary
	public partial class Thinker : Entity {
		public enum Occupation : uint {
			None,

			Bandit,
			
			Blacksmith,
			Merchant,
			Gunsmith,
			
			// DNA splicer
			Splicer,
			
			MercenaryMaster,
			Mercenary,
			Politician,
			
			Count
		};

		[Export]
		protected TileMapFloor Floor;
		
		private NodePath InitialPath;
		
		[Export]
		private bool IsPremade = false;
		[Export]
		protected Building Home;
		[Export]
		protected StringName BotName;
		[Export]
		protected Occupation Job;
		[Export]
		protected int Age = 0; // in years
		[Export]
		protected FamilyTree FamilyTree;
		[Export]
		protected Settlement BirthPlace = null;

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
		private bool HasMetPlayer = false;
		[Export]
		protected float MovementSpeed = 40.0f;
		
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

		protected Timer MoveTimer = null;

		// networking
		protected NetworkWriter SyncObject = null;

		protected AudioStreamPlayer2D AudioChannel;

		protected uint Pay = 0;

		private VisibleOnScreenNotifier2D VisibilityNotifier;
		protected bool OnScreen = false;

		protected System.Threading.Thread ThinkThread = null;
		protected bool Quit = false;
		protected int ThreadSleep = Constants.THREADSLEEP_PLAYER_AWAY;

		public int GetAge() => Age;
		public StringName GetFirstName() => FirstName;

		public void SetOccupation( Occupation job ) => Job = job;
		public Occupation GetOccupation() => Job;
		
		public void SetTileMapFloor( TileMapFloor floor ) => Floor = floor;
		
		protected virtual void SendPacket() {
			if ( !OnScreen ) {
				return;
			}
		}
		protected virtual void ReceivePacket( System.IO.BinaryReader reader ) {
			if ( !OnScreen ) {

			}
		}

		public virtual void Notify( GroupEvent nEventType, Thinker source ) {
		}
		private void FindGroup() {
		}

		private void OnPlayerEnteredArea() {
			SetProcess( true );
			SetPhysicsProcess( true );

			System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_PLAYER_IN_AREA );
			ThinkThread.Priority = Constants.THREAD_IMPORTANCE_PLAYER_IN_AREA;

			ProcessMode = ProcessModeEnum.Pausable;

			if ( !IsInsideTree() ) {
//				GetTree().CurrentScene.GetNode( "Thinkers" ).CallDeferred( "add_child", this );
			}

			if ( ( Flags & ThinkerFlags.Dead ) == 0 ) {
				AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
			}
		}
		private void OnPlayerExitedArea() {
			SetProcess( false );
			SetPhysicsProcess( false );

			ProcessMode = ProcessModeEnum.Disabled;

//			GetTree().CurrentScene.GetNode( "Thinkers" ).CallDeferred( "remove_child", this );

			if ( Location.GetBiome().IsPlayerHere() ) {
				System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_PLAYER_IN_BIOME );
				ThinkThread.Priority = Constants.THREAD_IMPORTANCE_PLAYER_IN_BIOME;
			} else {
				System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_PLAYER_AWAY );
				ThinkThread.Priority = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;
			}

			if ( ( Flags & ThinkerFlags.Dead ) == 0 ) {
				AudioChannel.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
		public override void SetLocation( WorldArea location ) {
			Location.PlayerEntered -= OnPlayerEnteredArea;
			Location.PlayerExited -= OnPlayerExitedArea;

			Location = location;

			Location.PlayerEntered += OnPlayerEnteredArea;
			Location.PlayerExited += OnPlayerExitedArea;
		}

		protected virtual void OnRigidBody2DShapeEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Entity entity && entity != null ) {
				// momentum combat
				float speed = entity.LinearVelocity.Length();
				if ( speed >= Constants.DAMAGE_VELOCITY ) {
					Damage( entity, speed );
					LinearVelocity = entity.LinearVelocity * 2.0f;
					Flags |= ThinkerFlags.Pushed;
				}
			}
		}

		public override StringName GetObjectName() => Name;

		public void Save( SaveSystem.SaveSectionWriter writer, int nIndex ) {
			string key = "Thinker" + nIndex;
			int count;

			writer.SaveBool( key + nameof( IsPremade ), IsPremade );
			if ( IsPremade ) {
				writer.SaveString( key + nameof( InitialPath ), InitialPath );
			}

			writer.SaveVector2( key + nameof( GlobalPosition ), GlobalPosition );
			writer.SaveFloat( key + nameof( Health ), Health );
			writer.SaveInt( key + nameof( Age ), Age );

			// stats
			writer.SaveInt( key + nameof( Strength ), Strength );
			writer.SaveInt( key + nameof( Dexterity ), Dexterity );
			writer.SaveInt( key + nameof( Wisdom ), Wisdom );
			writer.SaveInt( key + nameof( Intelligence ), Intelligence );
			writer.SaveInt( key + nameof( Constitution ), Constitution );

			writer.SaveUInt( key + nameof( Flags ), (uint)Flags );
			writer.SaveFloat( key + nameof( MovementSpeed ), MovementSpeed );
			writer.SaveUInt( key + nameof( Job ), (uint)Job );
			writer.SaveString( key + nameof( Location ), Location.Name );
			writer.SaveBool( key + nameof( HasMetPlayer ), HasMetPlayer );

			writer.SaveInt( key + "RelationCount", RelationCache.Count );
			count = 0;
			foreach ( var relation in RelationCache ) {
				writer.SaveString( string.Format( "{0}RelationNode{1}", key, count ), relation.Key.GetObjectName() );
				writer.SaveFloat( string.Format( "{0}RelationValue{1}", key, count ), relation.Value );
				count++;
			}

			writer.SaveInt( key + "TraitCount", TraitCache.Count );
			count = 0;
			foreach ( var trait in TraitCache ) {
				writer.SaveString( string.Format( "{0}TraitName{1}", key, count ), trait.GetTraitName() );
				writer.SaveUInt( string.Format( "{0}TraitType{1}", key, count ), (uint)trait.GetTraitType() );
				count++;
			}
		}
		public void Load( SaveSystem.SaveSectionReader reader, int nIndex ) {
			string key = "Thinker" + nIndex;

			GlobalPosition = reader.LoadVector2( key + nameof( GlobalPosition ) );
			Health = reader.LoadFloat( key + nameof( Health ) );
			Age = reader.LoadInt( key + nameof( Age ) );

			Flags = (ThinkerFlags)reader.LoadUInt( key + nameof( Flags ) );
			MovementSpeed = reader.LoadFloat( key + nameof( MovementSpeed ) );
			Job = (Occupation)reader.LoadUInt( key + nameof( Job ) );
			HasMetPlayer = reader.LoadBoolean( key + nameof( HasMetPlayer ) );

			Strength = reader.LoadInt( key + nameof( Strength ) );
			Dexterity = reader.LoadInt( key + nameof( Dexterity ) );
			Wisdom = reader.LoadInt( key + nameof( Wisdom ) );
			Intelligence = reader.LoadInt( key + nameof( Intelligence ) );
		}

		protected virtual void SetAnimationsColor( Color color ) {
		}

		protected virtual void OnScreenEnter() {
			OnScreen = true;
		}
		protected virtual void OnScreenExit() {
			OnScreen = false;
		}

		public Thinker() {
			if ( !IsInGroup( "Thinkers" ) ) {
				AddToGroup( "Thinkers" );
			}
		}
		public override void _ExitTree() {
			base._ExitTree();

			Quit = true;
		}
		public override void _Ready() {
			base._Ready();

			NavAgent = new NavigationAgent2D();
			NavAgent.PathMaxDistance = 10000.0f;
			NavAgent.AvoidanceEnabled = true;
			NavAgent.Radius = 60.0f;
			NavAgent.MaxNeighbors = 20;
			NavAgent.MaxSpeed = 200.0f;
			NavAgent.Connect( "target_reached", Callable.From( OnTargetReached ) );
			AddChild( NavAgent );

			AudioChannel = new AudioStreamPlayer2D();
			AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AudioChannel.ProcessMode = ProcessModeEnum.Disabled;
			AddChild( AudioChannel );

			/*
			VisibilityNotifier = GetNode<VisibleOnScreenNotifier2D>( "VisibleOnScreenNotifier2D" );
			VisibilityNotifier.Connect( "screen_entered", Callable.From( OnScreenEnter ) );
			VisibilityNotifier.Connect( "screen_exited", Callable.From( OnScreenExit ) );
			AddChild( VisibilityNotifier );
			*/

			if ( IsPremade ) {
				InitialPath = GetPath();
			}

			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnRigidBody2DShapeEntered ) );
			GotoPosition = GlobalPosition;
			
			if ( SettingsData.GetNetworkingEnabled() ) {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, SendPacket, ReceivePacket ) );
			}

			ProcessMode = ProcessModeEnum.Pausable;
			
			Location.PlayerEntered += OnPlayerEnteredArea;
			Location.PlayerExited += OnPlayerExitedArea;

			ThinkThread = new System.Threading.Thread( () => {
				while ( !Quit ) {
					System.Threading.Thread.Sleep( ThreadSleep );
					if ( ( Flags & ThinkerFlags.Dead ) != 0 ) {
						continue;
					}
					RenownProcess();
				}
			} );
			ThinkThread.Priority = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;
			ThinkThread.Start();
		}

        public override void _PhysicsProcess( double delta ) {
			if ( Health <= 0.0f ) {
				return;
			}

			base._PhysicsProcess( delta );

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 ) {
				if ( LinearVelocity == Godot.Vector2.Zero ) {
					Flags &= ~ThinkerFlags.Pushed;
					PhysicsMaterialOverride.Friction = 1.0f;
				} else {
					return;
				}
			}
			if ( Location.IsPlayerHere() ) {
				GlobalRotation = 0.0f;
				if ( MoveTimer != null ) {
					if ( MoveTimer.IsStopped() && LinearVelocity != Godot.Vector2.Zero ) {
						MoveTimer.Start();
					}
				}
			}

			MoveAlongPath();
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % 20 ) != 0 ) {
				return;
			}

			base._Process( delta );

			ProcessAnimations();
			SetAnimationsColor( GameConfiguration.DemonEyeActive ? DemonEyeColor : DefaultColor );

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 || Health <= 0.0f ) {
				return;
			}

			Think( (float)delta );
		}

		/// <summary>
		/// Runs more specific and detailed interactions when the player is in the area.
		/// </summary>
		/// <param name="delta"></param>
		protected virtual void Think( float delta ) {
		}
		
		/// <summary>
		/// Processes various relations, debts, etc. to run the renown system per entity.
		/// expensive, but not animations and very little godot engine calls will be present.
		/// </summary>
		protected virtual void RenownProcess() {
		}
		protected virtual void ProcessAnimations() {
		}
		
		protected void HaveChild() {
		}

		protected bool MoveAlongPath() {
			if ( NavAgent.IsTargetReached() ) {
				LinearVelocity = Godot.Vector2.Zero;
				return true;
			}
			Godot.Vector2 nextPathPosition = NavAgent.GetNextPathPosition();
			LookDir = GlobalPosition.DirectionTo( nextPathPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			LinearVelocity = LookDir * MovementSpeed;
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
			LinearVelocity = Godot.Vector2.Zero;
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
		public static Thinker Create( Settlement location, int specificAge ) {
			// TODO: create outliers, special bots
			
			Thinker thinker = new Thinker();
			System.Random random = new System.Random();
			
			Godot.Collections.Array<Node> families = location.GetTree().GetNodesInGroup( "Families" );
			
			thinker.Location = location;
			thinker.BirthPlace = location;
			thinker.Age = specificAge == -1 ? random.Next( 0, 70 ) : specificAge;
			thinker.FamilyTree = families[ random.Next( 0, families.Count - 1 ) ] as FamilyTree;
			thinker.FirstName = location.NameCache[ random.Next( 0, location.NameCache.Length - 1 ) ];
			thinker.BotName = string.Format( "{0} {1}", thinker.FirstName, thinker.FamilyTree.Name );
			thinker.Name = string.Format( "{0}{1}{2}{3}", thinker.FirstName, thinker.FamilyTree.Name, thinker.BirthPlace.Name, thinker.Age );
			thinker.FamilyTree.AddMember( thinker );

			//
			// generate traits, relations, and debts
			//

			int traitCount = random.Next( 2, (int)TraitType.Count - 1 );
			thinker.Traits = new Godot.Collections.Array<TraitType>();
			thinker.Traits.Resize( traitCount );

			for ( int i = 0; i < traitCount; i++ ) {
				Trait trait = null;

				while ( trait == null ) {
					TraitType proposed = (TraitType)random.Next( 0, (int)TraitType.Count - 1 );
					switch ( proposed ) {
					case TraitType.Cruel:
						trait = new Traits.Cruel();
						break;
					case TraitType.Greedy:
						trait = new Traits.Greedy();
						break;
					case TraitType.Honorable:
						trait = new Traits.Honorable();
						break;
					case TraitType.Reliable:
						trait = new Traits.Reliable();
						break;
					case TraitType.Merciful:
						trait = new Traits.Merciful();
						break;
					case TraitType.Liar:
						trait = new Traits.Liar();
						break;
					case TraitType.WarCriminal:
						// cant really have an infant being a war criminal
						if ( thinker.Age < 10 ) {
							continue;
						}
						trait = new Traits.WarCriminal();
						break;
					};

					// check for conflicting traits
					for ( int t = 0; t < thinker.Traits.Count; t++ ) {
						if ( trait.Conflicts( thinker.Traits[i] ) ) {
							// trash it, try again
							trait = null;
							break;
						}
					}
				}
			}
			
			// now we pull a D&D
			// TODO: incorporate evolution over millions of years
			thinker.Strength = random.Next( 3, thinker.FamilyTree.GetStrengthMax() )
				+ thinker.FamilyTree.GetStrengthBonus();
			
			thinker.Constitution = random.Next( 3, thinker.FamilyTree.GetConstitutionMax() )
				+ thinker.FamilyTree.GetConstitutionBonus();
			
			thinker.Intelligence = random.Next( 3, thinker.FamilyTree.GetIntelligenceMax() )
				+ thinker.FamilyTree.GetIntelligenceBonus();
			
			thinker.Wisdom = random.Next( 3, thinker.FamilyTree.GetWisdomMax() )
				+ thinker.FamilyTree.GetWisdomBonus();
			
			thinker.Dexterity = random.Next( 3, thinker.FamilyTree.GetDexterityMax() )
				+ thinker.FamilyTree.GetDexterityBonus();
			
			thinker.MovementSpeed += thinker.Dexterity * 10.0f;
			thinker.Health += thinker.Strength * 2.0f + ( thinker.Constitution * 10.0f );

			thinker.ProcessMode = ProcessModeEnum.Disabled;

			ThinkerCache.AddThinker( thinker );
	
			return thinker;
		}
	};
};
