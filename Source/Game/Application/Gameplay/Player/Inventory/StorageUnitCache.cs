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
using Nomad.Game.Application.Gameplay.Inventory;
using Nomad.Game.Domain.Data.Inventory;
using Nomad.Game.Domain.Interfaces.Items;
using Nomad.Game.Domain.Interfaces.Player.Inventory;

namespace Nomad.Game.Application.Gameplay.Player.Inventory
{
	/*
	===================================================================================

	StorageUnitService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class StorageUnitService
	{
		private readonly Dictionary<InternString, IStorageUnit> _inventories = new();
		private readonly IItemCatalog _itemCatalog;
		private readonly IGameEventRegistryService _eventFactory;

		/*
		===============
		StorageUnitService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemCatalog"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public StorageUnitService( IItemCatalog itemCatalog, IGameEventRegistryService eventFactory )
		{
			_itemCatalog = itemCatalog ?? throw new ArgumentNullException( nameof( itemCatalog ) );
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
