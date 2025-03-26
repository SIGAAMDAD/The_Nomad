using Godot;
using System.Collections.Generic;

namespace Renown {
	// most thinkers except for politicians will most likely never get the chance nor the funds
	// to hire a personal mercenary
	public partial class Thinker : CharacterBody2D {
		public enum Occupation {
			None,

			Bandit,
			
			Blacksmith,
			
			Mercenary,
			Politician,
			
			Count
		};
		
		[Export]
		private StringName BotName;
		[Export]
		private Godot.Collections.Array<Relationship> SetRelations; // preset relationships
		[Export]
		private Occupation Job;
//		[Export]
//		private EventHistory History;
//		[Export]
//		private Godot.Collections.Array<Trait> Traits;
		[Export]
		private uint Age = 0; // in years
		[Export]
		private Settlement Location = null;
		
		[ExportCategory("Stats")]
		[Export]
		private uint Strength;
		[Export]
		private uint Intelligence;
		[Export]
		private uint Constitution;
		[Export]
		private float Health = 100.0f;
		[Export]
		private bool HasMetPlayer = false;
		[Export]
		private Godot.Collections.Dictionary<string, bool> Personality;
		[Export]
		private float MovementSpeed = 40.0f;
		
		private Dictionary<Thinker, Relationship> Relations = new Dictionary<Thinker, Relationship>();
		protected NavigationAgent2D NavAgent;
		private Godot.Vector2 AngleDir;

		protected Godot.Vector2 PhysicsPosition = Godot.Vector2.Zero;

		private Settlement Settlement;

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

		private uint Money = 0;
		private uint Pay = 0;
		
		// called when entering a shop
//		public void SetCurrentShop( Shop shop ) {
//			Agent.State[ "Vendor" ] = shop;
//		}
		// called when entering a settlement
		public void SetCurrentSettlement( Settlement location ) => Settlement = location;
		
		protected virtual void SendPacket() {
		}
		protected virtual void ReceivePacket( System.IO.BinaryReader reader ) {
		}

		public virtual void Save() {
		}
		public virtual void Load() {
		}

		protected void ProcessAnimations()  {
			ArmAnimations.SetDeferred( "global_rotation", AimAngle );
			HeadAnimations.SetDeferred( "global_rotation", LookAngle );
		
			if ( LookAngle > 0.0f ) {
				HeadAnimations.SetDeferred( "flip_v", true );
			} else if ( LookAngle < 0.0f ) {
				HeadAnimations.SetDeferred( "flip_v", false );
			}
			if ( AimAngle > 0.0f ) {
				ArmAnimations.SetDeferred( "flip_v", true );
			} else if ( AimAngle < 0.0f ) {
				ArmAnimations.SetDeferred( "flip_v", false );
			}
			if ( Velocity.X > 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", false );
				ArmAnimations.SetDeferred( "flip_h", false );
			} else if ( Velocity.X < 0.0f ) {
				BodyAnimations.SetDeferred( "flip_h", true );
				ArmAnimations.SetDeferred( "flip_h", true );
			}

			if ( Velocity != Godot.Vector2.Zero ) {
				BodyAnimations.CallDeferred( "play", "move" );
				ArmAnimations.CallDeferred( "play", "move" );
				HeadAnimations.CallDeferred( "play", "move" );
			} else {
				BodyAnimations.CallDeferred( "play", "idle" );
				ArmAnimations.CallDeferred( "play", "idle" );
				HeadAnimations.CallDeferred( "play", "idle" );
			}
		}

		public override void _Ready() {
			base._Ready();

			if ( SettingsData.GetNetworkingEnabled() ) {
//				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, SendPacket, ReceivePacket ) );
			}

			NavAgent = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
			NavAgent.Connect( "target_reached", Callable.From( OnTargetReached ) );
		}
		public override void _Process( double delta ) {
			if ( ( Engine.GetProcessFrames() % 24 ) != 0 ) {
				return;
			}

			base._Process( delta );

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
		public override void _PhysicsProcess( double delta ) {
			base._PhysicsProcess( delta );
		
			PhysicsPosition = GlobalPosition;
			if ( MoveTimer.IsStopped() && Velocity != Godot.Vector2.Zero ) {
				MoveTimer.Start();
			}
			MoveAlongPath();
		}
		protected virtual void Think( float delta ) {
		}

		protected bool MoveAlongPath() {
			if ( GlobalPosition.DistanceTo( GotoPosition ) <= 10.0f ) {
				Velocity = Godot.Vector2.Zero;
				return true;
			}
			Godot.Vector2 nextPathPosition = NavAgent.GetNextPathPosition();
			AngleDir = GlobalPosition.DirectionTo( nextPathPosition );
			LookAngle = Mathf.Atan2( AngleDir.Y, AngleDir.X );
			Velocity = AngleDir * MovementSpeed;
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