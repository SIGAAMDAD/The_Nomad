namespace Renown.Traits {
	public partial class Greedy : Trait {
		public Greedy() {
		}

		public override string GetTraitName() => "Greedy";
		public override TraitType GetTraitType() => TraitType.Greedy;

		public override sbyte GetFearBias() => 0;
		public override sbyte GetTrustBias() => -20;

		public override bool Conflicts( Trait other ) {
			return false;
		}
		public override bool Conflicts( TraitType other ) {
			return false;
		}
		public override bool Agrees( Trait other ) {
			return true;
		}
		public override bool Agrees( TraitType other ) {
			return true;
		}
	};
};