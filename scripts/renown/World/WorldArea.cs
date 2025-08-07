using Godot;
using System;
using Renown.Thinkers;

namespace Renown.World {
	public partial class WorldArea : Area2D {
		private static readonly float PlayerCheckInterval = 0.30f;
		public static DataCache<WorldArea> Cache;

		private float CheckDelta = 0.0f;

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

		/*
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
		*/

		public override void _Ready() {
			base._Ready();

//			Connect( Area2D.SignalName.BodyEntered, Callable.From<Node2D>( OnProcessAreaBody2DEntered ) );
//			Connect( Area2D.SignalName.BodyExited, Callable.From<Node2D>( OnProcessAreaBody2DExited ) );
//			Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DEntered( body ) ) );
//			Connect( Area2D.SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DExited( body ) ) );

			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = (int)GetRid().Id;

			AddToGroup( "WorldAreas" );

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( !IsInGroup( "Locations" ) ) {
				AddToGroup( "Locations" );
			}
		}

		private void Disable() {
			PlayerStatus = false;
			EmitSignalPlayerExited();

			Hide();
			Godot.Collections.Array<Node> children = GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				if ( children[ i ].Name != "AreaShape" ) {
					children[ i ].SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );
				}
			}
		}
		private void Enable() {
			PlayerStatus = true;
			EmitSignalPlayerEntered();

			Show();
			Godot.Collections.Array<Node> children = GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				children[ i ].SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Pausable );
			}
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			CheckDelta += (float)delta;
			if ( CheckDelta > PlayerCheckInterval ) {
				if ( !GetOverlappingBodies().Contains( LevelData.Instance.ThisPlayer ) ) {
					CallDeferred( MethodName.Disable );
				} else {
					CallDeferred( MethodName.Enable );
				}
			}
		}
	};
};