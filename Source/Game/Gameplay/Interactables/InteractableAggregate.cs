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
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Gameplay.Entity;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk.Interactables;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Events.Interactables;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Gameplay.Interactables
{
	/*
	===================================================================================

	Interactable

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal partial class InteractableAggregate : EntityBase, IInteractableEntity
	{
		public PlayerInteractionStatus PlayerStatus => _playerStatus;
		private PlayerInteractionStatus _playerStatus = PlayerInteractionStatus.None;

		public IGameEvent<PlayerInteractionStatusChangedEventArgs> StatusChanged => _statusChanged;

		public InternString InteractionPrompt {
			get {
				throw new NotImplementedException();
			}
		}

		public IReadOnlyList<EntityInteractionKind> InteractionKinds {
			get {
				throw new NotImplementedException();
			}
		}

		public uint InteractionRevision => _revision;
		private uint _revision = 0;

		private readonly IGameEvent<PlayerInteractionStatusChangedEventArgs> _statusChanged = default;

		private readonly InteractableRoot? _prefab;

		/*
		===============
		InteractableAggregate
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="prefab"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public InteractableAggregate( InteractableRoot prefab, IGameEventRegistryService eventFactory )
			: this(
				prefab,
				new EntityId( Guid.NewGuid() ),
				InternString.Empty,
				InternString.Empty,
				EntityFlags.Interactable,
				eventFactory
			)
		{
		}

		protected InteractableAggregate( InteractableRoot? prefab, EntityId entityId, InternString definitionId, InternString displayName, EntityFlags flags, IGameEventRegistryService eventFactory )
			: base( entityId, definitionId, displayName, EntityType.Interactable, flags )
		{
			_prefab = prefab;

			if ( _prefab == null ) {
				return;
			}

			_prefab.PlayerEntered += OnPlayerEntered;
			_prefab.PlayerExited += OnPlayerExited;

			_statusChanged = eventFactory
				.GetEvent<PlayerInteractionStatusChangedEventArgs>(
					PlayerInteractionStatusChangedEventArgs.Name,
					PlayerInteractionStatusChangedEventArgs.NameSpace
				);
		}

		/*
		===============
		OnPlayerExited
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnPlayerExited( PlayerId peerId )
		{
			if ( _playerStatus == PlayerInteractionStatus.InRange || _playerStatus == PlayerInteractionStatus.Interacting ) {
				SetStatus( peerId, PlayerInteractionStatus.None );
			}
		}

		/*
		===============
		OnPlayerEntered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnPlayerEntered( PlayerId peerId )
		{
			SetStatus( peerId, PlayerInteractionStatus.InRange );
		}

		private void SetStatus( PlayerId interactorId, PlayerInteractionStatus newStatus )
		{
			if ( _playerStatus == newStatus ) {
				return;
			}

			PlayerInteractionStatus oldStatus = _playerStatus;
			_playerStatus = newStatus;

			_statusChanged.Publish(
				new PlayerInteractionStatusChangedEventArgs(
					interactorId,
					oldStatus,
					newStatus
				)
			);
		}
	};
};
