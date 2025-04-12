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
		private float MovementSpeed = 40.0f;
		
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
		
		// called when entering a shop
//		public void SetCurrentShop( Shop shop ) {
//			Agent.State[ "Vendor" ] = shop;
//		}
		// called when entering a settlement
		public void SetTileMapFloor( TileMapFloor floor ) => Floor = floor;
		
		protected virtual void SendPacket() {
		}
		protected virtual void ReceivePacket( System.IO.BinaryReader reader ) {
		}

		public virtual void Notify( GroupEvent nEventType, Thinker source ) {
		}
		private void FindGroup() {
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
			SaveSystem.SaveSectionWriter writer = new SaveSystem.SaveSectionWriter( "Thinker_" + GetPath() );

			writer.SaveVector2( "position", GlobalPosition );
			writer.SaveFloat( "health", Health );
			writer.SaveUInt( "age", Age );
			writer.Flush();
		}
		public override void Load() {
			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( "Thinker_" + GetPath() );

			// save file compability
			if ( reader == null ) {
				return;
			}

			SetDeferred( "global_position", reader.LoadVector2( "position" ) );
			SetDeferred( "health", reader.LoadFloat( "health" ) );
			SetDeferred( "age", reader.LoadUInt( "age" ) );
		}

		protected virtual void SetAnimationsColor( Color color ) {
		}

		protected virtual void OnScreenEnter() {
			OnScreen = true;
		}
		protected virtual void OnScreenExit() {
			OnScreen = false;
		}

		public override void _Ready() {
			base._Ready();

			NavAgent = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
			NavAgent.Connect( "target_reached", Callable.From( OnTargetReached ) );

			AudioChannel = new AudioStreamPlayer2D();
			AudioChannel.VolumeDb = Mathf.LinearToDb( 100.0f / SettingsData.GetEffectsVolume() );
			AddChild( AudioChannel );

			VisibilityNotifier = GetNode<VisibleOnScreenNotifier2D>( "VisibleOnScreenNotifier2D" );
			VisibilityNotifier.Connect( "screen_entered", Callable.From( OnScreenEnter ) );
			VisibilityNotifier.Connect( "screen_exited", Callable.From( OnScreenExit ) );

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
		}

        public override void _PhysicsProcess( double delta ) {
			base._PhysicsProcess( delta );
		
			PhysicsPosition = GlobalPosition;

			if ( ( Flags & ThinkerFlags.Pushed ) != 0 ) {
				if ( LinearVelocity == Godot.Vector2.Zero ) {
					Flags &= ~ThinkerFlags.Pushed;
					PhysicsMaterialOverride.Friction = 1.0f;
				} else {
					return;
				}
			}
			GlobalRotation = 0.0f;
			if ( MoveTimer != null ) {
				if ( MoveTimer.IsStopped() && LinearVelocity != Godot.Vector2.Zero ) {
					MoveTimer.Start();
				}
			}
			MoveAlongPath();
		}
		public override void _Process( double delta ) {
			base._Process( delta );
			if ( ( Flags & ThinkerFlags.Pushed ) != 0 || ( Engine.GetProcessFrames() % 30 ) != 0 ) {
				return;
			}

			SetAnimationsColor( GameConfiguration.DemonEyeActive ? DemonEyeColor : DefaultColor );
			ProcessAnimations();

			Think( (float)delta );
		}
		protected virtual void Think( float delta ) {
		}
		protected virtual void ProcessAnimations() {
		}
		
		protected void HaveChild() {
		}

		protected bool MoveAlongPath() {
			if ( GlobalPosition.DistanceTo( GotoPosition ) <= 10.0f ) {
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
		
		/*
		public static Thinker Create( Settlement location ) {
			// TODO: create outliers, special bots
			
			Thinker thinker = new Thinker();
			RandomNumberGenerator random = new RandomNumberGenerator();
			
			Godot.Collections.Array<Node> families = location.GetFamilies();
			
			thinker.BirthPlace = location;
			thinker.FamilyTree = families[ random.RandiRange( 0, families.Count - 1 ) ] as FamilyTree;
			thinker.Age = 0;
			thinker.BotName = string.Format( "{0} {1}", location.CreateBotName(), thinker.FamilyTree.Name );
			
			// now we pull a D&D
			// TODO: incorporate evolution over millions of years
			thinker.Strength = random.Next( 3, thinker.FamilyTree.GetStrengthMax() )
				+ thinker.FamilyTree.GetStrengthBonus();
			
			thinker.Consitution = random.Next( 3, thinker.FamilyTree.GetConsitutionMax() )
				+ thinker.FamilyTree.GetConstitutionBonus();
			
			thinker.Intelligence = random.Next( 3, thinker.FamilyTree.GetIntelligenceMax() )
				+ thinker.FamilyTree.GetIntelligenceBonus();
			
			thinker.Wisdom = random.Next( 3, thinker.FamilyTree.GetWisdomMax() )
				+ thinker.FamilyTree.GetWisdomBonus();
			
			thinker.Dexterity = random.Next( 3, thinker.FamilyTree.GetDexterityMax() )
				+ thinker.FamilyTree.GetDexterityBonus();
			
			thinker.MovementSpeed += thinker.Dexterity * 10.0f;
			thinker.Health += thinker.Strength * 2.0f + ( thinker.Constitution * 10.0f );
			
			thinker.GetTree().CurrentScene.GetNode( "Thinkers" ).AddChild( thinker );
			
			return thinker;
		}
		*/
	};
};
