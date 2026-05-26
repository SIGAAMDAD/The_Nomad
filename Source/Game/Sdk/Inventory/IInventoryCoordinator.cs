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

namespace Nomad.Game.Sdk.Inventory
{
	/// <summary>
	///
	/// </summary>
	public interface IInventoryCoordinator : IDisposable
	{
		/// <summary>
		///
		/// </summary>
		IReadOnlyCollection<IStorageUnit> StorageUnits { get; }

		/// <summary>
		///
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		bool TryTakeItems( ItemDefinitionId itemId, int amount );

		/// <summary>
		///
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		bool TryGiveItems( ItemDefinitionId itemId, int amount );

		/// <summary>
		///
		/// </summary>
		/// <param name="instanceId"></param>
		/// <returns></returns>
		bool TryTakeInstance( ItemInstanceId instanceId );

		/// <summary>
		///
		/// </summary>
		/// <param name="instanceId"></param>
		/// <returns></returns>
		bool TryGiveInstance( ItemInstanceId instanceId );
	}
}
