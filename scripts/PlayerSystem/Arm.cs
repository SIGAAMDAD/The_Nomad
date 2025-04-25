using Godot;

namespace PlayerSystem {
	public partial class Arm : Node {
		[Export]
		public AnimatedSprite2D Animations;
		public Player Parent;

		private SpriteFrames DefaultAnimation;
		private int Slot = -1;

		public bool Flip = false;

        public override void _Ready() {
            base._Ready();

			Parent = GetParent<Player>();
			DefaultAnimation = Animations.SpriteFrames;
        }

		public void SetWeapon( int slot ) {
			Slot = slot;
		}
		public int GetSlot() {
			return Slot;
		}

		public SpriteFrames GetAnimationSet() {
			if ( Slot != WeaponSlot.INVALID ) {
				WeaponEntity weapon = Parent.GetSlot( Slot ).GetWeapon();
				if ( weapon != null ) {
//					WeaponEntity.Properties mode = weapon.GetLastUsedMode();
					return Flip ? weapon.GetFramesLeft() : weapon.GetFramesRight();
					/*
					if ( ( mode & WeaponEntity.Properties.IsFirearm ) != 0 ) {
						return Flip ? weapon.GetFirearmFramesLeft() : weapon.GetFirearmFramesRight();
					} else if ( ( mode & WeaponEntity.Properties.IsBlunt ) != 0 ) {
						return Flip ? weapon.GetBluntFramesLeft() : weapon.GetBluntFramesRight();
					} else if ( ( mode & WeaponEntity.Properties.IsBladed ) != 0 ) {
						return Flip ? weapon.GetBladedFramesLeft() : weapon.GetBladedFramesRight();
					}
					*/
				}
			}
			return DefaultAnimation;
		}
    };
};