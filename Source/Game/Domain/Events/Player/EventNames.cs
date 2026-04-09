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

namespace Nomad.Game.Domain.Data.Player {
	public static class EventNames {
		public const string NAMESPACE = "Nomad.Game.Domain.Data.Player";

		public const string PLAYER_STATE_CHANGED = NAMESPACE + ".PlayerStateChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_ANIMATION_STATE_CHANGED = NAMESPACE + ".PlayerAnimationStateChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_DERIVED_STAT_CHANGED = NAMESPACE + ".PlayerDerivedStatChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_BASE_STAT_CHANGED = NAMESPACE + ".PlayerBaseStatChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_RESOURCE_CHANGED = NAMESPACE + ".PlayerResourceChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_DASH_STARTED = NAMESPACE + ".PlayerDashStarted";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_DASH_ENDED = NAMESPACE + ".PlayerDashEnded";

		/// <summary>
		/// Event that triggers whenever a dash "burnout" (overcharge) occurs.
		/// </summary>
		public const string PLAYER_DASH_BURNOUT = NAMESPACE + ".PlayerDashBurnout";

		/// <summary>
		/// Event that triggers whenever the dashkit recovers after a burnout.
		/// </summary>
		public const string PLAYER_DASH_RECHARGED = NAMESPACE + ".PlayerDashRecharged";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_DASH_MODULE_UNLOCKED = NAMESPACE + ".PlayerDashModuleUnlocked";

		/// <summary>
		/// Event that triggers whenever the player changes their current dashkit module.
		/// </summary>
		public const string PLAYER_DASH_MODULE_CHANGED = NAMESPACE + ".PlayerDashModuleChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_MOVEMENT_CHANGED = NAMESPACE + ".PlayerMovementChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string PLAYER_DIE = NAMESPACE + ".PlayerDie";

		public const string PLAYER_FLAGS_CHANGED = NAMESPACE + ".PlayerFlagsChanged";

		public const string PLAYER_SPAWN_REQUESTED = NAMESPACE + ".PlayerSpawnRequested";

		public const string PLAYER_SPAWN_RESULT_READY = NAMESPACE + ".PlayerSpawnResultReady";
	};
}