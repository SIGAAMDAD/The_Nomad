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
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;
using Nomad.Scene.GameObjects;

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

	internal partial class Interactable : EngineObject2D, IInteractable
	{
		public PlayerInteractionStatus PlayerStatus {
			get => _playerStatus;
			set {
				if ( _playerStatus == value ) {
					return;
				}
				PlayerInteractionStatus oldValue = _playerStatus;
				_playerStatus = value;
				_statusChanged.Publish( new PlayerInteractionStatusChangedEventArgs( Guid.Empty, oldValue, value ) );
			}
		}
		private PlayerInteractionStatus _playerStatus = PlayerInteractionStatus.None;

		public IGameEvent<PlayerInteractionStatusChangedEventArgs> StatusChanged => _statusChanged;
		private readonly IGameEvent<PlayerInteractionStatusChangedEventArgs> _statusChanged = default;

		private InteractableZone _zone;

		/*
		===============
		OnPlayerExited
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnPlayerExited()
		{
			if ( _playerStatus == PlayerInteractionStatus.InRange || _playerStatus == PlayerInteractionStatus.Interacting ) {
				PlayerStatus = PlayerInteractionStatus.None;
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
		private void OnPlayerEntered()
		{
			PlayerStatus = PlayerInteractionStatus.InRange;
		}

		protected override void OnInit()
		{
			base.OnInit();

			_zone = new InteractableZone();
			_zone.PlayerEntered += OnPlayerEntered;
			_zone.PlayerExited += OnPlayerExited;
		}
	};
};
