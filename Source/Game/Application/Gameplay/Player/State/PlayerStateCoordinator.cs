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
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player.State {
	/*
	===================================================================================
	
	PlayerStateCoordinator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerStateCoordinator : IPlayerStateWriter, IPlayerStateReader {
		public PlayerStateId Current => _state;
		private PlayerStateId _state = PlayerStateId.Idle;

		public IGameEvent<PlayerStateChangedEventArgs> StateChanged => _stateChanged;
		private readonly IGameEvent<PlayerStateChangedEventArgs> _stateChanged;

		/*
		===============
		PlayerStateCoordinator
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="initialState"></param>
		/// <param name="eventFactory"></param>
		public PlayerStateCoordinator( Guid playerId, PlayerStateId initialState, IGameEventRegistryService eventFactory ) {
			_state = initialState;
			_stateChanged = eventFactory
				.GetEvent<PlayerStateChangedEventArgs>( $"{playerId}:{EventNames.PLAYER_STATE_CHANGED}", EventNames.NAMESPACE );
		}

		public bool TrySetState( PlayerStateId newState ) {
			if ( newState == _state || !CanTransitionTo( newState ) ) {
				return false;
			}

			var oldState = _state;
			_state = newState;

			_stateChanged.Publish( new PlayerStateChangedEventArgs( oldState, newState ) );
			
			return true;
		}

		private bool CanTransitionTo( PlayerStateId newState ) {
			return (_state, newState) switch {
				(PlayerStateId.Dead, PlayerStateId.Moving) => false,
				(PlayerStateId.Dead, PlayerStateId.RestingAtCheckpoint) => false,
				_ => true
			};
		}
	};
};