using Godot;

namespace PlayerSystem {
	public partial class ConsumableStack : Node {
		public Resource ItemType;

		public int Amount = 0;
		public string ItemId;

		public void SetType( ConsumableEntity consumable ) {
			ItemType = consumable.Data;
			ItemId = (string)ItemType.Get( "id" );
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
};