using Godot;
using System.Collections.Generic;
using Renown.World;
using Renown.Thinkers;
using Renown.World.Buildings;
using Renown.Thinkers.Occupations;
using System;
using System.Linq;

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
	public enum Sex : uint {
		Male,
		Female,

		Count
	};
	public enum ThinkerRequirements : uint {
		Food,
		Water,
		Entertainment,
		Money,
		Energy,
		Happiness,

		Count
	};
	public enum ThinkerAction {
		Working,
		MovingToPoint,
		BuyingGoods,

		// replenish energy
		Sleep,
		DoOpiod,
		DoSteroid,
		
		Haggle,

		None,
	};
	public enum ThinkerState : uint {
		Sleeping,
		Working,
		Eating,
		Moving,

		Count
	};
	
	// most thinkers except for politicians will most likely never get the chance nor the funds
	// to hire a personal mercenary
	public partial class Thinker : Entity {
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
		protected OccupationType Occupation;
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
		private bool HasMetPlayer = false;
		[Export]
		protected float MovementSpeed = 40.0f;

		protected Dictionary<ThinkerRequirements, int> Requirements = new Dictionary<ThinkerRequirements, int>();
//		protected Dictionary<ThinkerRequirements, float> Priorities = new Dictionary<ThinkerRequirements, float>();

		// pseudo goap
		protected float CurrentPriority = 0.0f;
		protected ThinkerRequirements CurrentFocus;
		protected Queue<ThinkerAction> ActionPlan = new Queue<ThinkerAction>();
		protected ThinkerAction CurrentAction;
	
		// small adjustments made to priority scores so that even if we're bored, we don't forget to eat food
		private static readonly Dictionary<ThinkerRequirements, float> PriorityMultiplier = new Dictionary<ThinkerRequirements, float>{
			{ ThinkerRequirements.Food, 6.0f },
			{ ThinkerRequirements.Water, 8.0f },
			{ ThinkerRequirements.Entertainment, 0.0f },
			{ ThinkerRequirements.Money, 2.0f },
			{ ThinkerRequirements.Energy, 3.5f },
			{ ThinkerRequirements.Happiness, 2.75f },
		};

		private static readonly Dictionary<ThinkerAction, System.Action<Thinker>> Actions = new Dictionary<ThinkerAction, Action<Thinker>>{
			{ ThinkerAction.MovingToPoint, MoveToPoint },
			{ ThinkerAction.Working, Work },
			{ ThinkerAction.Sleep, Sleep }
		};

		protected uint LastFoodDrainTime = 0;
		protected uint LastWaterDrainTime = 0;
		protected uint LastEntertainmentDrainTime = 0;

		protected uint SleepTimeStartHour = 0;
		protected uint WorkTimeStart = 0;
		protected uint WorkTimeEnd = 0;

		protected float CostOfLiving = 0.0f;

		protected NavigationAgent2D NavAgent;
		protected Godot.Vector2 LookDir = Godot.Vector2.Zero;

		protected Godot.Vector2 PhysicsPosition = Godot.Vector2.Zero;

		protected ThinkerFlags Flags;
		protected ThinkerState State;

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
		
		protected uint LastPayDay = 0;

		// networking
		protected NetworkWriter SyncObject = null;

		protected AudioStreamPlayer2D AudioChannel;

		private VisibleOnScreenNotifier2D VisibilityNotifier;
		protected bool OnScreen = false;

		protected Occupation Job;
		protected SocietyRank SocietyRank;

		protected System.Threading.Thread ThinkThread = null;
		protected bool Quit = false;
		
		protected int ThreadSleep = Constants.THREADSLEEP_THINKER_PLAYER_AWAY;

		[Signal]
		public delegate void HaveChildEventHandler( Thinker parent );
		[Signal]
		private delegate void ActionFinishedEventHandler();

		public ThinkerState GetState() => State;
		public SocietyRank GetSocietyRank() => SocietyRank;
		public int GetAge() => Age;
		public StringName GetFirstName() => FirstName;
		public Occupation GetOccupation() => Job;

		public void SetHome( Building building ) => Home = building;
		public Building GetHome() => Home;
		
		public void SetTileMapFloor( TileMapFloor floor ) => Floor = floor;

		public override void Damage( Entity source, float nAmount ) {
			if ( IsPremade ) {
				base.Damage( source, nAmount );
			} else {
				Job.Damage( source, nAmount );
			}
		}
		
		protected virtual void SendPacket() {
			if ( !OnScreen ) {
				return;
			}

			SyncObject.Write( GlobalPosition );
			SyncObject.Sync();
		}
		protected virtual void ReceivePacket( System.IO.BinaryReader reader ) {
			if ( !OnScreen ) {
				return;
			}

			Godot.Vector2 position;
			position.X = (float)reader.ReadDouble();
			position.Y = (float)reader.ReadDouble();
			GlobalPosition = position;
		}

		public virtual void Notify( GroupEvent nEventType, Thinker source ) {
		}
		private void FindGroup() {
		}

		protected virtual void OnPlayerEnteredArea() {
			SetProcess( true );
			SetPhysicsProcess( true );

			System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_THINKER_PLAYER_IN_AREA );
			ThinkThread.Priority = Constants.THREAD_IMPORTANCE_PLAYER_IN_AREA;

			ProcessMode = ProcessModeEnum.Pausable;

//			if ( ( Flags & ThinkerFlags.Dead ) == 0 ) {
//				AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
//			}
		}
		protected virtual void OnPlayerExitedArea() {
			SetProcess( false );
			SetPhysicsProcess( false );

			ProcessMode = ProcessModeEnum.Disabled;

			if ( Location.GetBiome().IsPlayerHere() ) {
				System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_THINKER_PLAYER_IN_BIOME );
				ThinkThread.Priority = Constants.THREAD_IMPORTANCE_PLAYER_IN_BIOME;
			} else {
				System.Threading.Interlocked.Exchange( ref ThreadSleep, Constants.THREADSLEEP_THINKER_PLAYER_AWAY );
				ThinkThread.Priority = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;
			}

