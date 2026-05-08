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

namespace Nomad.Game.Domain.Data.Player
{
	public static class PlayerSpawnProfiles
	{
		public static readonly PlayerSpawnProfileDefinition NewGame = new() {
			Id = "NewGame",
			Health = new( SpawnValueMode.Full ),
			Rage = new( SpawnValueMode.Full ),
			Sanity = new( SpawnValueMode.PercentOfMax, 0.90f ),
			MovementSpeed = new( SpawnValueMode.Full ),
			DashHeat = new( SpawnValueMode.Zero ),
			FirelinkCharges = new( SpawnValueMode.Absolute, 4 ),
			ClearTransientCombatState = true,
			ClearTemporaryStatusEffects = true
		};
	};
};
