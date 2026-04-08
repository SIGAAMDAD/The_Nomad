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
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;

namespace Nomad.Game.Application.Gameplay.Player.State {
	/*
	===================================================================================
	
	PlayerStateChanged
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerStateController {
		public PlayerStateId State {
			get => _state;
			set {
				if ( _state == value ) {
					return;
				}
				var oldValue = _state;
				_state = value;
				_stateChanged.Publish( new PlayerStateChangedEventArgs( oldValue, _state ) );
			}
		}
		private PlayerStateId _state = PlayerStateId.Idle;

		public IGameEvent<PlayerStateChangedEventArgs> StateChanged => _stateChanged;
		private readonly IGameEvent<PlayerStateChangedEventArgs> _stateChanged;

		/*
		===============
		PlayerStateController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		public PlayerStateController( IGameEventRegistryService eventFactory ) {
			_stateChanged = eventFactory
				.GetEvent<PlayerStateChangedEventArgs>( EventNames.PLAYER_STATE_CHANGED, EventNames.NAMESPACE );
		}
	};
};