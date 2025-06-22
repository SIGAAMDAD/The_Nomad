/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

using Godot;

namespace PlayerSystem {
	public partial class Arm : Node {
		[Export]
		public AnimatedSprite2D Animations;
		public Player Parent;

		private SpriteFrames DefaultAnimation;
		public int Slot = WeaponSlot.INVALID;

		public bool Flip = false;

        public override void _Ready() {
            base._Ready();

			Parent = GetParent<Player>();
			DefaultAnimation = Animations.SpriteFrames;
        }

		public SpriteFrames GetAnimationSet() {
			if ( Slot != WeaponSlot.INVALID ) {
				WeaponEntity weapon = Parent.GetSlot( Slot ).GetWeapon();
				if ( weapon != null ) {
//					WeaponEntity.Properties mode = weapon.GetLastUsedMode();
					return Flip ? weapon.AnimationsLeft : weapon.AnimationsRight;
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