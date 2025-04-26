namespace Renown.Traits {
	public partial class WarCriminal : Trait {
		public WarCriminal() {
		}

		public override string GetTraitName() => "WarCriminal";
		public override TraitType GetTraitType() => TraitType.WarCriminal;

		public static sbyte GetFearBias() => 80;
		public static sbyte GetTrustBias() => -50;

		public override bool Conflicts( Trait other ) {
			if ( other.GetTraitType() == TraitType.Honorable || other.GetTraitType() == TraitType.Merciful ) {
				return true;
			}
			return false;
		}
		public override bool Conflicts( TraitType other ) {
			if ( other == TraitType.Honorable || other == TraitType.Merciful ) {
				return true;
			}
			return false;
		}
		public override bool Agrees( Trait other ) {
			if ( other.GetTraitType() == TraitType.Honorable || other.GetTraitType() == TraitType.Merciful ) {
				return false;
			}
			return true;
		}
		public override bool Agrees( TraitType other ) {
			if ( other == TraitType.Honorable || other == TraitType.Merciful ) {
				return false;
			}
			return true;
		}
	};
};