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
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Inventory;
using Nomad.Game.Domain.Data.Items;

namespace Nomad.Game.Domain.Interfaces.Player.Inventory
{
	/// <summary>
	///
	/// </summary>
	public interface IStorageUnit : IDisposable
	{
		InternString Id { get; }
		InternString DisplayName { get; }

		InventoryContainerType Type { get; }
		InventoryRules Rules { get; }
		float CurrentWeight { get; }

		bool TryAdd( ItemDefinitionId itemType, int amount );
		bool TryRemove( ItemDefinitionId itemType, int amount );

		bool TryAddInstance( ItemInstanceId instance );
		bool TryRemoveInstance( ItemInstanceId instance );
		bool ContainsInstance( ItemInstanceId instance );
	};
};
