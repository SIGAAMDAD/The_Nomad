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
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;

namespace Nomad.Game.Domain.Interfaces.Player.Inventory
{
	/// <summary>
	///
	/// </summary>
	public interface IWeaponSlotService : IDisposable
	{
		WeaponSlotIndex Current { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player" )]
		[EventPayload( "PreviousSlot", typeof( WeaponSlotIndex ), Order = 1 )]
		[EventPayload( "CurrentSlot", typeof( WeaponSlotIndex ), Order = 2 )]
		IGameEvent<WeaponSlotChangedEventArgs> WeaponSlotChanged { get; }

		/// <summary>
		///
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		bool IsOccupied( WeaponSlotIndex slot );

		/// <summary>
		///
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="weapon"></param>
		/// <returns></returns>
		bool TryGetSlot( WeaponSlotIndex slot, out ItemInstanceId weapon );

		/// <summary>
		///
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="weapon"></param>
		/// <returns></returns>
		bool TrySetSlot( WeaponSlotIndex slot, ItemInstanceId weapon );
	};
};
