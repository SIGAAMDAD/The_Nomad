namespace Renown.Traits {
	public partial class Liar : Trait {
		public Liar() {
		}

		public override string GetTraitName() => "Liar";
		public override TraitType GetTraitType() => TraitType.Liar;

		public static sbyte GetFearBias() => 0;
		public static sbyte GetTrustBias() => -50;

		public override bool Conflicts( Trait other ) {
			if ( other.GetTraitType() == TraitType.Honorable ) {
				return true;
			}
			return false;
		}
		public override bool Conflicts( TraitType other ) {
			if ( other == TraitType.Honorable ) {
				return true;
			}
			return false;
		}
		public override bool Agrees( Trait other ) {
			if ( other.GetTraitType() == TraitType.Honorable ) {
				return false;
			}
			return true;
		}
		public override bool Agrees( TraitType other ) {
			if ( other == TraitType.Honorable ) {
				return false;
			}
			return true;
		}
	};
};