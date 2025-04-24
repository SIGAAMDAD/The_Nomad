using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Thinker : Entity {
		public partial class MercenaryMaster : Occupation {
			public MercenaryMaster( Thinker worker, Faction faction )
				: base( worker, faction )
			{
				Type = OccupationType.MercenaryMaster;
			}
		};
	};
};