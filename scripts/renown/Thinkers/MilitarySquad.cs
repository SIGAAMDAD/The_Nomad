using Renown.World;

namespace Renown.Thinkers {
	public partial class MilitarySquad : ThinkerGroup {
		
		
		public MilitarySquad( Faction faction )
			: base( GroupType.Military, faction )
		{ }

		public void SurroundTarget( Entity target ) {
		}
	};
};