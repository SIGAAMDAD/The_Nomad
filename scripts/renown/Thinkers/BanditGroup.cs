using Renown.World;

namespace Renown.Thinkers {
	public partial class BanditGroup : ThinkerGroup {
		public BanditGroup( Faction faction )
			: base( GroupType.Bandit, faction )
		{ }

		public bool IsRouteOccupied( AIPatrolRoute route ) {
			for ( int i = 0; i < Thinkers.Count; i++ ) {
				if ( Thinkers[i] is Thinker mob && mob != null
					&& ( mob.GetOccupation() as Thinker.Bandit ).GetPatrolRoute() == route )
				{
					return true;
				}
			}
			return false;
		}

		public void SurroundTarget( Entity target ) {
		}
	};
};