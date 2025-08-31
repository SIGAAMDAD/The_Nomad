/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using System.Runtime.CompilerServices;

namespace PlayerSystem {
	/*
	===================================================================================
	
	Arm
	
	manages the player's arms
	
	===================================================================================
	*/
	
	public sealed partial class Arm : Node {
		[Export]
		public AnimatedSprite2D? Animations;
		[Export]
		private Player.Hands Hand;
		public Player? Parent { get; private set; }

		public SpriteFrames? DefaultAnimation { get; private set; }
		public int Slot { get; private set; } = WeaponSlot.INVALID;

		public bool Flip { get; set; } = false;

		/*
		===============
		SetSlot
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetSlot( int slot ) {
			Slot = slot;
		}

		/*
		===============
		SwapSlot
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SwapSlot( Arm other ) {
			// check if the destination hand has something in it, if true, then swap
			if ( other.Slot != WeaponSlot.INVALID ) {
				(Slot, other.Slot) = (other.Slot, Slot);
			} else {
				// if we have nothing in the destination hand, then just clear the source hand
				(Slot, other.Slot) = (WeaponSlot.INVALID, Slot);
			}
		}

		/*
		===============
		GetAnimationSet
		===============
		*/
		public SpriteFrames? GetAnimationSet() {
			if ( Slot != WeaponSlot.INVALID ) {
				WeaponEntity? weapon = Parent.Inventory.WeaponSlots[ Slot ].Weapon;
				if ( weapon != null ) {
					if ( weapon.IsFirearm() && weapon.CurrentState == WeaponEntity.WeaponState.Use ) {
						if ( weapon.CurrentRecoilOffset == Vector2.Zero ) {
							CreateTween().TweenProperty( Animations, "offset", Vector2.Zero, 0.90f );
						} else {
							Animations.Offset = weapon.CurrentRecoilOffset;
						}
						Animations.Rotation += Mathf.DegToRad( weapon.CurrentRecoilRotation );
					}

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
				} else {
					throw new System.Exception( $"Arm {Hand} has weapon slot {Slot} but the weapon slot is empty" );
				}
			}
			return DefaultAnimation;
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			Parent = GetParent<Player>();
			DefaultAnimation = Animations.SpriteFrames;
		}
	};
};