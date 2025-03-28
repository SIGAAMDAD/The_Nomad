using Godot;
using System.Collections.Generic;
using Renown.World;

namespace Renown {
	// most thinkers except for politicians will most likely never get the chance nor the funds
	// to hire a personal mercenary
	public partial class Thinker : CharacterBody2D {
		public static DataCache<Thinker> Cache;

		public enum Occupation {
			None,

			Bandit,
			
			Blacksmith,
			
			Mercenary,
			Politician,
			
			Count
		};

		[Export]
		protected TileMapFloor Floor;
		
		[Export]
		protected StringName BotName;
		[Export]
		protected Godot.Collections.Array<Relationship> SetRelations; // preset relationships
		[Export]
		protected Occupation Job;
//		[Export]
//		private EventHistory History;
//		[Export]
//		private Godot.Collections.Array<Trait> Traits;
		[Export]
		protected uint Age = 0; // in years
		[Export]
		protected WorldArea Location;

		[ExportCategory("Start")]
		[Export]
		protected DirType Direction;
		
		[ExportCategory("Stats")]
		[Export]
		protected uint Strength;
		[Export]
		protected uint Intelligence;
		[Export]
		protected uint Constitution;
		[Export]
		protected float Health = 100.0f;
		[Export]
		private bool HasMetPlayer = false;
		[Export]
		private Godot.Collections.Dictionary<string, bool> Personality;
		[Export]
		private float MovementSpeed = 40.0f;
		[Export]
		protected Faction Faction;
		
		protected Dictionary<Thinker, Relationship> Relations = new Dictionary<Thinker, Relationship>();
		protected NavigationAgent2D NavAgent;
		protected Godot.Vector2 LookDir = Godot.Vector2.Zero;

		protected Godot.Vector2 PhysicsPosition = Godot.Vector2.Zero;

		protected AnimatedSprite2D BodyAnimations;
		protected AnimatedSprite2D ArmAnimations;
		protected AnimatedSprite2D HeadAnimations;

		// memory
		protected bool TargetReached = false;
		protected Godot.Vector2 GotoPosition = Godot.Vector2.Zero;
		protected float LookAngle = 0.0f;
		protected float AimAngle = 0.0f;

		protected static readonly Color DefaultColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
		protected Color DemonEyeColor;

		protected Timer MoveTimer = null;
		protected AudioStreamPlayer2D MoveChannel;

		// networking
		protected NetworkWriter SyncObject = null;

		protected uint Money = 0;
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

		public virtual void Save() {
			SaveSystem.SaveSectionWriter writer = new SaveSystem.SaveSectionWriter( "Thinker_" + GetPath() );

			writer.SaveVector2( "position", GlobalPosition );
			writer.SaveFloat( "health", Health );
			writer.SaveUInt( "age", Age );
			writer.Flush();
		}
		public virtual void Load() {
			SaveSystem.SaveSectionReader reader = ArchiveSystem.GetSection( "Thinker_" + GetPath() );

			// save file compability
			if ( reader == null ) {
				return;
			}

			SetDeferred( "global_position", reader.LoadVector2( "position" ) );
			SetDeferred( "health", reader.LoadFloat( "health" ) );
			SetDeferred( "age", reader.LoadUInt( "age" ) );
		}

		protected virtual void OnScreenEnter() {
			OnScreen = true;
		}
		protected virtual void OnScreenExit() {
			OnScreen = false;
		}

		protected void InitBaseThinker() {
			base._Ready();

			NavAgent = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
			NavAgent.Connect( "target_reached", Callable.From( OnTargetReached ) );

			VisibilityNotifier = GetNode<VisibleOnScreenNotifier2D>( "VisibleOnScreenNotifier2D" );
			VisibilityNotifier.Connect( "screen_entered", Callable.From( OnScreenEnter ) );
			VisibilityNotifier.Connect( "screen_exited", Callable.From( OnScreenExit ) );
			
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
			if ( MoveTimer.IsStopped() && Velocity != Godot.Vector2.Zero ) {
				MoveTimer.Start();
			}
			MoveAlongPath();
		}
		public override void _Process( double delta ) {
			base._Process( delta );
			if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
				return;
			}

			if ( GameConfiguration.DemonEyeActive ) {
				HeadAnimations.SetDeferred( "modulate", DemonEyeColor );
				ArmAnimations.SetDeferred( "modulate", DemonEyeColor );
				BodyAnimations.SetDeferred( "modulate", DemonEyeColor );
			} else {
				HeadAnimations.SetDeferred( "modulate", DefaultColor );
			}

			ProcessAnimations();

			Think( (float)delta );
		}
        protected virtual void Think( float delta ) {
		}
		protected virtual void ProcessAnimations() {
		}

		protected bool MoveAlongPath() {
			if ( GlobalPosition.DistanceTo( GotoPosition ) <= 10.0f ) {
				Velocity = Godot.Vector2.Zero;
				return true;
			}
			Godot.Vector2 nextPathPosition = NavAgent.GetNextPathPosition();
			LookDir = GlobalPosition.DirectionTo( nextPathPosition );
			LookAngle = Mathf.Atan2( LookDir.Y, LookDir.X );
			Velocity = LookDir * MovementSpeed;
			return MoveAndSlide();
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
	};
};