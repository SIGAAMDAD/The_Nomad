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
using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Player.Inventory;

namespace Nomad.Game.Sdk.Items
{
	/// <summary>
	///
	/// </summary>
	public abstract record WeaponDefinition : ItemDefinition
	{
		public abstract WeaponType WeaponType { get; }
		public float BaseDurability { get; init; }
		public WeaponSlotIndex Slot { get; init; }

		protected virtual WeaponDefinition LoadBase( JsonElement json )
		{
			return this with {
				BaseDurability = json.GetRequired<float>( nameof( BaseDurability ) ),
				Slot = Enum.Parse<WeaponSlotIndex>( json.GetRequired<string>( nameof( Slot ) ) )
			};
		}
	}
}
