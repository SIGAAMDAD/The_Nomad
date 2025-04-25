using System.Collections.Generic;
using Godot;

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

		public StringName GetAreaName() => AreaName;

		[Signal]
		public delegate void PlayerEnteredEventHandler();
		[Signal]
		public delegate void PlayerExitedEventHandler();

		public virtual void Save() {
		}
		public virtual void Load() {
		}

		public AINodeCache GetNodeCache() => NodeCache;
		public bool IsPlayerHere() => PlayerStatus;
		public Biome GetBiome() => Biome;

		private void OnProcessAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			Player player = body as Player;
			if ( player == null ) {
				return;
			}
			PlayerStatus = true;
			EmitSignalPlayerEntered();
		}
		private void OnProcessAreaBodyShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			Player player = body as Player;
			if ( player == null ) {
				return;
			}
			PlayerStatus = false;
			EmitSignalPlayerExited();
		}

		public override void _Ready() {	
			base._Ready();

			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnProcessAreaBodyShape2DEntered ) );
			Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnProcessAreaBodyShape2DExited ) );

//			ProcessMode = ProcessModeEnum.Pausable;
//			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
//			ProcessThreadGroupOrder = Constants.THREAD_GROUP_BIOMES;

			if ( SettingsData.GetNetworkingEnabled() ) {
			}
			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( !IsInGroup( "Locations" ) ) {
				AddToGroup( "Locations" );
			}
		}
    };
};