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
using System;
using System.Runtime.CompilerServices;
using PlayerSystem.Inventory;

public partial class Player {
	public sealed partial class WeaponSlotManager : GodotObject {
		public static readonly WeaponSlotIndex MAX_WEAPON_SLOTS = WeaponSlotIndex.Count;

		public WeaponSlotIndex CurrentWeapon { get; private set; } = WeaponSlotIndex.Invalid;

		public WeaponSlot this[ WeaponSlotIndex index ] {
			get {
				return WeaponSlots[ (int)index ];
			}
		}

		private WeaponSlot[]? WeaponSlots;
		private Player? Owner;

		public WeaponSlotManager( Player? owner ) {
			ArgumentNullException.ThrowIfNull( owner );

			Owner = owner;
			WeaponSlots = new WeaponSlot[ (int)MAX_WEAPON_SLOTS ];

			for ( int i = 0; i < (int)MAX_WEAPON_SLOTS; i++ ) {
				WeaponSlots[ i ] = new WeaponSlot( i );
			}
		}

		/*
		===============
		WeaponSlotManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private WeaponSlotManager() {
		}

		/*
		===============
		Save
		===============
		*/
		public void Save( SaveSystem.SaveSectionWriter writer ) {
			writer.SaveInt( nameof( CurrentWeapon ), (int)CurrentWeapon );
		}

		/*
		===============
		Load
		===============
		*/
		public void Load( SaveSystem.SaveSectionReader reader ) {
			ArgumentNullException.ThrowIfNull( Owner );

			CurrentWeapon = (WeaponSlotIndex)reader.LoadInt( nameof( CurrentWeapon ) );
			if ( CurrentWeapon != WeaponSlotIndex.Invalid ) {
				Owner.CallDeferred( MethodName.EmitSignal, Player.SignalName.SwitchedWeapon, WeaponSlots[ (int)CurrentWeapon ].Weapon );
			}
		}

		/*
		===============
		SetEquippedWeapon
		===============
		*/
		/// <summary>
		/// Sets the currently equipped WeaponSlot
		/// </summary>
		/// <param name="weaponSlot">Which <see cref="WeaponSlotIndex"/> to use</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="weaponSlot"/> is not a valid <see cref="WeaponSlotIndex"/></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetEquippedWeapon( WeaponSlotIndex weaponSlot ) {
			if ( weaponSlot < 0 || weaponSlot >= WeaponSlotIndex.Count ) {
				throw new ArgumentOutOfRangeException( $"weaponSlot is invalid ({weaponSlot}), it must be a valid WeaponSlotIndex" );
			}
			CurrentWeapon = weaponSlot;
		}

		/*
		===============
		SetPrimaryWeapon
		===============
		*/
		/// <summary>
		/// Sets the <see cref="WeaponEntity"/> in the <see cref="WeaponSlotIndex.HeavyPrimary"/> slot
		/// </summary>
		/// <param name="weapon">The weapon to use</param>
		/// <exception cref="ArgumentNullException">Thrown if weapon is null</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetPrimaryWeapon( in WeaponEntity weapon ) {
			ArgumentNullException.ThrowIfNull( weapon );
			WeaponSlots[ (int)WeaponSlotIndex.Primary ].SetWeapon( weapon );
		}

		/*
		===============
		SetHeavyPrimaryWeapon
		===============
		*/
		/// <summary>
		/// Sets the <see cref="WeaponEntity"/> in the <see cref="WeaponSlotIndex.HeavyPrimary"/> slot
		/// </summary>
		/// <param name="weapon">The weapon to use</param>
		/// <exception cref="ArgumentNullException">Thrown if weapon is null</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetHeavyPrimaryWeapon( in WeaponEntity weapon ) {
			ArgumentNullException.ThrowIfNull( weapon );
			WeaponSlots[ (int)WeaponSlotIndex.HeavyPrimary ].SetWeapon( weapon );
		}

		/*
		===============
		SetSidearmWeapon
		===============
		*/
		/// <summary>
		/// Sets the <see cref="WeaponEntity"/> in the <see cref="WeaponSlotIndex.Sidearm"/> slot
		/// </summary>
		/// <param name="weapon">The weapon to use</param>
		/// <exception cref="ArgumentNullException">Thrown if weapon is null</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetSidearmWeapon( in WeaponEntity weapon ) {
			ArgumentNullException.ThrowIfNull( weapon );
			WeaponSlots[ (int)WeaponSlotIndex.Sidearm ].SetWeapon( weapon );
		}

		/*
		===============
		SetHeavySidearmWeapon
		===============
		*/
		/// <summary>
		/// Sets the <see cref="WeaponEntity"/> in the <see cref="WeaponSlotIndex.HeavySidearm"/> slot
		/// </summary>
		/// <param name="weapon">The weapon to use</param>
		/// <exception cref="ArgumentNullException">Thrown if weapon is null</exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetHeavySidearmWeapon( in WeaponEntity weapon ) {
			WeaponSlots[ (int)WeaponSlotIndex.HeavySidearm ].SetWeapon( weapon );
		}

		/*
		===============
		GetPrimaryWeapon
		===============
		*/
		/// <summary>
		/// Fetches the currently equipped weapon in the "Primary" slot
		/// </summary>
		/// <returns>The <see cref="WeaponSlot"/> object within the <see cref="WeaponSlotIndex.Primary"/> slot</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public WeaponSlot? GetPrimaryWeapon() {
			return WeaponSlots[ (int)WeaponSlotIndex.Primary ];
		}

		/*
		===============
		GetHeavyPrimaryWeapon
		===============
		*/
		/// <summary>
		/// Fetches the currently equipped weapon in the "Heavy Primary" slot
		/// </summary>
		/// <returns>The <see cref="WeaponSlot"/> object within the <see cref="WeaponSlotIndex.HeavyPrimary"/> slot</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public WeaponSlot? GetHeavyPrimaryWeapon() {
			return WeaponSlots[ (int)WeaponSlotIndex.HeavyPrimary ];
		}

		/*
		===============
		GetSidearmWeapon
		===============
		*/
		/// <summary>
		/// Fetches the currently equipped weapon in the "Sidearm" slot
		/// </summary>
		/// <returns>The <see cref="WeaponSlot"/> object within the <see cref="WeaponSlotIndex.Sidearm"/> slot</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public WeaponSlot? GetSidearmWeapon() {
			return WeaponSlots[ (int)WeaponSlotIndex.Sidearm ];
		}

		/*
		===============
		GetHeavySidearmWeapon
		===============
		*/
		/// <summary>
		/// Fetches the currently equipped weapon in the "Heavy Sidearm" slot
		/// </summary>
		/// <returns>The <see cref="WeaponSlot"/> object within the <see cref="WeaponSlotIndex.HeavySidearm"/> slot</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public WeaponSlot? GetHeavySidearmWeapon() {
			return WeaponSlots[ (int)WeaponSlotIndex.HeavySidearm ];
		}
	};
};