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

		protected Node[] Children;

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
			if ( body is Player player && player != null ) {
				PlayerStatus = true;
				player.SetLocation( this );

				CallDeferred( MethodName.EmitSignal, SignalName.PlayerEntered );
				CallDeferred( MethodName.Show );

				for ( int i = 0; i < Children.Length; i++ ) {
					Children[ i ].SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Pausable );
				}
			}
		}
		private void OnProcessAreaBody2DExited( Node2D body ) {
			if ( !GetOverlappingBodies().Contains( LevelData.Instance.ThisPlayer ) ) {
				PlayerStatus = false;
				CallDeferred( MethodName.EmitSignal, SignalName.PlayerExited );
				CallDeferred( MethodName.Hide );

				for ( int i = 0; i < Children.Length; i++ ) {
					Children[ i ].SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );
				}
			}
		}

		public override void _Ready() {
			base._Ready();

			Connect( Area2D.SignalName.BodyEntered, Callable.From<Node2D>( OnProcessAreaBody2DEntered ) );
			Connect( Area2D.SignalName.BodyExited, Callable.From<Node2D>( OnProcessAreaBody2DExited ) );
			Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DEntered( body ) ) );
			Connect( Area2D.SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DExited( body ) ) );

			CollisionMask = (uint)( PhysicsLayer.WorldAreaPlayerStatus );
			CollisionLayer = (uint)( PhysicsLayer.WorldAreaPlayerStatus );

			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = (int)GetRid().Id;

			Godot.Collections.Array<Node> children = GetChildren();
			int count = 0;
			for ( int i = 0; i < children.Count; i++ ) {
				// if we're not the enabler shape or a world area that needs an enabler shape, then don't disable
				if ( children[ i ].Name != "AreaShape" && children[ i ] is not WorldArea ) {
					count++;
				}
			}

			Children = new Node[ count ];
			count = 0;
			for ( int i = 0; i < children.Count; i++ ) {
				if ( children[ i ].Name != "AreaShape" && children[ i ] is not WorldArea ) {
					Children[ count++ ] = children[ i ];
				}
			}

			if ( !IsInGroup( "WorldAreas" ) ) {
				AddToGroup( "WorldAreas" );
			}
			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( !IsInGroup( "Locations" ) ) {
				AddToGroup( "Locations" );
			}
			SetProcess( false );
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			CheckDelta += (float)delta;
			if ( CheckDelta > PlayerCheckInterval ) {
				if ( !GodotServerManager.GetCollidingObjects( GetRid() ).Contains( LevelData.Instance.ThisPlayer ) ) {
					PlayerStatus = false;
					CallDeferred( MethodName.EmitSignal, SignalName.PlayerExited );

					CallDeferred( MethodName.Hide );
					for ( int i = 0; i < Children.Length; i++ ) {
						Children[ i ].SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );
					}
				} else {
					PlayerStatus = true;
					CallDeferred( MethodName.EmitSignal, SignalName.PlayerEntered );

					CallDeferred( MethodName.Show );
					for ( int i = 0; i < Children.Length; i++ ) {
						Children[ i ].SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Pausable );
					}
				}
			}
		}
	};
};