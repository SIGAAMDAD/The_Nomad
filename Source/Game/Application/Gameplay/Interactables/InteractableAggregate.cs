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
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Interactables
{
	/*
	===================================================================================

	Interactable

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal partial class InteractableAggregate : IInteractable
	{
		public PlayerInteractionStatus PlayerStatus => _playerStatus;
		private PlayerInteractionStatus _playerStatus = PlayerInteractionStatus.None;

		public IGameEvent<PlayerInteractionStatusChangedEventArgs> StatusChanged => _statusChanged;
		private readonly IGameEvent<PlayerInteractionStatusChangedEventArgs> _statusChanged = default;

		private readonly InteractableRoot _prefab;

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
		public InteractableAggregate( InteractableRoot prefab )
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_prefab.PlayerEntered += OnPlayerEntered;
			_prefab.PlayerExited += OnPlayerExited;
		}

		/*
		===============
		OnPlayerExited
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnPlayerExited( PeerId peerId )
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
		private void OnPlayerEntered( PeerId peerId )
		{
			SetStatus( peerId, PlayerInteractionStatus.InRange );
		}

		private void SetStatus( PeerId interactorId, PlayerInteractionStatus newStatus )
		{
			if ( _playerStatus == newStatus ) {
				return;
			}
			var oldStatus = _playerStatus;
			_playerStatus = newStatus;
			_statusChanged.Publish(
				new PlayerInteractionStatusChangedEventArgs( interactorId, oldStatus, newStatus )
			);
		}
	};
};
