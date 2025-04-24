using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Thinker : Entity {
		public partial class Industry : Occupation {
			public Industry( Thinker worker, Faction faction )
				: base( worker, faction )
			{
				Type = OccupationType.Industry;
			}
		};
	};
};