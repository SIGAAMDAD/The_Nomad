/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using Renown.Thinkers;
using System.Collections.Generic;

namespace Renown.World {
	/*
	===================================================================================
	
	WorldArea
	
	===================================================================================
	*/
	
	public partial class WorldArea : Area2D {
		private static readonly float PlayerCheckInterval = 0.30f;
		public static DataCache<WorldArea> Cache;

		private float CheckDelta = 0.0f;

		[Export]
		public Biome Biome { get; private set; }
		[Export]
		public StringName AreaName { get; private set; }
		[Export]
		public AINodeCache NodeCache { get; private set; }
		[Export]
		protected TileMapLayer[] TileMaps;

		public bool PlayerStatus { get; private set; } = false;

		protected int ThreadSleep = Constants.THREADSLEEP_FACTION_PLAYER_AWAY;
		protected System.Threading.ThreadPriority Importance = Constants.THREAD_IMPORTANCE_PLAYER_AWAY;

		protected Node[] Children;

		/// <summary>
		/// The player's renown score in this WorldArea
		/// </summary>
		public int PlayerRenownScore { get; private set; } = 0;

		/// <summary>
		/// The player's known trait scores in this WorldArea
		/// </summary>
		public Dictionary<Trait, float>? PlayerTraitScores { get; private set; } = new Dictionary<Trait, float>();

		[Signal]
		public delegate void PlayerEnteredEventHandler();
		[Signal]
		public delegate void PlayerExitedEventHandler();

		public virtual void Save() {
			using var writer = new SaveSystem.SaveSectionWriter( AreaName, ArchiveSystem.SaveWriter );

			writer.SaveInt( nameof( PlayerRenownScore ), PlayerRenownScore );

			writer.SaveInt( "PlayerTraitCount", PlayerTraitScores.Count );
			foreach ( var trait in PlayerTraitScores ) {
				writer.SaveFloat( $"PlayerTraitScore{trait.Key.Name}", trait.Value );
			}
		}
		public virtual void Load() {
			using var reader = ArchiveSystem.GetSection( AreaName );

			if ( reader == null ) {
				return;
			}

			PlayerRenownScore = reader.LoadInt( nameof( PlayerRenownScore ) );

			int count = reader.LoadInt( "PlayerTraitCount" );
			PlayerTraitScores = new Dictionary<Trait, float>( count );
			for ( int i = 0; i < count; i++ ) {

			}
		}

		/*
		===============
		ChangePlayerTraitScore
		===============
		*/
		public void ChangePlayerTraitScore( Trait trait, float score ) {
		}

		/*
		===============
		ChangePlayerRenownScore
		===============
		*/
		public void ChangePlayerRenownScore( int score ) {

		}

		/*
		===============
		_ProcessAreaBody2DEntered
		===============
		*/
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

		/*
		===============
		OnProcessAreaBody2DExited
		===============
		*/
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

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			CollisionMask = (uint)( PhysicsLayer.WorldAreaPlayerStatus );
			CollisionLayer = (uint)( PhysicsLayer.WorldAreaPlayerStatus );

			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = (int)GetRid().Id;

			Godot.Collections.Array<Node> children = GetChildren();
			int count, i;

			count = 0;
			for ( i = 0; i < children.Count; i++ ) {
				// if we're not the enabler shape or a world area that needs an enabler shape, then don't disable
				if ( children[ i ].Name != "AreaShape" && children[ i ] is not WorldArea ) {
					count++;
				}
			}

			Children = new Node[ count ];
			count = 0;
			for ( i = 0; i < children.Count; i++ ) {
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

			if ( ArchiveSystem.IsLoaded() ) {
				Load();
			} else {
				PlayerRenownScore = 0;
			}

			GameEventBus.ConnectSignal( this, SignalName.BodyEntered, this, Callable.From<Node2D>( OnProcessAreaBody2DEntered ) );
			GameEventBus.ConnectSignal( this, SignalName.BodyExited, this, Callable.From<Node2D>( OnProcessAreaBody2DExited ) );
			GameEventBus.ConnectSignal( this, SignalName.BodyShapeEntered, this, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DEntered( body ) ) );
			GameEventBus.ConnectSignal( this, SignalName.BodyShapeExited, this, Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, localShapeIndex, bodyShapeIndex ) => OnProcessAreaBody2DExited( body ) ) );
		}

		/*
		===============
		_Process
		===============
		*/
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
				CheckDelta = 0.0f;
			}
		}
	};
};