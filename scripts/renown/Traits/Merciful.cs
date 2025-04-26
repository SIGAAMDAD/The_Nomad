namespace Renown.Traits {
	public partial class Merciful : Trait {
		public Merciful() {
		}

		public override string GetTraitName() => "Merciful";
		public override TraitType GetTraitType() => TraitType.Merciful;

		public static sbyte GetFearBias() => -30;

		// mercy is rare in the wastes
		public static sbyte GetTrustBias() => 50;

		public override bool Conflicts( Trait other ) {
			if ( other.GetTraitType() == TraitType.Cruel || other.GetTraitType() == TraitType.WarCriminal ) {
				return true;
			}
			return false;
		}
		public override bool Conflicts( TraitType other ) {
			if ( other == TraitType.Cruel || other == TraitType.WarCriminal ) {
				return true;
			}
			return false;
		}
		public override bool Agrees( Trait other ) {
			if ( other.GetTraitType() == TraitType.Cruel || other.GetTraitType() == TraitType.WarCriminal ) {
				return false;
			}
			return true;
		}
		public override bool Agrees( TraitType other ) {
			if ( other == TraitType.Cruel || other == TraitType.WarCriminal ) {
				return false;
			}
			return true;
		}
	};
};