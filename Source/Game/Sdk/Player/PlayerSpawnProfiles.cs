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

using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Sdk.Player
{
	public static class PlayerSpawnProfiles
	{
		public static readonly PlayerSpawnProfileDefinition NewGame = new() {
			Id = "NewGame",
			Health = new SpawnValueRule( SpawnValueMode.Full ),
			Rage = new SpawnValueRule( SpawnValueMode.Full ),
			Sanity = new SpawnValueRule( SpawnValueMode.PercentOfMax, 0.90f ),
			MovementSpeed = new SpawnValueRule( SpawnValueMode.Full ),
			DashHeat = new SpawnValueRule( SpawnValueMode.Zero ),
			FirelinkCharges = new SpawnValueRule( SpawnValueMode.Absolute, 4 ),
			ClearTransientCombatState = true,
			ClearTemporaryStatusEffects = true
		};
	}
}
