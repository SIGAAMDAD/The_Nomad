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

namespace Nomad.Game.Application.Gameplay.Enemy.Planner
{
	/// <summary>
	/// Unique identifier keys for various planner variables used by the internal GOAP system.
	/// </summary>
	public enum WorldKey : uint
	{
		/**
		 * Shared perception keys
		 */
		EnemyVisible,
		EnemyAudible,
		EnemyLastKnownPositionValid,
		EnemyInMeleeRange,
		EnemyInCloseRange,
		EnemyInMidRange,
		EnemyInFarRange,
		PathToEnemyClear,
		HasLineOfSight,
		HasAttackLane,

		/**
		 * Shared self-state keys
		 */
		LowHealth,
		CriticalHealth,
		UnderHeavyFire,
		Staggered,
		Suppressed,
		WeaponLoaded,
		NeedsReload,
		HasGrenade,
		HasSpecialAttack,
		InGuard,
		CanParry,
		CanDodge,
		HasStaminaWindow,

		/**
		 * Shared tactical positioning keys
		 */
		HasCover,
		InCover,
		HasNearbyBetterCover,
		FlankRouteAvailable,
		RetreatRouteAvailable,
		IsExposed,
		AtPreferredRange,
		AtTargetPosition,

		/**
		 * Shared squad keys
		 */
		AllyEngagingTarget,
		AllySuppressingTarget,
		AllyFlankingTarget,
		AllyNeedsHelp,
		SquadPushOrdered,
		SquadRetreatOrdered,
		SquadHasFrontliner,
		SquadHasSuppressor,
		LaneReservedByAlly,

		/**
		 * Target posture keys
		 */
		TargetBlocking,
		TargetRecovering,
		TargetVulnerable,
		TargetSuppressed,
		TargetFacingMe,
		TargetExposed,
		TargetCommittedToAttack,

		Count
	};
};
