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
using Godot;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Events.Items;

namespace Nomad.Game.Prefabs
{
	internal sealed partial class PickupRoot : InteractableRoot
	{
		public EntityId PickupEntityId => _pickupEntityId;
		private readonly EntityId _pickupEntityId = new EntityId( Guid.NewGuid() );

		public ItemDefinitionId ItemId => _itemId;
		private ItemDefinitionId _itemId = ItemDefinitionId.Invalid;

		public int Amount => _amount;

		[Export]
		private string _exportedItemId = string.Empty;

		[Export]
		private int _amount = 1;

		public override InternString InteractionPrompt => new InternString( "Pick Up" );
		public override EntityInteractionKind PrimaryInteractionKind => EntityInteractionKind.Pickup;

		private IGameEvent<ItemPickupRequestedEventArgs> _pickupRequested = null;
		private IDisposable? _pickupCompleted;

		public override void _Ready()
		{
			base._Ready();

			EnsurePickupEvent();
			_pickupCompleted = ServiceLocator.GetService<IGameEventRegistryService>()
				.GetEvent<ItemPickupCompletedEventArgs>(
					ItemPickupCompletedEventArgs.Name,
					ItemPickupCompletedEventArgs.NameSpace
				)
				.Subscribe( OnPickupCompleted );

			if ( !string.IsNullOrWhiteSpace( _exportedItemId ) ) {
				_itemId = new ItemDefinitionId( new InternString( _exportedItemId ) );
			}
		}

		public void Configure( ItemDefinitionId itemId, int amount )
		{
			_itemId = itemId;
			_amount = Math.Max( 1, amount );
		}

		public EntityInteractionResult RequestPickup( PlayerId playerId )
		{
			playerId.ThrowIfInvalid( nameof( RequestPickup ) );

			if ( !_itemId.IsValid || _amount <= 0 ) {
				return EntityInteractionResult.InvalidTarget();
			}

			EnsurePickupEvent();
			_pickupRequested.Publish(
				new ItemPickupRequestedEventArgs(
					playerId,
					_itemId,
					_amount,
					_pickupEntityId
				)
			);

			return EntityInteractionResult.SuccessResult();
		}

		public override void BuildInteractionOptions( List<InteractionMenuOption> options )
		{
			options.Add( new InteractionMenuOption( new InternString( "Pick Up" ), EntityInteractionKind.Pickup ) );
		}

		public override EntityInteractionResult RequestInteraction( PlayerId playerId, EntityInteractionKind kind )
		{
			return kind == EntityInteractionKind.Pickup
				? RequestPickup( playerId )
				: EntityInteractionResult.InvalidTarget();
		}

		public override void _ExitTree()
		{
			_pickupCompleted?.Dispose();
			_pickupCompleted = null;

			base._ExitTree();
		}

		private void EnsurePickupEvent()
		{
			if ( _pickupRequested != null ) {
				return;
			}

			_pickupRequested = ServiceLocator.GetService<IGameEventRegistryService>()
				.GetEvent<ItemPickupRequestedEventArgs>(
					ItemPickupRequestedEventArgs.Name,
					ItemPickupRequestedEventArgs.NameSpace
				);
		}

		private void OnPickupCompleted( in ItemPickupCompletedEventArgs args )
		{
			if ( args.Success && args.PickupEntityId == _pickupEntityId ) {
				QueueFree();
			}
		}
	};
};
