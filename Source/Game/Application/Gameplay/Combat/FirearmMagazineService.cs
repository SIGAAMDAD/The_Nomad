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
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Combat;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Interfaces.Items;
using Nomad.Game.Domain.Interfaces.Inventory;

namespace Nomad.Game.Application.Gameplay.Combat
{
	/*
	===================================================================================

	FirearmMagazineService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class FirearmMagazineService
	{
		public bool CanUse => _ammoCount > 0;

		private readonly IFirearmInstance _instance;

		/// <summary>
		/// The current number of bullets loaded into the firearm.
		/// </summary>
		private int _ammoCount = 0;
		private AmmoDefinition _ammoDefinition;

		public FirearmMagazine Snapshot => new FirearmMagazine(
			_instance.Stats.MagazineSize,
			_ammoCount
		);

		public FirearmMagazineService( AmmoDefinition ammoDefinition, IFirearmInstance instance, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			TrySetAmmo( ammoDefinition );
			_instance = instance ?? throw new ArgumentNullException( nameof( instance ) );
		}

		/*
		===============
		TrySetAmmo
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="ammoDefinition"></param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public bool TrySetAmmo( AmmoDefinition ammoDefinition )
		{
			ArgumentGuard.ThrowIfNull( ammoDefinition, nameof( ammoDefinition ) );

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

			return true;
		}

		public bool TryReload( IStorageUnit inventory )
		{
			int reloadAmount = _instance.Stats.MagazineSize + 1 - _ammoCount;

			if ( !inventory.TryRemove( _ammoDefinition.Id, reloadAmount ) ) {
				return false;
			}

			return true;
		}
	};
};
