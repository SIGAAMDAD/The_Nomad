using Godot;

namespace Renown {
	public enum TraitType : uint {
		Greedy,
		Cruel,
		WarCriminal,
		Honorable,
		Reliable,

		Count
	};

	public abstract partial class Trait : Node {
		public Trait() {
		}

		public abstract string GetTraitName();
		public abstract TraitType GetTraitType();

		public abstract sbyte GetFearBias();
		public abstract sbyte GetTrustBias();

		public abstract bool Conflicts( Trait other );
		public abstract bool Agrees( Trait other );
	};
};