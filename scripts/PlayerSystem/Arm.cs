/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using Items;
using PlayerSystem.Inventory;
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
		public WeaponSlotIndex Slot { get; private set; } = WeaponSlotIndex.Invalid;

		public bool Flip { get; set; } = false;

		/*
		===============
		SetSlot
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetSlot( WeaponSlotIndex slot ) {
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
			if ( other.Slot != WeaponSlotIndex.Invalid ) {
				(Slot, other.Slot) = (other.Slot, Slot);
			} else {
				// if we have nothing in the destination hand, then just clear the source hand
				(Slot, other.Slot) = (WeaponSlotIndex.Invalid, Slot);
			}
		}

		/*
		===============
		GetAnimationSet
		===============
		*/
		public SpriteFrames? GetAnimationSet() {
			if ( Slot != WeaponSlotIndex.Invalid ) {
				WeaponEntity? weapon = Parent.WeaponSlots[ Slot ].Weapon;
				if ( weapon != null ) {
					if ( weapon.IsFirearm() && weapon.CurrentState == WeaponEntity.WeaponState.Use && weapon is WeaponFirearm firearm ) {
						if ( firearm.CurrentRecoilOffset == Vector2.Zero ) {
							CreateTween().TweenProperty( Animations, "offset", Vector2.Zero, 0.90f );
						} else {
							Animations.Offset = firearm.CurrentRecoilOffset;
						}
						Animations.Rotation += Mathf.DegToRad( firearm.CurrentRecoilRotation );
					}

					bool oneHanded = weapon.IsOneHanded();

					// if we're running one-handed, we want the same animation frames, just the "flip variant",
					// but if we're running default (two handed weapon) then determine animation frames based
					// on orientation
					return Hand == Player.Hands.Right ?
						oneHanded ? weapon.FramesRight :
							Animations.FlipV ? weapon.FramesLeft : weapon.FramesRight
						:
						oneHanded ? weapon.FramesLeft :
							Animations.FlipV ? weapon.FramesRight : weapon.FramesLeft;
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