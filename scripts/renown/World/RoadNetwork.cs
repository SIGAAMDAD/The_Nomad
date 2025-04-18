using System.Collections.Generic;
using Godot;

namespace Renown.World {
	public partial class RoadNetwork : Node {
		private List<Road> Cache = null;

		private static RoadNetwork _Instance;

		public override void _Ready() {
			base._Ready();

			_Instance = this;

			Godot.Collections.Array<Node> roads = GetTree().GetNodesInGroup( "Roads" );
			Cache = new List<Road>( roads.Count );
			for ( int i = 0; i < roads.Count; i++ ) {
				Cache.Add( roads[i] as Road );
			}
		}

		public static Road GetRoad( WorldArea to, WorldArea from ) {
			for ( int i = 0; i < _Instance.Cache.Count; i++ ) {
				if ( _Instance.Cache[i].HasLocationLink( to ) && _Instance.Cache[i].HasLocationLink( from ) ) {
					return _Instance.Cache[i];
				}
			}
			return null;
		}
	};
};