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

using System.Collections.Generic;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Sdk.Player
{
	public sealed record PlayerSpawnProfileDefinition
	{
		public string Id { get; init; } = string.Empty;

		public SpawnValueRule Health { get; init; } = new SpawnValueRule( SpawnValueMode.Preserve );
		public SpawnValueRule Rage { get; init; } = new SpawnValueRule( SpawnValueMode.Preserve );
		public SpawnValueRule Sanity { get; init; } = new SpawnValueRule( SpawnValueMode.Preserve );
		public SpawnValueRule DashHeat { get; init; } = new SpawnValueRule( SpawnValueMode.Zero );
		public SpawnValueRule MovementSpeed { get; init; } = new SpawnValueRule( SpawnValueMode.Preserve );

		public SpawnValueRule FirelinkCharges { get; init; } = new SpawnValueRule( SpawnValueMode.Preserve );

		public bool ClearTransientCombatState { get; init; } = true;
		public bool ClearTemporaryStatusEffects { get; init; } = false;

		public Dictionary<string, bool> FlagOverrides { get; init; } = new Dictionary<string, bool>();
	}
}
