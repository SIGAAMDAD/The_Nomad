using Renown.World;

namespace Renown.Thinkers {
	public partial class Thinker : Entity {
		public partial class Doctor : Occupation {
			public Doctor( Thinker worker, Faction faction )
				: base( worker, faction )
			{
				Type = OccupationType.Doctor;
			}
		};
	};
};