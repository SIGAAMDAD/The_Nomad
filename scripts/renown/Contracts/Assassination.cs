using Godot;
using Renown.Thinkers;
using Renown.World;

namespace Renown {
	public partial class Assassination : Contract {
		private Thinker Target;

		public Assassination( string name, WorldTimestamp duedate, ContractFlags flags, ContractType type,
			float basePay, WorldArea area, Object contractor, Faction guild
		)
			: base( name, duedate, flags, type, basePay, area, contractor, guild )
		{ }
	};
};