public partial class StatusBurning : StatusEffect {
	public override void _Process( double delta ) {
		base._Process( delta );

		Victim.Damage( null, 8.0f * (float)delta );
	}
};
