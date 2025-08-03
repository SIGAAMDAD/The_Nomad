using Godot;
using System.Diagnostics;

namespace Renown.World {
	public partial class BiomeManager : Node {
		private static readonly float ActivateDistance = 2048.0f;
		private static readonly float ProcessDeltaInterval = 0.30f;

		private float CheckDelta = 0.0f;

		private struct AreaData {
			public Godot.Vector2 Position;
			public WorldArea Area;
		};

		private AreaData[] AreaCache;

		public override void _Ready() {
			base._Ready();

			Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "WorldAreas" );
			AreaCache = new AreaData[ nodes.Count ];
			for ( int i = 0; i < nodes.Count; i++ ) {
				AreaCache[ i ] = new AreaData();
				AreaCache[ i ].Area = nodes[ i ] as WorldArea;
				AreaCache[ i ].Position = AreaCache[ i ].Area.GlobalPosition;
			}
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			CheckDelta += (float)delta;
			if ( CheckDelta >= ProcessDeltaInterval ) {
				Godot.Vector2 position = LevelData.Instance.ThisPlayer.GlobalPosition;
				System.Threading.Tasks.Parallel.For( 0, AreaCache.Length, ( index ) => {
					if ( position.DistanceTo( AreaCache[ index ].Position ) < ActivateDistance ) {
						AreaCache[ index ].Area.SetDeferred( WorldArea.PropertyName.ProcessMode, (long)ProcessModeEnum.Pausable );
						AreaCache[ index ].Area.SetDeferred( WorldArea.PropertyName.Visible, true );
					} else {
						AreaCache[ index ].Area.SetDeferred( WorldArea.PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled );
						AreaCache[ index ].Area.SetDeferred( WorldArea.PropertyName.Visible, false );
					}
				} );
				CheckDelta = 0.0f;
			}
		}
	};
};