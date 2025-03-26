namespace Renown.Traits {
	public class Cruel : Trait {
		public Cruel() {
		}

		public override string GetName() => "Cruel";
		public override TraitType GetTraitType() => TraitType.Cruel;

		public override sbyte GetFearBias() => 30;
		public override sbyte GetTrustBias() => 20;
    };
};