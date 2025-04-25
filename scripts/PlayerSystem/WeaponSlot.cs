using Godot;

namespace PlayerSystem {
	public class WeaponSlot {
		public static int INVALID = -1;

		private WeaponEntity Weapon = null;
		private int Index = 0;
		private WeaponEntity.Properties Mode = WeaponEntity.Properties.None;

		public WeaponEntity GetWeapon() {
			return Weapon;
		}
		public void SetWeapon( WeaponEntity weapon ) {
			Weapon = weapon;
		}
		public WeaponEntity.Properties GetMode() {
			return Mode;
		}
		public void SetMode( WeaponEntity.Properties mode ) {
			Mode = mode;
		}
		public void SetIndex( int nIndex ) {
			Index = nIndex;
		}

		public bool IsUsed() {
			return Weapon != null;
		}
	};
};