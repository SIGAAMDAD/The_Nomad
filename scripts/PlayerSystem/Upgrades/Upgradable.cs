using System.Collections.Generic;

namespace PlayerSystem.Upgrades {
	public interface IUpgradable {
		public int Level { get; }
		public int MaxLevel { get; }

		public IReadOnlyDictionary<string, int> GetUpgradeCost();
		public void ApplyUpgrade();
	};
};