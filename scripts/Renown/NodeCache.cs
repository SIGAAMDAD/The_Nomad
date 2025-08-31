using Godot;
using System.Collections.Generic;

namespace Renown {
	public partial class NodeCache : Node {
		private Dictionary<NodePath, Node> Cache;

		private void LoadNodes() {
		}

		public override void _Ready() {
			base._Ready();

			if ( ArchiveSystem.IsLoaded() ) {
				
			} else {
				Cache = new Dictionary<NodePath, Node>();
			}
		}
	};
};