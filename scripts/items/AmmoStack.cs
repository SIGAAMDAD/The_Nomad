using GDExtension.Wrappers;
using Godot;

public partial class AmmoStack : Node {
	public Resource AmmoType;

	public int Amount = 0;
	public string ItemId;

	public void SetType( AmmoEntity ammo ) {
		AmmoType = ammo.Data;
		ItemId = (string)AmmoType.Get( "id" );
	}
	public void AddItems( int nItems ) {
		Amount += nItems;
	}
	public int RemoveItems( int nItems ) {
		if ( Amount - nItems < 0 ) {
			int tmp = Amount;
			Amount = 0;
			return tmp;
		}

		Amount -= nItems;
		return nItems;
	}
};