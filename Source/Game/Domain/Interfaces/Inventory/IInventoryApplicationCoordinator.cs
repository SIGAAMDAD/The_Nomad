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
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Events.Items;

namespace Nomad.Game.Domain.Interfaces.Inventory
{
	public interface IInventoryApplicationCoordinator : IDisposable
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Items", PayloadName = "ItemPickupRequestedEventArgs" )]
		[EventPayload( "PlayerId", typeof( PlayerId ), Order = 1 )]
		[EventPayload( "ItemId", typeof( ItemDefinitionId ), Order = 2 )]
		[EventPayload( "Amount", typeof( int ), Order = 3 )]
		[EventPayload( "PickupEntityId", typeof( EntityId ), Order = 4 )]
		IGameEvent<ItemPickupRequestedEventArgs> ItemPickupRequested { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Items", PayloadName = "ItemPickupCompletedEventArgs" )]
		[EventPayload( "PlayerId", typeof( PlayerId ), Order = 1 )]
		[EventPayload( "ItemId", typeof( ItemDefinitionId ), Order = 2 )]
		[EventPayload( "Amount", typeof( int ), Order = 3 )]
		[EventPayload( "PickupEntityId", typeof( EntityId ), Order = 4 )]
		[EventPayload( "Success", typeof( bool ), Order = 5 )]
		IGameEvent<ItemPickupCompletedEventArgs> ItemPickupCompleted { get; }
	};
};
