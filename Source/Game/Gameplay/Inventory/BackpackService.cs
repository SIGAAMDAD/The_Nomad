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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Inventory;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Prefabs;
using Nomad.Game.Gameplay.Items;
using System.Collections.Generic;

namespace Nomad.Game.Gameplay.Inventory
{
	/*
	===================================================================================

	BackpackService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class BackpackService : ItemInstance, IBackpackService
	{
		public InternString StorageId => _inventory.StorageId;
		public InventoryContainerType ContainerType => _inventory.ContainerType;
		public InventoryRules Rules => _inventory.Rules;
		public float CurrentWeight => _inventory.CurrentWeight;
		public IReadOnlyList<ItemStack> Stacks => _inventory.Stacks;

		public BackpackLocationKind LocationKind => _locationKind;
		private BackpackLocationKind _locationKind;

		public EntityId LocationEntityId => _ownerId;
		private EntityId _ownerId;

		public Vector2 DroppedOrigin => Vector2.Zero;

		public BackpackStatus Status => _status;
		private BackpackStatus _status = BackpackStatus.Equipped;

		public IStorageUnit? Storage => _inventory;
		private readonly IStorageUnit _inventory;

		private readonly IDisposable _stateChanged;

//		private readonly BackpackUnequippedPrefab _unequippedPrefab;

		private PlayerStateId _currentState = PlayerStateId.Idle;

		public IGameEvent<BackpackStatusChangedEventArgs> StateChanged => _statusChanged;
		private readonly IGameEvent<BackpackStatusChangedEventArgs> _statusChanged = null;

		public IGameEvent<BackpackUnequipRequestedEventArgs> UnequipRequested => _unequipRequested;
		private readonly IGameEvent<BackpackUnequipRequestedEventArgs> _unequipRequested = null;

		public IGameEvent<BackpackUnequippedEventArgs> Unequipped => _unequipped;

		private readonly IGameEvent<BackpackUnequippedEventArgs> _unequipped = null;

		/*
		===============
		BackpackService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="initialStatus"></param>
		/// <param name="stateReader"></param>
		/// <param name="inventory"></param>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public BackpackService(
			BackpackStatus initialStatus,
			IPlayerStateReader stateReader,
			IStorageUnit inventory,
			IGameEventRegistryService eventFactory
		)
			: base( new ItemInstanceId( Guid.NewGuid() ), null, eventFactory )
		{
			RangeGuard.ThrowIfOutOfRange( (int)initialStatus, (int)BackpackStatus.Min, (int)BackpackStatus.Max, nameof( initialStatus ) );
			ArgumentGuard.ThrowIfNull( stateReader, nameof( stateReader ) );
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_status = initialStatus;
			_inventory = inventory ?? throw new ArgumentNullException( nameof( inventory ) );

			_statusChanged = eventFactory
				.GetEvent<BackpackStatusChangedEventArgs>(
					BackpackStatusChangedEventArgs.Name,
					BackpackStatusChangedEventArgs.NameSpace
				);

			_unequipRequested = eventFactory
				.GetEvent<BackpackUnequipRequestedEventArgs>(
					BackpackUnequipRequestedEventArgs.Name,
					BackpackUnequipRequestedEventArgs.NameSpace
				);

//			_unequippedPrefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );

			_stateChanged = stateReader.StateChanged.Subscribe( OnStateChanged );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			_statusChanged.Dispose();
			_unequipRequested.Dispose();
			_stateChanged.Dispose();
		}

		/*
		===============
		TryEquip
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool TryEquip()
		{
			if ( _status == BackpackStatus.Equipped ) {
				return false;
			}

			return false;
		}

		/*
		===============
		TryUnequip
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public bool TryUnequip()
		{
			if ( _status != BackpackStatus.Equipped ) {
				return false;
			}

			switch ( _currentState ) {
				case PlayerStateId.Dead:
					return false;

				case PlayerStateId.Moving:
				case PlayerStateId.Idle:
					RemoveBackpack();
					break;

				case PlayerStateId.RestingAtCheckpoint:
					RemoveBackpack( true );
					break;

				default:
					throw new IndexOutOfRangeException( nameof( _currentState ) );
			}

			return true;
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
			return _inventory.TryAdd( itemType, amount );
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
			return _inventory.TryRemove( itemType, amount, out removed );
		}

		public bool TryAddInstance( ItemInstanceId instanceId )
		{
			return _inventory.TryAddInstance( instanceId );
		}

		public bool TryRemoveInstance( ItemInstanceId instanceId )
		{
			return _inventory.TryRemoveInstance( instanceId );
		}

		public bool ContainsInstance( ItemInstanceId instanceId )
		{
			return _inventory.ContainsInstance( instanceId );
		}

		public bool MoveStacksTo( IStorageUnit storageUnit )
		{
			return _inventory.MoveStacksTo( storageUnit );
		}

		public int GetStackAmount( ItemDefinitionId itemType )
		{
			return _inventory.GetStackAmount( itemType );
		}

		public bool ContainsStack( ItemDefinitionId itemType )
		{
			return _inventory.ContainsStack( itemType );
		}

		private void SetBackpackStatus( BackpackStatus status )
		{
			BackpackStatus previousStatus = _status;
			_status = status;

			_statusChanged.Publish(
				new BackpackStatusChangedEventArgs(
					previousStatus,
					_status
				)
			);
		}

		private void RemoveBackpack( bool atCheckpoint = false )
		{
			if ( _status != BackpackStatus.Equipped ) {
				return;
			}
		}

		/*
		===============
		OnStateChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnStateChanged( in PlayerStateChangedEventArgs args )
		{
			_currentState = args.NewState;
		}
	};
};
