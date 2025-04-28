using Godot;
using Renown;

public partial class StatusBurning : StatusEffect {
	private Entity _Owner;

	public StatusBurning( Entity owner ) {
		_Owner = owner;
	}

	public override void _Process( double delta ) {
		base._Process( delta );

		_Owner.Damage( null, 0.25f * (float)delta );
	}
};
