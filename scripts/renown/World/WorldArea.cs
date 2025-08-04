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

					SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Pausable );
				}
				entity.SetLocation( this );
			}
		}
		private void OnProcessAreaBody2DExited( Node2D body ) {
			if ( !GodotServerManager.GetCollidingObjects( GetRid() ).Contains( LevelData.Instance.ThisPlayer ) ) {
				PlayerStatus = false;
				EmitSignalPlayerExited();

				SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );
			}
		}

		public override void _Ready() {
			base._Ready();

			Connect( Area2D.SignalName.BodyEntered, Callable.From<Node2D>( OnProcessAreaBody2DEntered ) );
			Connect( Area2D.SignalName.BodyExited, Callable.From<Node2D>( OnProcessAreaBody2DExited ) );
			Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DEntered( body ) ) );
			Connect( Area2D.SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DExited( body ) ) );

			ProcessMode = ProcessModeEnum.Disabled;
			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = (int)GetRid().Id;

			AddToGroup( "WorldAreas" );

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( !IsInGroup( "Locations" ) ) {
				AddToGroup( "Locations" );
			}
			Hide();
		}
	};
};