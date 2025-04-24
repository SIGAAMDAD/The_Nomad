using Godot;
using System.Collections.Generic;

namespace Renown.World {
	public class ContractManager {
		private static Dictionary<int, Contract> ContractCache = null;

		public static void Init() {
			ContractCache = new Dictionary<int, Contract>();
		}
		public static void AddContract( Settlement location, Contract contract ) {
		}
	};
};