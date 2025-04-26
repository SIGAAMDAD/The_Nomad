namespace Renown.Traits {
	public partial class Cruel : Trait {
		public Cruel() {
		}

		public override string GetTraitName() => "Cruel";
		public override TraitType GetTraitType() => TraitType.Cruel;

		public static sbyte GetFearBias() => 30;
		public static sbyte GetTrustBias() => -10;

		public override bool Conflicts( Trait other ) {
			if ( other.GetTraitType() == TraitType.Merciful ) {
				return true;
			}
			return false;
		}
		public override bool Conflicts( TraitType other ) {
			if ( other == TraitType.Merciful ) {
				return true;
			}
			return false;
		}
		public override bool Agrees( Trait other ) {
			if ( other.GetTraitType() == TraitType.Merciful ) {
				return false;
			}
			return true;
		}
		public override bool Agrees( TraitType other ) {
			if ( other == TraitType.Merciful ) {
				return false;
			}
			return true;
		}
	};
};