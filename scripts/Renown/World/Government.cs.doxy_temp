using Godot;
using Renown.Thinkers;

namespace Renown.World {
	public partial class Government : Faction {
		public override void _Ready() {
			base._Ready();

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
		}
	};
};
