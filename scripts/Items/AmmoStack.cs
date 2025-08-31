using Godot;
using System;
using Interactables;

public partial class AmmoStack : Node, IConsumableStack {
	public Ammo AmmoType { get; private set; }
	public int Amount { get; private set; }

	public AmmoStack( Resource? ammoType, int amount ) {
		ArgumentNullException.ThrowIfNull( ammoType );

		AmmoType = new Ammo( ammoType );
		Amount = amount;
	}
	public AmmoStack( string itemPath ) {
		CallDeferred( MethodName.InitFromNode, itemPath );
	}

	/*
	===============
	InitFromNode
	===============
	*/
	private void InitFromNode( string path ) {
		ItemPickup? pickup = null;

		try {
			pickup = GetNode<ItemPickup>( path );
			if ( pickup == null ) {
				Console.PrintError( $"AmmoStack.InitFromNode: node {path} isn't a valid node" );
				QueueFree();
				return;
			}
		} catch ( InvalidCastException ) {
			Console.PrintError( $"AmmoStack.InitFromNode: node {path} isn't an ItemPickup node (InvalidCastException)" );
			QueueFree();
			return;
		}

		AmmoType = new Ammo( pickup.Data );
		Amount = pickup.Amount;
	}

	/*
	===============
	AddItems
	===============
	*/
	public void AddItems( int items ) {
		Amount += items;
	}

	/*
	===============
	RemoveItems
	===============
	*/
	public int RemoveItems( int items ) {
		if ( Amount - items < 0 ) {
			int tmp = Amount;
			Amount = 0;
			return tmp;
		}

		Amount -= items;
		return items;
	}
};