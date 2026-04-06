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

using Nomad.Game.Domain.Data.Gameplay;

namespace Nomad.Game.Domain.Events.Gameplay {
	/// <summary>
	/// Event that triggers whenever the game state has been changed.
	/// </summary>
	public readonly struct GameStateChangedEventArgs {
		public GameState PrevState { get; }
		public GameState CurrentState { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="prevState"></param>
		/// <param name="currentState"></param>
		public GameStateChangedEventArgs( GameState prevState, GameState currentState ) {
			PrevState = prevState;
			CurrentState = currentState;
		}
	};
};