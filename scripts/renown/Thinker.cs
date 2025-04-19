using Godot;
using System.Collections.Generic;
using Renown.World;
using Renown.Thinkers;

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
		public static DataCache<Thinker> Cache = null;

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
		
		[Export]
		protected StringName BotName;
		[Export]
		protected Occupation Job;
		[Export]
		protected uint Age = 0; // in years
		[Export]
		protected FamilyTree FamilyTree;
		[Export]
		protected Settlement BirthPlace = null;

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
		protected System.Threading.ThreadPriority Importance = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;

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

			ThreadSleep = Constants.THREADSLEEP_PLAYER_IN_AREA;
			Importance = Constants.THREAD_IMPORTANCE_PLAYER_IN_AREA;

			ThinkThread.Priority = Importance;
			AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
		}
		private void OnPlayerExitedArea() {
			SetProcess( false );

			if ( Location.GetBiome().IsPlayerHere() ) {
				ThreadSleep = Constants.THREADSLEEP_PLAYER_IN_BIOME;
				Importance = Constants.THREAD_IMPORTANCE_PLAYER_IN_BIOME;
			} else {
				ThreadSleep = Constants.THREADSLEEP_PLAYER_AWAY;
				Importance = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;
			}

			ThinkThread.Priority = Importance;
			AudioChannel.ProcessMode = ProcessModeEnum.Disabled;
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

		public override StringName GetObjectName() => BotName;

		public override void Save() {
			using ( var writer = new SaveSystem.SaveSectionWriter( "Thinker_" + BotName + FamilyTree.Name ) ) {
				writer.SaveVector2( "Position", GlobalPosition );
				writer.SaveFloat( nameof( Health ), Health );
				writer.SaveUInt( nameof( Age ), Age );

				// stats
				writer.SaveInt( nameof( Strength ), Strength );
				writer.SaveInt( nameof( Dexterity ), Dexterity );
				writer.SaveInt( nameof( Wisdom ), Wisdom );
				writer.SaveInt( nameof( Intelligence ), Intelligence );
				writer.SaveInt( nameof( Constitution ), Constitution );

				writer.SaveUInt( nameof( Flags ), (uint)Flags );
				writer.SaveFloat( nameof( MovementSpeed ), MovementSpeed );
				writer.SaveUInt( nameof( Job ), (uint)Job );
			}
		}
		public override void Load() {
			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( "Thinker_" + BotName + FamilyTree.Name );

			// save file compability
			if ( reader == null ) {
				return;
			}

			SetDeferred( "global_position", reader.LoadVector2( "Position" ) );
			SetDeferred( "health", reader.LoadFloat( "Health" ) );
			SetDeferred( "age", reader.LoadUInt( "Age" ) );
		}

		protected virtual void SetAnimationsColor( Color color ) {
		}

		protected virtual void OnScreenEnter() {
			OnScreen = true;
		}
		protected virtual void OnScreenExit() {
			OnScreen = false;
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

			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnRigidBody2DShapeEntered ) );

			GotoPosition = GlobalPosition;
			
			if ( SettingsData.GetNetworkingEnabled() ) {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, SendPacket, ReceivePacket ) );
			}
			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( !IsInGroup( "Thinkers" ) ) {
				AddToGroup( "Thinkers" );
			}

			ProcessMode = ProcessModeEnum.Pausable;

			Location.PlayerEntered += OnPlayerEnteredArea;
			Location.PlayerExited += OnPlayerExitedArea;

			ThinkThread = new System.Threading.Thread( () => {
				while ( !Quit ) {
					System.Threading.Thread.Sleep( ThreadSleep );
					if ( Health <= 0.0f ) {
						continue;
					}
					RenownProcess();
				}
			} );
			ThinkThread.Priority = Importance;
			ThinkThread.Start();

			SetProcess( false );
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
		
		public static Thinker Create( Settlement location ) {
			// TODO: create outliers, special bots
			
			Thinker thinker = new Thinker();
			System.Random random = new System.Random();
			
			Godot.Collections.Array<FamilyTree> families = location.GetFamilyTrees();
			
			thinker.Location = location;
			thinker.BirthPlace = location;
			thinker.FamilyTree = families[ random.Next( 0, families.Count - 1 ) ] as FamilyTree;
			thinker.Age = 0;
//			thinker.BotName = string.Format( "{0} {1}", location.CreateBotName(), thinker.FamilyTree.Name );
			
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

			location.CallDeferred( "AddThinker", thinker );
			
			return thinker;
		}
	};
};
