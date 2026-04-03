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

using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Interactables;
using Nomad.Game.Domain.Events.Interactables;
using Nomad.Game.Domain.Interfaces.Interactables;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Interactables {
	/*
	===================================================================================
	
	Interactable
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal partial class Interactable : EngineSceneObject, IInteractable {
		public PlayerInteractionStatus PlayerStatus {
			get => _playerStatus;
			set {
				PlayerInteractionStatus oldValue = _playerStatus;
				_playerStatus = value;
			}
		}
		private PlayerInteractionStatus _playerStatus = PlayerInteractionStatus.None;

		public IGameEvent<PlayerInteractionStatusChangedEventArgs> StatusChanged => _statusChanged;
		private IGameEvent<PlayerInteractionStatusChangedEventArgs> _statusChanged;

		protected override void OnInit() {
			base.OnInit();

			_statusChanged = GameEventRegistry.GetEvent<PlayerInteractionStatusChangedEventArgs>( EventNames.PLAYER_INTERACTION_STATUS_CHANGED, EventNames.NAMESPACE );
		}

		protected override void OnShutdown() {
			base.OnShutdown();

			_statusChanged?.Dispose();
		}
	};
};