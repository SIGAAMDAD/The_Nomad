namespace PlayerSystem.Runes {
	public partial class RuneOfWandering : Rune {
		public override void Connect( Player user ) {
			Owner = user;
		}
		public override void Activate() {
			Owner.SetMeta( "FastTravelRageCost", Owner.GetMeta( "FastTravelRageCost" ).AsSingle() / 2.0f );
		}
		public override void Disconnect() {
			Owner.SetMeta( "FastTravelRageCost", Owner.GetMeta( "FastTravelRageCost" ).AsSingle() * 2.0f );
		}
	};
};