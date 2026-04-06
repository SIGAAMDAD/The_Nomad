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
	/// 
	/// </summary>
	public readonly struct PlayerStatChangedEventArgs {
		/// <summary>
		/// 
		/// </summary>
		public float NewValue { get; }

		/// <summary>
		/// 
		/// </summary>
		public float OldValue { get; }

		/// <summary>
		/// 
		/// </summary>
		public StatType StatId { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newValue"></param>
		/// <param name="oldValue"></param>
		/// <param name="statId"></param>
		public PlayerStatChangedEventArgs( float newValue, float oldValue, StatType statId ) {
			NewValue = newValue;
			OldValue = oldValue;
			StatId = statId;
		}
	};
};