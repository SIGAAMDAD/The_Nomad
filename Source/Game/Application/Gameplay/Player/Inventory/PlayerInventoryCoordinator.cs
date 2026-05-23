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

using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Inventory;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Data.Player.Inventory;
using Nomad.Game.Domain.Interfaces.Items;
using Nomad.Game.Domain.Interfaces.Player.Stats;
using Nomad.Game.Domain.Interfaces.Player.State;
using Nomad.Game.Domain.Interfaces.Player.Inventory;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Interfaces.Inventory;
using Nomad.Game.Application.Gameplay.Inventory;

namespace Nomad.Game.Application.Gameplay.Player.Inventory
{
	internal sealed class PlayerInventoryCoordinator : InventoryCoordinator, IPlayerInventoryCoordinator
	{
		public IBackpackService Backpack => _backpack;
		private readonly BackpackService _backpack;

		public IWeaponSlotService WeaponSlots => null;

		private readonly StorageUnitRepository _storageUnitRepository;
		private readonly PlayerWeaponSlotService _slotService;

		public PlayerInventoryCoordinator( IPlayerBaseStatsRepository baseStatsRepository, IPlayerStateReader stateReader, IGameEventRegistryService eventFactory, IItemCatalog itemCatalog )
		{
			_storageUnitRepository = new StorageUnitRepository( itemCatalog, eventFactory );

			if ( _storageUnitRepository.TryAddInventory(
				new StorageUnitDefinition {
					Id = new InternString( "INVENTORY_PLAYER_BACKPACK" ),
					DisplayName = new InternString( "INVENTORY_PLAYER_BACKPACK_NAME" ),
					Rules = new InventoryRules {
						MaxWeight = baseStatsRepository.GetBaseStatValue( BaseStatType.EncumbranceThreshold ),
						IgnoreWeight = false,
						AcceptsItem = _ => true
					},
					Type = InventoryContainerType.Backpack,
				},
				out var backpackStorage
			) ) {
				_backpack = new BackpackService( BackpackStatus.Equipped, stateReader, backpackStorage, eventFactory );
			}
		}

		public void Dispose()
		{
			_slotService.Dispose();
		}

		public bool TryUnequipBackpack()
		{
			return _backpack.TryUnequip();
		}

		public bool TryTakeItems( ItemDefinitionId itemId, int amount )
		{
			throw new System.NotImplementedException();
		}

		public bool TryGiveItems( ItemDefinitionId itemId, int amount )
		{
			throw new System.NotImplementedException();
		}

		public bool TryGetFirearm( ItemInstanceId itemId, out FirearmDefinition firearm )
		{
			throw new System.NotImplementedException();
		}
	};
};
