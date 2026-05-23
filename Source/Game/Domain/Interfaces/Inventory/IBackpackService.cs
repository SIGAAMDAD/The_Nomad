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
using System.Numerics;
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Inventory;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Data.Player.Inventory;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Items;

namespace Nomad.Game.Domain.Interfaces.Inventory
{
	/// <summary>
	///
	/// </summary>
	public interface IBackpackService : IItemInstance<ItemDefinition>, IStorageUnit, IDisposable
	{
		Vector2 DroppedOrigin { get; }

		BackpackLocationKind LocationKind { get; }
		EntityId LocationEntityId { get; }

		IStorageUnit Storage { get; }

		BackpackStatus Status { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "BackpackStatusChangedEventArgs" )]
		[EventPayload( "PreviousStatus", typeof( BackpackStatus ), Order = 1 )]
		[EventPayload( "CurrentStatus", typeof( BackpackStatus ), Order = 2 )]
		IGameEvent<BackpackStatusChangedEventArgs> StateChanged { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "BackpackUnequipRequestedEventArgs" )]
		IGameEvent<BackpackUnequipRequestedEventArgs> UnequipRequested { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "BackpackUnequippedEventArgs" )]
		IGameEvent<BackpackUnequippedEventArgs> Unequipped { get; }

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		bool TryEquip();

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		bool TryUnequip();
	};
};
