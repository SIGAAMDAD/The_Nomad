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
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace PlayerSystem {
	public partial class Arm : Node {
		[Export]
		public AnimatedSprite2D Animations;
		[Export]
		private Player.Hands Hand;
		public Player Parent;

		public SpriteFrames DefaultAnimation { get; private set; }
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
					bool oneHanded = weapon.IsOneHanded();

					// if we're running one-handed, we want the same animation frames, just the "flip variant",
					// but if we're running default (two handed weapon) then determine animation frames based
					// on orientation
					return Hand == Player.Hands.Right ?
						oneHanded ? weapon.AnimationsRight :
							Animations.FlipV ? weapon.AnimationsLeft : weapon.AnimationsRight
						:
						oneHanded ? weapon.AnimationsLeft :
							Animations.FlipV ? weapon.AnimationsRight : weapon.AnimationsLeft;
				}
			}
			return DefaultAnimation;
		}
    };
};