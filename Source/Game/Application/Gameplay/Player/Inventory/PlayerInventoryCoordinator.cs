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
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Player.Stats;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Application.Gameplay.Inventory;
using Nomad.Game.Sdk.Multiplayer;
using Godot;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player.Inventory
{
	/*
	===================================================================================

	PlayerInventoryCoordinator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerInventoryCoordinator : InventoryCoordinator, IPlayerInventoryCoordinator
	{
		public IBackpackService Backpack => _backpack;
		private readonly BackpackService _backpack;

		public IWeaponSlotService WeaponSlots => _slotService;
		private readonly IWeaponSlotService _slotService;

		private readonly StorageUnitRepository _storageUnitRepository;

		public PlayerInventoryCoordinator( PlayerId playerId, IPlayerBaseStatsRepository baseStatsRepository, IPlayerStateReader stateReader, IGameEventRegistryService eventFactory, IItemCatalog itemCatalog )
		{
			_storageUnitRepository = new StorageUnitRepository( itemCatalog, eventFactory );

			if ( !_storageUnitRepository.TryAddInventory(
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
				throw new System.InvalidOperationException( "Failed to create player backpack storage." );
			}

			backpackStorage = backpackStorage ?? throw new System.InvalidOperationException( "Player backpack storage was not returned." );
			AddStorageUnit( backpackStorage );

			_backpack = new BackpackService(
				BackpackStatus.Equipped,
				stateReader,
				backpackStorage,

				// FIXME: don't manually load it here
				ResourceLoader.Load<PackedScene>( "res://Assets/Prefabs/Player/BackpackUnequipped/BackpackUnequipped.tscn" ).Instantiate<BackpackUnequippedPrefab>(),

				eventFactory
			);

			_slotService = new PlayerWeaponSlotService( playerId, eventFactory );
		}

		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			_slotService.Dispose();
		}

		public bool TryUnequipBackpack()
		{
			return _backpack.TryUnequip();
		}
	};
};
