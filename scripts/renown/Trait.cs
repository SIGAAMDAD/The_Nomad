using Godot;

namespace Renown {
	public enum TraitType : uint {
		Greedy,
		Cruel,
		WarCriminal,
		Honorable,
		Reliable,
		Merciful,
		Liar,

		Count
	};

	public abstract partial class Trait : Node {
		public Trait() {
		}

		public static Trait Create( TraitType nType ) {
			switch ( nType ) {
			case TraitType.Greedy:
				return new Traits.Greedy();
			case TraitType.Cruel:
				return new Traits.Cruel();
			case TraitType.WarCriminal:
				return new Traits.WarCriminal();
			case TraitType.Honorable:
				return new Traits.Honorable();
			case TraitType.Reliable:
				return new Traits.Reliable();
			case TraitType.Merciful:
				return new Traits.Merciful();
			case TraitType.Liar:
				return new Traits.Liar();
			default:
				break;
			};
			return null;
		}

		public abstract string GetTraitName();
		public abstract TraitType GetTraitType();
		
		public abstract bool Conflicts( Trait other );
		public abstract bool Conflicts( TraitType other );
		public abstract bool Agrees( Trait other );
		public abstract bool Agrees( TraitType other );
	};
};