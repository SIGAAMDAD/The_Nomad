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
using Nomad.Core.Events;
using Nomad.Game.Application.Gameplay.Entity;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Events.Items;
using Nomad.Game.Domain.Interfaces.Inventory;
using Nomad.Game.Domain.Interfaces.Items;
using Nomad.Save.Interfaces;
using Nomad.Save.Services;

namespace Nomad.Game.Domain.Data.Items
{
	/*
	===================================================================================

	ItemInstance

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class ItemInstance : EntityBase, IItemInstance<ItemDefinition>
	{
		public ItemType BaseType => _definition.BaseType;

		public ItemInstanceId InstanceId => _instanceId;
		private readonly ItemInstanceId _instanceId;

		public ItemDefinition Definition => _definition;
		private readonly ItemDefinition _definition;

		public ItemStatus State => _status;
		private ItemStatus _status = ItemStatus.Dormant;

		public IStorageUnit? StorageUnit => _storageUnit;
		private IStorageUnit? _storageUnit = null;

		public int StackCount => _stackCount;
		private int _stackCount = 1;

		public IGameEvent<ItemStatusChangedEventArgs> StatusChanged => _statusChanged;
		private readonly IGameEvent<ItemStatusChangedEventArgs> _statusChanged = null;

		/*
		===============
		ItemInstance
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="instanceId"></param>
		/// <param name="definition"></param>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ItemInstance( ItemInstanceId instanceId, ItemDefinition definition, IGameEventRegistryService eventFactory )
			: base( new EntityId( instanceId.Value ), definition.Id.Value, definition.Name, EntityType.Item )
		{
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
			_instanceId = instanceId;

			_statusChanged = eventFactory.GetEvent<ItemStatusChangedEventArgs>(
				ItemStatusChangedEventArgs.Name,
				ItemStatusChangedEventArgs.NameSpace
			);
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}

			base.Dispose( disposing );

			_statusChanged.Dispose();
		}
	};
};