//			if ( ( Flags & ThinkerFlags.Dead ) == 0 ) {
//				AudioChannel.ProcessMode = ProcessModeEnum.Disabled;
//			}
		}
		public override void SetLocation( WorldArea location ) {
			Location.PlayerEntered -= OnPlayerEnteredArea;
			Location.PlayerExited -= OnPlayerExitedArea;
			
			if ( location == null ) {
				Location = null;
				return;
			}

			Location = location;
			if ( Location is Settlement settlement && settlement != null && Home.GetLocation() == Location ) {
				settlement.AddToPopulation( this );
			}

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
			} else {
				writer.SaveString( key + nameof( Family ), Family.GetPath() );
			}

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
			writer.SaveString( key + nameof( Location ), Location.Name );
			writer.SaveBool( key + nameof( HasMetPlayer ), HasMetPlayer );

			Job.Save( writer, key );

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
		public void Load( SaveSystem.SaveSectionReader reader, int nIndex ) {
			string key = "Thinker" + nIndex;

			GlobalPosition = reader.LoadVector2( key + nameof( GlobalPosition ) );
			Health = reader.LoadFloat( key + nameof( Health ) );
			Age = reader.LoadInt( key + nameof( Age ) );
			BotName = reader.LoadString( key + nameof( BotName ) );

			if ( !IsPremade ) {
				Family = GetTree().Root.GetNode<Family>( reader.LoadString( key + nameof( Family ) ) );
			}

			Flags = (ThinkerFlags)reader.LoadUInt( key + nameof( Flags ) );
			MovementSpeed = reader.LoadFloat( key + nameof( MovementSpeed ) );
			HasMetPlayer = reader.LoadBoolean( key + nameof( HasMetPlayer ) );

			Strength = reader.LoadInt( key + nameof( Strength ) );
			Dexterity = reader.LoadInt( key + nameof( Dexterity ) );
			Wisdom = reader.LoadInt( key + nameof( Wisdom ) );
			Intelligence = reader.LoadInt( key + nameof( Intelligence ) );
			Constitution = reader.LoadInt( key + nameof( Constitution ) );

			Job.Load( reader, key );

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

		private void DrainRequirements( uint hour ) {
			if ( hour - LastFoodDrainTime >= 8 ) {
				LastFoodDrainTime = hour;
				Requirements[ ThinkerRequirements.Food ] -= 24;

				// starve
				if ( Requirements[ ThinkerRequirements.Food ] < 0 ) {
					System.Threading.Interlocked.Exchange( ref Health, Health - 10.0f );
				}
			}
			if ( State == ThinkerState.Working ) {
				if ( hour - LastEntertainmentDrainTime >= 2 ) {
					Requirements[ ThinkerRequirements.Entertainment ] -= 10;
				}
			} else {
				if ( hour - LastEntertainmentDrainTime >= 5 ) {
					Requirements[ ThinkerRequirements.Entertainment ] -= 2;
				}
			}
			/* for later
			if ( LastWaterDrainTime != hour ) {
				LastWaterDrainTime = hour;
				Requirements[ ThinkerRequirements.Water ] -= 2;

				if ( Requirements[ ThinkerRequirements.Water ] <= 0 ) {
					System.Threading.Interlocked.Exchange( ref Health, Health - 10.0f );
				}
			}
			*/
		}

		private void CreateActionPlan() {
			switch ( CurrentFocus ) {
			case ThinkerRequirements.Food: {
				if ( Location is Settlement settlement && settlement != null ) {
				} else {
					// we're fucked... kind of
					SetNavigationTarget( Settlement.Cache.FindNearest( PhysicsPosition ).GlobalPosition );
					ActionPlan.Enqueue( ThinkerAction.MovingToPoint );
					ActionPlan.Enqueue( ThinkerAction.BuyingGoods );
				}
				break; }
			case ThinkerRequirements.Energy:
				SetNavigationTarget( Home.GlobalPosition );
				ActionPlan.Enqueue( ThinkerAction.MovingToPoint );
				ActionPlan.Enqueue( ThinkerAction.Sleep );
				break;
			case ThinkerRequirements.Water:
				break;
			case ThinkerRequirements.Entertainment:
				break;
			case ThinkerRequirements.Money:
				if ( Job == null || Occupation == OccupationType.None ) {
					// TODO: implement crime
				} else {
					SetNavigationTarget( Job.GetWorkPlace().GlobalPosition );
					ActionPlan.Enqueue( ThinkerAction.MovingToPoint );
					ActionPlan.Enqueue( ThinkerAction.Working );
				}
				break;
			};
			if ( ActionPlan.Count > 0 ) {
				CurrentAction = ActionPlan.Dequeue();
			}
		}
		private void OnTimeTick( uint day, uint hour, uint minute ) {
			if ( LastPayDay != day ) {
				GetPaid();
			}

			DrainRequirements( hour );
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
			NavAgent.ProcessMode = ProcessModeEnum.Pausable;
			NavAgent.Connect( "target_reached", Callable.From( OnTargetReached ) );
			AddChild( NavAgent );

			AudioChannel = new AudioStreamPlayer2D();
			AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AudioChannel.ProcessMode = ProcessModeEnum.Pausable;
			AddChild( AudioChannel );

			if ( !IsPremade ) {
				WorldTimeManager.Instance.TimeTick += OnTimeTick;
			}

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
			}, 512*1024 );
			ThinkThread.Priority = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;
