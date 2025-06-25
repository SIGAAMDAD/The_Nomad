using Godot;
using System.Collections.Generic;

public partial class ItemNodeCache : Node {
	private HashSet<UsedItemPickup> Items;

	private static ItemNodeCache Instance;

	private void Load() {
		using var reader = ArchiveSystem.GetSection( "ItemNodeCache" );

		// save file compatibility
		if ( reader == null ) {
			return;
		}

		int count = reader.LoadInt( "Count" );
		Items = new HashSet<UsedItemPickup>( count );

		for ( int i = 0; i < count; i++ ) {
			string name = reader.LoadString( string.Format( "ItemPickupName{0}", i ) );
			string type = reader.LoadString( string.Format( "ItemPickupType{0}", i ) );

			switch ( type ) {
			case "Weapon": {
				WeaponEntity weapon = new WeaponEntity();
				weapon.Load( reader, i );

				UsedItemPickup pickup = ResourceCache.GetScene( "res://scenes/interactables/used_item_pickup.tscn" ).Instantiate<UsedItemPickup>();
				pickup.Entity = weapon;
				Items.Add( pickup );
				break; }
			case "Ammo": {
				AmmoEntity ammo = new AmmoEntity();
				ammo.Load( reader, i );

				int amount = reader.LoadInt( string.Format( "ItemPickupAmount{0}", i ) );
				
				UsedItemPickup pickup = ResourceCache.GetScene( "res://scenes/interactables/used_item_pickup.tscn" ).Instantiate<UsedItemPickup>();
				pickup.Entity = ammo;
				pickup.Amount = amount;
				Items.Add( pickup );
				break; }
			case "Consumable": {
				ConsumableEntity consumable = new ConsumableEntity();
				//				consumable.Load( reader, i );

				int amount = reader.LoadInt( string.Format( "ItemPickupAmount{0}", i ) );

				UsedItemPickup pickup = ResourceCache.GetScene( "res://scenes/interactables/used_item_pickup.tscn" ).Instantiate<UsedItemPickup>();
				pickup.Entity = consumable;
				pickup.Amount = amount;
				Items.Add( pickup );
				break; }
			};
		}
	}
	public void Save() {
		using var writer = new SaveSystem.SaveSectionWriter( "ItemNodeCache" );

		writer.SaveInt( "Count", Items.Count );

		int index = 0;
		foreach ( var item in Items ) {
			writer.SaveString( string.Format( "ItemPickupName{0}", index ), item.Name );
			if ( item.Entity is WeaponEntity weapon && weapon != null ) {
				writer.SaveString( string.Format( "ItemPickupType{0}", index ), "Weapon" );
				weapon.Save( writer, index );
			} else if ( item.Entity is AmmoEntity ammo && ammo != null ) {
				writer.SaveString( string.Format( "ItemPickupType{0}", index ), "Ammo" );
				writer.SaveInt( string.Format( "ItemPickupAmount{0}", index ), item.Amount );
			} else if ( item.Entity is ConsumableEntity consumable && consumable != null ) {
				writer.SaveString( string.Format( "ItemPickupType{0}", index ), "Consumable" );
				writer.SaveInt( string.Format( "ItemPickupAmount{0}", index ), item.Amount );
			}
			index++;
		}
	}
	public override void _Ready() {
		base._Ready();

		Instance = this;

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		} else {
			Items = new HashSet<UsedItemPickup>();
		}
	}

	public static void AddNode( UsedItemPickup pickup ) {
		Instance.Items.Add( pickup );
	}
};