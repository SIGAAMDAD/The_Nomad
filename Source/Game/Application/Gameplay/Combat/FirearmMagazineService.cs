/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Sdk.Combat;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Inventory;

namespace Nomad.Game.Application.Gameplay.Combat
{
	/*
	===================================================================================

	FirearmMagazineService

	===================================================================================
	*/
	/// <summary>
	/// Owns mutable magazine state for a firearm instance.
	/// </summary>
	/// <remarks>
	/// The service tracks the selected ammunition definition and the number of
	/// rounds currently loaded. It does not resolve damage, range, velocity, or
	/// recoil; those values stay on <see cref="AmmoDefinition"/> and are read
	/// from <see cref="LoadedAmmo"/> by combat systems.
	/// </remarks>
	internal sealed class FirearmMagazineService
	{
		public bool CanUse => _ammoCount > 0;
		public AmmoDefinition? LoadedAmmo => _ammoDefinition;

		private readonly IFirearmInstance _instance;

		/// <summary>
		/// The current number of bullets loaded into the firearm.
		/// </summary>
		private int _ammoCount;
		private AmmoDefinition? _ammoDefinition;

		public FirearmMagazine Snapshot => new FirearmMagazine(
			_instance.Stats.MagazineSize,
			_ammoCount
		);

		public FirearmMagazineService( IFirearmInstance instance )
		{
			_instance = instance ?? throw new ArgumentNullException( nameof( instance ) );
		}

		/*
		===============
		TrySetAmmo
		===============
		*/
		/// <summary>
		/// Selects the ammunition definition used by subsequent reloads.
		/// </summary>
		/// <param name="ammoDefinition">Ammo definition to select.</param>
		/// <returns>
		/// True when the firearm accepts the ammo type and supports the ammo's
		/// projectile modifier.
		/// </returns>
		public bool TrySetAmmo( AmmoDefinition ammoDefinition )
		{
			ArgumentGuard.ThrowIfNull( ammoDefinition, nameof( ammoDefinition ) );

			if ( !_instance.FirearmDefinition.AcceptsAmmo( ammoDefinition ) ) {
				return false;
			}

			if ( _ammoCount > 0 && _ammoDefinition != null && _ammoDefinition.Id != ammoDefinition.Id ) {
				return false;
			}

			switch ( ammoDefinition.Modifier ) {
				case AmmoModifier.ArmorPiercing:
					if ( !_instance.FirearmDefinition.Flags.HasFlag( FirearmFlags.ArmorPiercingCapable ) ) {
						return false;
					}
					break;

				case AmmoModifier.Explosive:
					if ( !_instance.FirearmDefinition.Flags.HasFlag( FirearmFlags.ExplosiveAmmoCapable ) ) {
						return false;
					}
					break;

				case AmmoModifier.HollowPoint:
					if ( !_instance.FirearmDefinition.Flags.HasFlag( FirearmFlags.HollowPointCapable ) ) {
						return false;
					}
					break;

				case AmmoModifier.Incendiary:
					if ( !_instance.FirearmDefinition.Flags.HasFlag( FirearmFlags.IncendiaryCapable ) ) {
						return false;
					}
					break;

				case AmmoModifier.Subsonic:
					if ( !_instance.FirearmDefinition.Flags.HasFlag( FirearmFlags.SubsonicCapable ) ) {
						return false;
					}
					break;

				case AmmoModifier.None:
					break;

				default:
					throw new IndexOutOfRangeException( nameof( ammoDefinition.Modifier ) );
			}

			_ammoDefinition = ammoDefinition;
			return true;
		}

		/// <summary>
		/// Attempts to load rounds from inventory into the magazine.
		/// </summary>
		/// <param name="inventory">Inventory containing a stack of the selected ammo.</param>
		/// <param name="roundsLoaded">Number of rounds successfully loaded.</param>
		/// <returns>True when one or more rounds were loaded.</returns>
		public bool TryReload( IStorageUnit inventory, out int roundsLoaded )
		{
			roundsLoaded = 0;
			ArgumentGuard.ThrowIfNull( inventory, nameof( inventory ) );

			if ( _ammoDefinition == null ) {
				return false;
			}

			int reloadAmount = Math.Max( 0, _instance.Stats.MagazineSize - _ammoCount );
			if ( reloadAmount == 0 ) {
				return false;
			}

			for ( int amount = reloadAmount; amount > 0; amount-- ) {
				if ( inventory.TryRemove( _ammoDefinition.Id, amount ) ) {
					_ammoCount += amount;
					roundsLoaded = amount;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Consumes a single loaded round.
		/// </summary>
		/// <returns>True when a round was available and consumed.</returns>
		public bool TryConsumeRound()
		{
			if ( _ammoCount <= 0 ) {
				return false;
			}

			_ammoCount--;
			return true;
		}
	};
};
