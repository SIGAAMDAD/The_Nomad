using Godot;
using System;
using Renown.Thinkers;

namespace Renown.World {
	public partial class WorldArea : Area2D {
		public static DataCache<WorldArea> Cache;

		[Export]
		protected Biome Biome;
		[Export]
		protected StringName AreaName;
		[Export]
		protected AINodeCache NodeCache;
		[Export]
		protected TileMapLayer[] TileMaps;

		protected bool PlayerStatus = false;

		protected int ThreadSleep = Constants.THREADSLEEP_FACTION_PLAYER_AWAY;
		protected System.Threading.ThreadPriority Importance = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;

		[Signal]
		public delegate void PlayerEnteredEventHandler();
		[Signal]
		public delegate void PlayerExitedEventHandler();

		public StringName GetAreaName() => AreaName;
		public AINodeCache GetNodeCache() => NodeCache;
		public bool IsPlayerHere() => PlayerStatus;
		public Biome GetBiome() => Biome;

		public virtual void Save() {
		}
		public virtual void Load() {
		}

		private void OnProcessAreaBody2DEntered( Node2D body ) {
			if ( body is Entity entity && entity != null ) {
				if ( body is Player player && player != null ) {
					PlayerStatus = true;
					EmitSignalPlayerEntered();
				}
				entity.SetLocation( this );
			}
		}
		private void OnProcessAreaBody2DExited( Node2D body ) {
			if ( !GetOverlappingBodies().Contains( LevelData.Instance.ThisPlayer ) ) {
				PlayerStatus = false;
				EmitSignalPlayerExited();
			}
		}

		public override void _Ready() {	
			base._Ready();
			
			Connect( "body_entered", Callable.From<Node2D>( OnProcessAreaBody2DEntered ) );
			Connect( "body_exited", Callable.From<Node2D>( OnProcessAreaBody2DExited ) );
			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DEntered( body ) ) );
			Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DExited( body ) ) );

			ProcessMode = ProcessModeEnum.Pausable;
			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = (int)GetRid().Id;

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( !IsInGroup( "Locations" ) ) {
				AddToGroup( "Locations" );
			}
		}
	};
};