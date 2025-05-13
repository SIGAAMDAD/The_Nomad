using Godot;
using Renown;

public partial class Hitbox : Area2D {
	[Export]
	private Entity Parent;

	[Signal]
	public delegate void HitEventHandler( Entity source );

	public void OnHit( Entity source ) => EmitSignalHit( source );

	public override void _Ready() {
		base._Ready();

		SetMeta( "Owner", Parent );
	}
};