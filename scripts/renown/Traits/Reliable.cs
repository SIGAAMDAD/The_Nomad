namespace Renown.Traits {
	public partial class Reliable : Trait {
		public Reliable() {
		}

		public override string GetTraitName() => "Reliable";
		public override TraitType GetTraitType() => TraitType.Reliable;

		public static sbyte GetFearBias() => -30;

		// mercy is rare in the wastes
		public static sbyte GetTrustBias() => 50;

		public override bool Conflicts( Trait other ) {
			if ( other.GetTraitType() == TraitType.Liar ) {
				return true;
			}
			return false;
		}
		public override bool Conflicts( TraitType other ) {
			if ( other == TraitType.Liar ) {
				return true;
			}
			return false;
		}
		public override bool Agrees( Trait other ) {
			if ( other.GetTraitType() == TraitType.Liar ) {
				return false;
			}
			return true;
		}
		public override bool Agrees( TraitType other ) {
			if ( other == TraitType.Liar ) {
				return false;
			}
			return true;
		}
	};
};