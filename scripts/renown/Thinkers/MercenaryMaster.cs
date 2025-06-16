using Godot;
using Renown.World;
using System.Collections.Generic;

namespace Renown.Thinkers {
	public partial class MercenaryMaster : Thinker {
		[Export]
		private Faction Guild;

		private HashSet<Contract> ContractList = new HashSet<Contract>();
		public Godot.Collections.Array<Contract> GetContracts() => [ .. ContractList ];

		public void SubmitContract( Contract contract ) {
			ContractList.Add( contract );
		}

		public override void _Ready() {
			base._Ready();
		}
	};
};