namespace Renown {
	public enum TraitType : uint {
		Greedy,
		Cruel,
		WarCriminal,
		Honorable,
		Reliable,

		Count
	};

	public abstract class Trait {
		public Trait() {
		}

		public abstract string GetName();
		public abstract TraitType GetTraitType();

		public abstract sbyte GetFearBias();
		public abstract sbyte GetTrustBias();
	};
};