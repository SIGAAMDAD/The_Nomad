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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Infrastructure.Gameplay.Items;
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Sdk.Items;
using Nomad.Save.Extensions;
using Nomad.Save.Services;

namespace Nomad.Game.Application.Gameplay.Inventory
{
	/*
	===================================================================================

	StorageUnit

	===================================================================================
	*/
	/// <summary>
	/// Represents a in-game item storage unit. This can be the player's inventory, a loot drop,
	/// or even a shop's inventory.
	/// </summary>

	internal sealed class StorageUnit : IStorageUnit
	{
		public InternString StorageId => _definition.Id;
		public InternString DisplayName => _definition.DisplayName;
		public InventoryContainerType ContainerType => _definition.Type;
		public InventoryRules Rules => _definition.Rules;

		public IReadOnlyList<ItemStack> Stacks {
			get {
				var stacks = new List<ItemStack>( _stacks.Count );

				foreach ( var stack in _stacks ) {
					stacks.Add( new ItemStack( stack.Key, stack.Value ) );
				}

				return stacks;
			}
		}

		private readonly StorageUnitDefinition _definition;
		private readonly ItemCatalog _catalog;
		private readonly ItemInstanceRepository _instanceRepository;

		private readonly Dictionary<ItemDefinitionId, int> _stacks = new();
		private readonly HashSet<ItemInstanceId> _instances = new();

		private readonly IDisposable _saveBegin;
		private readonly IDisposable _loadBegin;

		private bool _isDisposed = false;

		public float CurrentWeight {
			get {
				if ( _definition.Rules.IgnoreWeight ) {
					return 0.0f;
				}

				float weight = 0.0f;

				foreach ( var stack in _stacks ) {
					var item = _catalog.Get<ItemDefinition>( stack.Key );
					weight += item.Weight * stack.Value;
				}

				if ( _instanceRepository != null ) {
					foreach ( var instance in _instances ) {
						var item = _instanceRepository.Get( instance );
						if ( item != null ) {
							weight += item.Definition.Weight;
						}
					}
				}

				return weight;
			}
		}

		/*
		===============
		StorageUnit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="displayName"></param>
		/// <param name="rules"></param>
		/// <param name="type"></param>
		/// <param name="catalog"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public StorageUnit(
			StorageUnitDefinition definition,
			ItemCatalog catalog,
			ItemInstanceRepository instanceRepository,
			IGameEventRegistryService eventFactory
		)
		{
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
			_catalog = catalog ?? throw new ArgumentNullException( nameof( catalog ) );
			_instanceRepository = instanceRepository ?? throw new ArgumentNullException( nameof( instanceRepository ) );

			_saveBegin = eventFactory
				.GetEvent<SaveBeginEventArgs>(
					SaveBeginEventArgs.Name,
					SaveBeginEventArgs.NameSpace
				)
				.Subscribe( OnSaveBegin );

			_loadBegin = eventFactory
				.GetEvent<LoadBeginEventArgs>(
					LoadBeginEventArgs.Name,
					LoadBeginEventArgs.NameSpace
				)
				.Subscribe( OnLoadBegin );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_saveBegin?.Dispose();
			_loadBegin?.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		TryAdd
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemType"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool TryAdd( ItemDefinitionId itemType, int amount )
		{
			RangeGuard.ThrowIfNegativeOrZero( amount, nameof( amount ) );

			var item = _catalog.Get<ItemDefinition>( itemType );
			if ( item == null ) {
				return false;
			}

			if ( !_definition.Rules.AcceptsItem( item ) ) {
				return false;
			}

			if ( !_definition.Rules.IgnoreWeight ) {
				float newWeight = CurrentWeight + item.Weight * amount;
				if ( newWeight > _definition.Rules.MaxWeight ) {
					return false;
				}
			}

			checked {
				_stacks.TryGetValue( itemType, out int currentAmount );
				_stacks[itemType] = currentAmount + amount;
			}

			return true;
		}

		/*
		===============
		TryRemove
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="itemType"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool TryRemove( ItemDefinitionId itemType, int amount, out int removed )
		{
			removed = 0;

			if ( amount <= 0 ) {
				return false;
			}

			if ( !_stacks.TryGetValue( itemType, out int stackSize ) ) {
				return false;
			}

			if ( stackSize < amount ) {
				removed = stackSize;
				return false;
			}

			stackSize -= amount;
			removed = amount;
			if ( stackSize == 0 ) {
				_stacks.Remove( itemType );
			} else {
				_stacks[itemType] = stackSize;
			}

			return true;
		}

		/*
		===============
		TryAddInstance
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="instanceId"></param>
		/// <returns></returns>
		public bool TryAddInstance( ItemInstanceId instanceId )
		{
			return _instances.Add( instanceId );
		}

		/*
		===============
		TryRemoveInstance
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="instanceId"></param>
		/// <returns></returns>
		public bool TryRemoveInstance( ItemInstanceId instanceId )
		{
			return _instances.Remove( instanceId );
		}

		/*
		===============
		ContainsInstance
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="instanceId"></param>
		/// <returns></returns>
		public bool ContainsInstance( ItemInstanceId instanceId )
		{
			return _instances.Contains( instanceId );
		}

		public bool MoveStacksTo( IStorageUnit storageUnit )
		{
			ArgumentGuard.ThrowIfNull( storageUnit, nameof( storageUnit ) );

			if ( _stacks.Count == 0 ) {
				return false;
			}

			foreach ( var stack in _stacks ) {
				if ( !storageUnit.TryAdd( stack.Key, stack.Value ) ) {
					return false;
				}
			}

			_stacks.Clear();

			return true;
		}

		public int GetStackAmount( ItemDefinitionId itemType )
		{
			return _stacks.TryGetValue( itemType, out int stackSize ) ? stackSize : 0;
		}

		public bool ContainsStack( ItemDefinitionId itemType )
		{
			return _stacks.ContainsKey( itemType );
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
		private void OnSaveBegin( in SaveBeginEventArgs args )
		{
			lock ( this ) {
				var writer = args.Writer.AddSection( $"StorageUnit#{(string)StorageId}:{(string)DisplayName}" );

				writer.WriteDictionary(
					"Stacks",
					_stacks
				);

				writer.AddField( "InstanceCount", _instances.Count );
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
		private void OnLoadBegin( in LoadBeginEventArgs args )
		{
		}
	};
};
