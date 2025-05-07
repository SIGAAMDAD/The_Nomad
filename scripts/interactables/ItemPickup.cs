using GDExtension.Wrappers;
using Godot;

public partial class ItemPickup : InteractionItem {
	[Export]
	public Resource Data;

	private Sprite2D Icon;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		//TODO: auto-pickup toggle?
		Player player = body as Player;
		if ( player == null ) {
			return;
		}

		Godot.Collections.Array<Resource> Categories = (Godot.Collections.Array<Resource>)Data.Get( "categories" );

		string name = (string)Categories[0].Get( "name" );
		switch ( name ) {
		case "Weapon":
			WeaponEntity weapon = new WeaponEntity();
			weapon.Name = "Weapon";
			weapon.Data = Data as ItemDefinition;
			weapon.SetResourcePath( "player/" );
			weapon.SetOwner( player );
			break;
		case "Ammo":
			break;
		};

		Icon.QueueFree();
		QueueFree();
	}

	public override void _Ready() {
		base._Ready();

		Icon = GetNode<Sprite2D>( "Icon" );
		Icon.SetProcess( false );
		Icon.SetProcessInternal( false );

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};