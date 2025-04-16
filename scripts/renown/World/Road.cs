using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Renown.World {
	public partial class Road : Node2D {
		[Export]
		private Settlement[] Links;
		[Export]
		protected StringName Title;
		[Export]
		protected NavigationRegion2D NavRegion;
		[Export]
		private Godot.Collections.Dictionary<WorldArea, Node2D[]> EntryNodes;
		
		protected HashSet<Settlement> LocationLinks = null;
		protected Dictionary<WorldArea, Node2D[]> EntryPoints = null;
		
		public StringName GetRoadName() => return Title;
		public NavigationRegion2D GetNavRegion() => return NavRegion;
		public HashSet<Settlement> GetLocationLinks() => LocationLinks;
		
		public Node2D FindClosestEntryPoint( WorldArea location, Godot.Vector2 position ) {
			if ( !EntryPoints.TryGetValue( location, out Node2D[] nodes ) ) {
				return null;
			}
			
			Node2D entryPoint = null;
			float bestDistance = float.MaxValue;
			
			for ( int i = 0; i < nodes.Length; i++ ) {
				float distance = nodes[i].GlobalPosition.DistanceTo( position );
				if ( distance < bestDistance ) {
					bestDistance = distance;
					entryPoint = nodes[i];
				}
			}
			
			return entryPoint;
		}
		
		public override void _Ready() {
			base._Ready();
			
			EntryPoints = new Dictionary<WorldArea, Node2D[]>( EntryNodes.Count );
			foreach ( var node in EntryNodes ) {
				EntryPoints.Add( node.Key, node.Value );
			}
			
			LocationLinks = new HashSet<Settlement>();
			for ( int i = 0; i < Links.Length; i++ ) {
				LocationLinks.Add( Links[i] );
			}
		}
	};
};
