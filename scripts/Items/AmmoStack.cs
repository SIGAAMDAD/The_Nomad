using Godot;

public partial class AmmoStack : Node {
	public AmmoEntity AmmoType;

	public int Amount = 0;
	public string ItemId;

	public void SetType( AmmoEntity ammo ) {
		AmmoType = ammo;
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