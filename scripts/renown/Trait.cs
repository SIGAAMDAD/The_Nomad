namespace Renown {
	public enum TraitType : uint {
		Greedy,
		Cruel,
		WarCriminal,
		Honorable,
		Reliable,

		Count
	};

	public class Trait {
		public Trait() {
		}

		public virtual string GetName() {
			return "Invalid";
		}
		public virtual TraitType GetTraitType() {
			return TraitType.Count;
		}
	};
};