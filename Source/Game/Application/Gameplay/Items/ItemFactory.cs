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
using Nomad.Game.Application.Gameplay.Combat;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Items;
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Application.Gameplay.Items
{
	internal sealed class ItemFactory
	{
		private readonly INomadBehaviorRegistry _behaviorRegistry;
		private readonly IItemInstanceRepository _instanceRepository;
		private readonly IGameEventRegistryService _eventFactory;

		public ItemFactory(
			INomadBehaviorRegistry behaviorRegistry,
			IItemInstanceRepository instanceRepository,
			IGameEventRegistryService eventFactory
		)
		{
			_behaviorRegistry = behaviorRegistry ?? throw new ArgumentNullException( nameof( behaviorRegistry ) );
			_instanceRepository = instanceRepository ?? throw new ArgumentNullException( nameof( instanceRepository ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
		}

		public ItemInstance CreateItem( ItemDefinition definition )
		{
			ArgumentGuard.ThrowIfNull( definition, nameof( definition ) );

			var instanceId = new ItemInstanceId( Guid.NewGuid() );

			switch ( definition.BaseType ) {
				case ItemType.Consumable: {
						var consumableDefinition = definition as ConsumableDefinition ??
							throw new InvalidOperationException( $"Item '{(string)definition.Id.Value}' is not a consumable definition." );

						InternString behaviorId = consumableDefinition.BehaviorId ??
							throw new InvalidOperationException( $"Consumable '{(string)definition.Id.Value}' has no behavior id." );

						IConsumableBehavior behavior = _behaviorRegistry.Create<IConsumableBehavior, ConsumableDefinition>(
							behaviorId,
							consumableDefinition
						);

						var instance = new ConsumableInstance(
							instanceId,
							consumableDefinition,
							behavior,
							_eventFactory
						);

						_instanceRepository.TryAdd( instanceId, instance );
						return instance;
					}

				case ItemType.FirearmWeapon: {
						var firearmDefinition = definition as FirearmDefinition ??
							throw new InvalidOperationException( $"Item '{(string)definition.Id.Value}' is not a firearm definition." );

						var instance = new FirearmInstance( instanceId, _eventFactory, firearmDefinition );
						_instanceRepository.TryAdd( instanceId, instance );
						return instance;
					}

				case ItemType.Ammunition:
				case ItemType.FirearmMod:
				case ItemType.MeleeWeapon:
				case ItemType.MeleeMod:
					throw new NotSupportedException( $"Item type '{definition.BaseType}' does not have an instance factory yet." );

				default:
					throw new ArgumentOutOfRangeException( nameof( definition ) );
			}
		}
	};
};
