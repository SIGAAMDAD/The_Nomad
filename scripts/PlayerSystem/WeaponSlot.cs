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

	WeaponSlot

	===================================================================================
	*/
	
	public partial class WeaponSlot : GodotObject {
		/// <summary>
		/// Represents an invalid slot
		/// </summary>
		public readonly static int INVALID = -1;

		public WeaponEntity? Weapon { get; private set; } = null;
		public int Index { get; private set; } = 0;
		public WeaponEntity.Properties Mode { get; private set; } = WeaponEntity.Properties.None;

		/*
		===============
		SetWeapon
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetWeapon( in WeaponEntity? weapon ) {
			Weapon = weapon;
		}

		/*
		===============
		SetMode
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetMode( WeaponEntity.Properties mode ) {
			Mode = mode;
		}

		/*
		===============
		SetIndex
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetIndex( int index ) {
			Index = index;
		}

		/*
		===============
		IsUsed
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool IsUsed() {
			return Weapon != null;
		}
	};
};