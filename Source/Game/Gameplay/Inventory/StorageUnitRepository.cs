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
using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Gameplay.Items;
using Nomad.Game.Content.Catalogs.Items;
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Sdk.Items;

namespace Nomad.Game.Gameplay.Inventory
{
	/*
	===================================================================================

	StorageUnitRepository

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class StorageUnitRepository
	{
		private readonly Dictionary<InternString, IStorageUnit> _inventories = new();
		private readonly ItemCatalog _itemCatalog;
		private readonly ItemInstanceRepository _instanceRepository;
		private readonly IGameEventRegistryService _eventFactory;

		/*
		===============
		StorageUnitRepository
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemCatalog"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public StorageUnitRepository( ItemCatalog itemCatalog, ItemInstanceRepository instanceRepository, IGameEventRegistryService eventFactory )
		{
			_itemCatalog = itemCatalog ?? throw new ArgumentNullException( nameof( itemCatalog ) );
			_instanceRepository = instanceRepository ?? throw new ArgumentNullException( nameof( instanceRepository ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
		}

		/*
		===============
		TryAddInventory
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="displayName"></param>
		/// <param name="rules"></param>
		/// <param name="type"></param>
		/// <param name="inventory"></param>
		/// <returns></returns>
		public bool TryAddInventory(
			StorageUnitDefinition definition,
			out IStorageUnit? inventory
		)
		{
			if ( _inventories.ContainsKey( definition.Id ) ) {
				inventory = null;
				return false;
			}

			inventory = new StorageUnit(
				definition,
				_itemCatalog,
				_instanceRepository,
				_eventFactory
			);

			_inventories[definition.Id] = inventory;

			return true;
		}

		/*
		===============
		TryGetInventory
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="inventory"></param>
		/// <returns></returns>
		public bool TryGetInventory( InternString id, out IStorageUnit inventory )
		{
			if ( _inventories.TryGetValue( id, out inventory ) ) {
				return true;
			}

			return false;
		}
	};
};
