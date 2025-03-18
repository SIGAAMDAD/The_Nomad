using Godot;
using System;

public partial class StatusBurning : StatusEffect {
	private CharacterBody2D _Owner;

	public StatusBurning( CharacterBody2D owner ) {
		_Owner = owner;
	}

	public override StatusEffectType GetEffectType() {
		return StatusEffectType.Burning;
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		_Owner.Call( "Damage", 0.25f * (float)delta );
	}
};
