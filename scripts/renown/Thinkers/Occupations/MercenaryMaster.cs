using System.Collections.Generic;
using Godot;
using Renown.World;

namespace Renown.Thinkers {
	public partial class Thinker : Entity {
		public partial class MercenaryMaster : Occupation {
			private List<Contract> ContractList = null;

			public MercenaryMaster( Thinker worker, Faction faction )
				: base( worker, faction )
			{
				Type = OccupationType.MercenaryMaster;

				ContractList = new List<Contract>();
			}
		};
	};
};