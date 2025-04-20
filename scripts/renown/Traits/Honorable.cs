namespace Renown.Traits {
	public partial class Honorable : Trait {
		public Honorable() {
		}

		public override string GetTraitName() => "Honorable";
		public override TraitType GetTraitType() => TraitType.Honorable;

		public override sbyte GetFearBias() => -50;
		public override sbyte GetTrustBias() => 90;

		public override bool Conflicts( Trait other ) {
			if ( other.GetTraitType() == TraitType.WarCriminal ) {
				return true;
			}
			return false;
		}
		public override bool Conflicts( TraitType other ) {
			if ( other == TraitType.WarCriminal ) {
				return true;
			}
			return false;
		}
		public override bool Agrees( Trait other ) {
			if ( other.GetTraitType() == TraitType.WarCriminal ) {
				return false;
			}
			return true;
		}
		public override bool Agrees( TraitType other ) {
			if ( other == TraitType.WarCriminal ) {
				return false;
			}
			return true;
		}
	};
};