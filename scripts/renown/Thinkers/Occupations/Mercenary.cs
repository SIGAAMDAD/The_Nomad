using Renown.World;

namespace Renown.Thinkers {
	public partial class Thinker : Entity {
		public partial class Mercenary : Occupation {
			public Mercenary( Thinker worker, Faction faction )
				: base( worker, faction )
			{
				Type = OccupationType.Mercenary;
			}
		};
	};
};