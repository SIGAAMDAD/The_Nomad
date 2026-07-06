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
using Nomad.Game.Gameplay.Player;
using Nomad.Game.Sdk.Events.Items;
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Game.Sdk.Multiplayer;

namespace Nomad.Game.Gameplay.Inventory
{
	internal sealed class InventoryApplicationCoordinator : IInventoryApplicationCoordinator
	{
		private readonly IPlayerRuntimeRegistry _players;
		private readonly IDisposable _itemPickupRequested;

		private bool _isDisposed = false;

		public IGameEvent<ItemPickupRequestedEventArgs> ItemPickupRequested => _itemPickupRequestedEvent;
		private readonly IGameEvent<ItemPickupRequestedEventArgs> _itemPickupRequestedEvent = null;

		public IGameEvent<ItemPickupCompletedEventArgs> ItemPickupCompleted => _itemPickupCompletedEvent;
		private readonly IGameEvent<ItemPickupCompletedEventArgs> _itemPickupCompletedEvent = null;

		public InventoryApplicationCoordinator( IPlayerRuntimeRegistry players, IGameEventRegistryService eventFactory )
		{
			_players = players ?? throw new ArgumentNullException( nameof( players ) );

			_itemPickupRequestedEvent = eventFactory.GetEvent<ItemPickupRequestedEventArgs>(
				ItemPickupRequestedEventArgs.Name,
				ItemPickupRequestedEventArgs.NameSpace
			);
			_itemPickupRequested = _itemPickupRequestedEvent.Subscribe( OnItemPickupRequested );

			_itemPickupCompletedEvent = eventFactory.GetEvent<ItemPickupCompletedEventArgs>(
				ItemPickupCompletedEventArgs.Name,
				ItemPickupCompletedEventArgs.NameSpace
			);
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_itemPickupRequested.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnItemPickupRequested( in ItemPickupRequestedEventArgs args )
		{
			args.PlayerId.ThrowIfInvalid( nameof( OnItemPickupRequested ) );

			if ( args.Amount <= 0 || !args.ItemId.IsValid ) {
				PublishPickupCompleted( in args, false );
				return;
			}

			if ( !_players.TryGetInventory( args.PlayerId, out IPlayerInventoryCoordinator? inventory ) ) {
				PublishPickupCompleted( in args, false );
				return;
			}

			PublishPickupCompleted( in args, inventory.Backpack.TryAdd( args.ItemId, args.Amount ) );
		}

		private void PublishPickupCompleted( in ItemPickupRequestedEventArgs args, bool success )
		{
			_itemPickupCompletedEvent.Publish(
				new ItemPickupCompletedEventArgs(
					args.PlayerId,
					args.ItemId,
					args.Amount,
					args.PickupEntityId,
					success
				)
			);
		}
	};
};