//			ThinkThread.Start();

			ActionFinished += () => {
				if ( !ActionPlan.TryDequeue( out CurrentAction ) ) {
					CurrentAction = ThinkerAction.None;
				}
			};
		}

		private static void MoveToPoint( Thinker thinker ) {
			// completion check
			if ( thinker.NavAgent.IsTargetReached() ) {
				thinker.LinearVelocity = Godot.Vector2.Zero;
				thinker.EmitSignalActionFinished();
				return;
			}

			// execution
			Godot.Vector2 nextPathPosition = thinker.NavAgent.GetNextPathPosition();
			thinker.LookDir = thinker.GlobalPosition.DirectionTo( nextPathPosition );
			thinker.LookAngle = Mathf.Atan2( thinker.LookDir.Y, thinker.LookDir.X );
			thinker.LinearVelocity = thinker.LookDir * thinker.MovementSpeed;
			thinker.State = ThinkerState.Moving;
		}
		private static void Sleep( Thinker thinker ) {
			if ( WorldTimeManager.Hour >= thinker.WorkTimeStart ) {
				thinker.EmitSignalActionFinished();
				thinker.State = ThinkerState.Working;
				return; // get up for the daily 7-8
			}
			thinker.Requirements[ ThinkerRequirements.Energy ] += 20;
			thinker.State = ThinkerState.Sleeping;
		}
		private static void Work( Thinker thinker ) {
			if ( WorldTimeManager.Hour > thinker.WorkTimeEnd ) {
				thinker.EmitSignalActionFinished();
				thinker.State = ThinkerState.Sleeping;
				return; // go back home
			}
			if ( thinker.LastEntertainmentDrainTime - WorldTimeManager.Hour >= 1 ) {
				thinker.Requirements[ ThinkerRequirements.Entertainment ] -= 10;
			}
			thinker.Job.Process();
			thinker.State = ThinkerState.Working;
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
			if ( IsPremade ) {
				CallDeferred( "MoveAlongPath" );
			}
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
				return;
			}

			base._Process( delta );

			ProcessAnimations();
			SetAnimationsColor( GameConfiguration.DemonEyeActive ? DemonEyeColor : DefaultColor );

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 || Health <= 0.0f ) {
				return;
			}

			if ( IsPremade ) {
				Think( (float)delta );
				return;
			}

			// evaluate which action has the highest priority, used as an override
			ThinkerRequirements focus = ThinkerRequirements.Count;
			float highest = float.MinValue;
			foreach( var state in Requirements ) {
				float priorityScore = state.Value * PriorityMultiplier[ state.Key ];
				if ( priorityScore > highest ) {
					highest = priorityScore;
					focus = state.Key;
				}
			}
			if ( highest > CurrentPriority ) {
				CurrentPriority = highest;
				CurrentFocus = focus;
				ActionPlan.Clear();

				CreateActionPlan();
				return;
			}

			if ( CurrentAction != ThinkerAction.None ) {
				GD.Print( "Executing" );
				Actions[ CurrentAction ].Invoke( this );
			} else {
				CreateActionPlan();
			}
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
			NavAgent.ProcessMode = ProcessModeEnum.Pausable;
		}
		protected virtual void OnTargetReached() {
			TargetReached = true;
			GotoPosition = GlobalPosition;
			LinearVelocity = Godot.Vector2.Zero;
			NavAgent.ProcessMode = ProcessModeEnum.Disabled;
		}

		public void PayTaxes( out float expectedTaxes, out float paidTaxes ) {
			paidTaxes = 0.0f;

			float taxationPercentage = Job.GetWage() / ( Home as BuildingHouse ).GetLocation().GetTaxationRate();
			expectedTaxes = taxationPercentage;

			if ( SocietyRank >= SocietyRank.Upper || Occupation == OccupationType.Politician ) {
				// if we're a little more greedy than everyone else, pay a little less taxes
				if ( HasTrait( TraitType.Greedy ) ) {

				}
			} else {
				if ( Money - expectedTaxes < 0.0f ) {
					float decreaseAmount = ( Money - expectedTaxes ) * 0.01f;
					RelationDecrease( ( Home as BuildingHouse ).GetLocation().GetGovernment(), decreaseAmount );
					// TODO: create debt
				}

				paidTaxes = expectedTaxes;
			}
		}
		public void GetPaid() {
			float amount = Job.GetWage();
			float incomeTax = amount / ( Location as Settlement ).GetTaxationRate();
			amount -= incomeTax;
			
			Job.GetCompany().PayWorker( incomeTax, amount, this );
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
			
			Thinker thinker = ResourceLoader.Load<PackedScene>( "res://scenes/renown/thinker.tscn" ).Instantiate<Thinker>();
			System.Random random = new System.Random();
			
			thinker.Location = location;
			thinker.BirthPlace = location;
			thinker.Age = specificAge == -1 ? random.Next( 0, 70 ) : specificAge;
			thinker.Family = ThinkerCache.GetFamily( location );
			thinker.FirstName = NameGenerator.GenerateName();
			thinker.BotName = string.Format( "{0} {1}", thinker.FirstName, thinker.Family.Name );
			thinker.SocietyRank = thinker.Family.GetSocietyRank();

			System.Span<int> jobChances = stackalloc int[ (int)OccupationType.Count ];
			for ( OccupationType occupation = OccupationType.None; occupation < OccupationType.Count; occupation++ ) {
				jobChances[ (int)occupation ] = Constants.JobChances_SocioEconomicStatus[ occupation ][ thinker.SocietyRank ];
			}

			for ( ThinkerRequirements requirements = ThinkerRequirements.Food; requirements < ThinkerRequirements.Count; requirements++ ) {
				thinker.Requirements.Add( requirements, 100 );
			}

			Godot.Collections.Array<Node> factions = location.GetTree().GetNodesInGroup( "Factions" );
			thinker.Family.GetHome().GetLocation().GetGovernment().MemberJoin( thinker );

			Building workplace;
			thinker.Occupation = thinker.Family.GetHome().GetLocation().CalcJob( random, thinker, out workplace );
			thinker.Job = Thinkers.Occupation.Create( thinker.Occupation, thinker, thinker.Faction );
			if ( thinker.Occupation == OccupationType.Merchant ) {
				MarketplaceSlot slot = null;

				//FIXME: THIS
				for ( int i = 0; i < thinker.Family.GetHome().GetLocation().GetMarketplaces().Length; i++ ) {
					slot = thinker.Family.GetHome().GetLocation().GetMarketplaces()[i].GetFreeTradingSpace();
					if ( slot != null ) {
						( thinker.GetOccupation() as Merchant ).SetTradingSlot( slot );
						break;
					}
				}
				if ( slot == null ) {
					// just be a merc
					thinker.Occupation = OccupationType.Mercenary;
					thinker.Job = Thinkers.Occupation.Create( thinker.Occupation, thinker, thinker.Faction );
				}
			}
			thinker.Job.SetWorkPlace( workplace );

			thinker.Name = string.Format( "{0}{1}{2}{3}{4}", thinker.FirstName, thinker.Family.Name, thinker.BirthPlace.Name, thinker.Age, thinker.GetHashCode() );

			thinker.Home = thinker.Family.GetHome();
			thinker.Family.AddMember( thinker );

			switch ( thinker.SocietyRank ) {
			case SocietyRank.Lower:
				thinker.CostOfLiving = 800.0f;
				thinker.Money = new RandomNumberGenerator().RandfRange( 200, 1000 );
				thinker.WorkTimeStart = 7;
				thinker.WorkTimeEnd = 20;
				break;
			case SocietyRank.Middle:
				thinker.CostOfLiving = 16000.0f;
				thinker.Money = new RandomNumberGenerator().RandfRange( 4000, 25000 );
				break;
			case SocietyRank.Upper:
				thinker.CostOfLiving = 60000.0f;
				thinker.Money = new RandomNumberGenerator().RandfRange( 30000, 120000 );
				break;
			};

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
			thinker.Strength = random.Next( 3, thinker.Family.MaxStrength )
				+ thinker.Family.StrengthBonus;
			
			thinker.Constitution = random.Next( 3, thinker.Family.MaxConstitution )
				+ thinker.Family.ConstitutionBonus;
			
			thinker.Intelligence = random.Next( 3, thinker.Family.MaxIntelligence )
				+ thinker.Family.IntelligenceBonus;
			
			thinker.Wisdom = random.Next( 3, thinker.Family.MaxWisdom )
				+ thinker.Family.WisdomBonus;
			
			thinker.Dexterity = random.Next( 3, thinker.Family.MaxDexterity )
				+ thinker.Family.DexterityBonus;
			
			thinker.Charisma = random.Next( 3, thinker.Family.MaxCharisma )
				+ thinker.Family.CharismaBonus;
			
			thinker.MovementSpeed += thinker.Dexterity * 10.0f;
			thinker.Health += thinker.Strength * 2.0f + ( thinker.Constitution * 10.0f );

			GD.Print( "Thinker " + thinker.Name + " Generated:" );
			GD.Print( "\t[Renown Data]" );
			GD.Print( "\t\tLocation: " + thinker.Location.GetAreaName() );
			GD.Print( "\t\tBirthPlace: " + thinker.BirthPlace.GetAreaName() );
			GD.Print( "\t\tAge: " + thinker.Age );
			GD.Print( "\t\tRenown Score: " + thinker.RenownScore );
			GD.Print( "\t\tMoney: " + thinker.Money );
			GD.Print( "\t\tWage: " + thinker.Job.GetWage() );

			if ( thinker.Faction != null ) {
				GD.Print( "\t\tFaction: " + thinker.Faction.GetFactionName() );
			} else {
				GD.Print( "\t\tFaction: None" );
			}
			GD.Print( "\t\tOccupation: " + thinker.Occupation.ToString() );
			GD.Print( "\t\tHome: " + thinker.Home.GetPath() );
			GD.Print( "\tStats:" );
			GD.Print( "\t\tStrength: " + thinker.Strength );
			GD.Print( "\t\tDexterity: " + thinker.Dexterity );
			GD.Print( "\t\tIntelligence: " + thinker.Intelligence );
			GD.Print( "\t\tWisdom: " + thinker.Wisdom );
			GD.Print( "\t\tConstitution: " + thinker.Constitution );

			thinker.CallDeferred( "set_process", true );
			thinker.CallDeferred( "set_physics_process", true );
			thinker.GetNode<CollisionShape2D>( "BodyCollision" ).SetDeferred( "disabled", true );

			thinker.ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			thinker.ProcessThreadGroupOrder = Constants.THREAD_GROUP_THINKERS;

			ThinkerCache.AddThinker( thinker );
	
			return thinker;
		}

		private class NameGenerator {
			private static System.Random random = new System.Random();

			private static readonly string[] FirstNameScramble_Begin = [
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
			private static readonly string[] FirstNameScramble_End = [
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
