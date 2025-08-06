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
using System.Runtime.CompilerServices;

namespace PlayerSystem {
	public partial class WeaponSlot : GodotObject {
		public readonly static int INVALID = -1;

		private WeaponEntity Weapon = null;
		private int Index = 0;
		private WeaponEntity.Properties Mode = WeaponEntity.Properties.None;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public WeaponEntity GetWeapon() => Weapon;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetWeapon( WeaponEntity weapon ) => Weapon = weapon;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public WeaponEntity.Properties GetMode() => Mode;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetMode( WeaponEntity.Properties mode ) => Mode = mode;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetIndex( int nIndex ) => Index = nIndex;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool IsUsed() => Weapon != null;
	};
};