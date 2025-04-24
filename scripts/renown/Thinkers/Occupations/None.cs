using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Thinker : Entity {
		public partial class None : Occupation {
			public None( Thinker worker, Faction faction )
				: base( worker, faction )
			{
				Type = OccupationType.None;
			}
		};
	};
};