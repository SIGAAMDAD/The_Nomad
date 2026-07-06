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
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Inventory;

namespace Nomad.Game.Gameplay.Inventory
{
	internal class InventoryCoordinator : IInventoryCoordinator
	{
		public IReadOnlyCollection<IStorageUnit> StorageUnits => _units;
		private readonly List<IStorageUnit> _units = new();

		private bool _isDisposed = false;

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			Dispose( true );

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		protected virtual void Dispose( bool disposing )
		{
		}

		protected void AddStorageUnit( IStorageUnit storageUnit )
		{
			_units.Add( storageUnit ?? throw new ArgumentNullException( nameof( storageUnit ) ) );
		}

		public bool TryGetFirearm( ItemInstanceId itemId, out FirearmDefinition firearm )
		{
			throw new System.NotImplementedException();
		}

		public bool TryGiveItems( ItemDefinitionId itemId, int amount )
		{
			throw new System.NotImplementedException();
		}

		public bool TryTakeItems( ItemDefinitionId itemId, int amount )
		{
			throw new System.NotImplementedException();
		}

		public bool TryTakeInstance( ItemInstanceId instanceId )
		{
			throw new System.NotImplementedException();
		}

		public bool TryGiveInstance( ItemInstanceId instanceId )
		{
			throw new System.NotImplementedException();
		}
	};
};
