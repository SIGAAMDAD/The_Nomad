using Godot;
using Renown.World;

namespace Renown.Thinkers.Occupations {
	public partial class Politician : Occupation {
		public Politician( Thinker worker, Faction faction )
			: base( worker, faction )
		{
			Type = OccupationType.Politician;
			Wage = DefaultWages[ OccupationType.Politician ];
		}
	};
};