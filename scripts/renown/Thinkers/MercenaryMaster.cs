using System.Collections.Generic;

namespace Renown.Thinkers {
	public partial class MercenaryMaster : Thinker {
		private List<Contract> ContractList = null;

		public override void _Ready() {
			base._Ready();

			ContractList = new List<Contract>();
		}

		public void GetContractList() {
		}
	};
};