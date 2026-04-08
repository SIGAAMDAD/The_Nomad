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

using Nomad.Game.Domain.Data.Player;

namespace Nomad.Game.Domain.Events.Player {
	/// <summary>
	/// Event that triggers whenever the player's internal state has changed.
	/// </summary>
	public readonly struct PlayerStateChangedEventArgs {
		/// <summary>
		/// The old replaced state.
		/// </summary>
		public PlayerStateId OldState { get; }

		/// <summary>
		/// The current state.
		/// </summary>
		public PlayerStateId NewState { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldState"></param>
		/// <param name="newState"></param>
		public PlayerStateChangedEventArgs( PlayerStateId oldState, PlayerStateId newState ) {
			OldState = oldState;
			NewState = newState;
		}
	};
};