using Renown.World;

namespace Renown.Thinkers {
	public partial class MilitarySquad : ThinkerGroup {
		public MilitarySquad( Faction faction )
			: base( GroupType.Military, faction )
		{ }

		public void SurroundTarget( Entity target ) {
		}
		public bool IsTargetInvestigated( Godot.Vector2 position ) {
			for ( int i = 0; i < Thinkers.Count; i++  ) {
				if ( ( Thinkers[i] as Mercenary ).GetInvestigationPosition() == position ) {
					return true;
				}
			}
			return false;
		}
	};
};