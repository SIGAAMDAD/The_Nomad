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
using Nomad.Core.Logger;
using Nomad.Game.Application.Gameplay.Combat;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Inventory;
using Nomad.Save.Extensions;
using Nomad.Save.Services;
using Nomad.Game.Application.Gameplay.Items;

namespace Nomad.Game.Infrastructure.Gameplay.Items
{
	/*
	===================================================================================

	ItemInstanceRepository

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class ItemInstanceRepository : DataInstanceRegistry<ItemInstanceId, ItemInstance>, IItemInstanceRepository
	{
		protected override string LoggerCategoryName => nameof( ItemInstanceRepository );

		private readonly IItemCatalog _catalog;
		private readonly IGameEventRegistryService _eventFactory;

		/*
		===============
		ItemInstanceRepository
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="eventFactory"></param>
		/// <param name="catalog"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ItemInstanceRepository( ILoggerService logger, IGameEventRegistryService eventFactory, IItemCatalog catalog )
			: base( logger, eventFactory )
		{
			_catalog = catalog ?? throw new ArgumentNullException( nameof( catalog ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
		}

		/*
		===============
		Get
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TItemDefinition"></typeparam>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public IItemInstance<TItemDefinition>? Get<TItemDefinition>( ItemInstanceId itemId )
			where TItemDefinition : ItemDefinition
		{
			return (IItemInstance<TItemDefinition>?)Get( itemId );
		}

		/*
		===============
		TryGet
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TItemDefinition"></typeparam>
		/// <param name="itemId"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public bool TryGet<TItemDefinition>( ItemInstanceId itemId, out IItemInstance<TItemDefinition>? instance )
			where TItemDefinition : ItemDefinition
		{
			if ( !TryGet( itemId, out var itemInstance ) || itemInstance is not IItemInstance<TItemDefinition> converted ) {
				instance = null;
				return false;
			}
			instance = converted;
			return true;
		}

		/*
		===============
		OnSaveBegin
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		protected override void OnSaveBegin( in SaveBeginEventArgs args )
		{
			lock ( this ) {
				using var writer = args.Writer.AddSection( nameof( ItemInstanceRepository ) );

				writer.AddField( "InstanceCount", dataCache.Count );

				int index = 0;
				foreach ( var instance in dataCache ) {
					writer.AddField( $"DefinitionId#{index}", (string)instance.Value.DefinitionId );
					writer.AddField( $"BaseType#{index}", (byte)instance.Value.BaseType );
				}
			}
		}

		/*
		===============
		OnLoadBegin
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		/// <exception cref="Exception"></exception>
		protected override void OnLoadBegin( in LoadBeginEventArgs args )
		{
			lock ( this ) {
				using var reader = args.Reader.FindSection( nameof( ItemInstanceRepository ) );

				if ( reader == null ) {
					throw new Exception( $"" );
				}

				Clear();

				int instanceCount = reader.GetField<int>( "InstanceCount" );
				dataCache.EnsureCapacity( instanceCount );

				for ( int i = 0; i < instanceCount; i++ ) {
					var instanceId = new ItemInstanceId( Guid.NewGuid() );
					var definitionId = reader.GetField<ItemDefinitionId>( $"DefinitionId#{i}" );

					if ( !_catalog.TryGet( definitionId, out ItemDefinition definition ) ) {
						throw new InvalidOperationException( $"Invalid item definition in save data: {(string)definitionId.Value}" );
					}

					// TODO: item instance factory
					ItemInstance instance = null;

					switch ( definition.BaseType ) {
						case ItemType.Ammunition:
							break;

						case ItemType.Consumable:
							break;

						case ItemType.FirearmWeapon: {
								var firearmDefinition = definition as FirearmDefinition ??
									throw new InvalidOperationException( $"ItemDefinition '{(string)definition.Id.Value}' has mismatched types (firearm)" );

								instance = new FirearmInstance( instanceId, _eventFactory, firearmDefinition );
								break;
							}

						default:
							throw new ArgumentOutOfRangeException( nameof( definition ) );
					}
				}
			}
		}
	};
};
