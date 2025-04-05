namespace Renown.Traits {
	public partial class Cruel : Trait {
		public Cruel() {
		}

		public override string GetTraitName() => "Cruel";
		public override TraitType GetTraitType() => TraitType.Cruel;

		public override sbyte GetFearBias() => 30;
		public override sbyte GetTrustBias() => 20;

		public override bool Conflicts( Trait other ) {
			return false;
		}
		public override bool Agrees( Trait other ) {
			return false;
		}
	};
};