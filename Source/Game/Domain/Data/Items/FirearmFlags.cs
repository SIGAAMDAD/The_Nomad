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

namespace Nomad.Game.Domain.Data.Items
{
	[Flags]
	public enum FirearmFlags : ulong
	{
		None = 0,

		// basic operation
		CanAimDownSights = 1UL << 0,
		CanHipFire = 1UL << 1,
		CanFireWhileMoving = 1UL << 2,
		CanFireWhileDashing = 1UL << 3,
		CanFireWhileSliding = 1UL << 4,
		CanFireWhileJumping = 1UL << 5,
		CanFireDuringParry = 1UL << 6,

		// handling / cycling
		HasManualAction = 1UL << 7,   // pump, lever, bolt, break
		RequiresCycleAfterShot = 1UL << 8,
		RequiresCocking = 1UL << 9,
		HasBurstLimiter = 1UL << 10,
		OpenBolt = 1UL << 11,
		ClosedBolt = 1UL << 12,

		// feeding / reload behavior
		HasDetachableMagazine = 1UL << 13,
		HasInternalMagazine = 1UL << 14,
		HasTubeMagazine = 1UL << 15,
		HasCylinder = 1UL << 16,
		SingleShot = 1UL << 17,
		BreakAction = 1UL << 18,
		PerShellReload = 1UL << 19,
		SupportsTacticalReload = 1UL << 20,

		// reliability / maintenance
		CanJam = 1UL << 21,
		CanOverheat = 1UL << 22,
		GetsDirty = 1UL << 23,
		WeatherSensitive = 1UL << 24,
		MisfireProne = 1UL << 25,
		ExplosiveAmmoRisk = 1UL << 26,

		// projectile / penetration traits
		PelletSpread = 1UL << 27,
		FiresSlug = 1UL << 28,
		ArmorPiercingCapable = 1UL << 29,
		HollowPointCapable = 1UL << 30,
		ExplosiveAmmoCapable = 1UL << 31,
		IncendiaryCapable = 1UL << 32,
		EnergyWeapon = 1UL << 33,

		// recoil / ergonomics
		HighRecoil = 1UL << 34,
		TwoHanded = 1UL << 35,
		OneHandCapable = 1UL << 36,
		HeavyWeapon = 1UL << 37,
		Concealable = 1UL << 38,

		// stealth / signature
		LoudReport = 1UL << 39,
		SuppressorCompatible = 1UL << 40,
		RevealsShooterPosition = 1UL << 41,
		MuzzleFlashHeavy = 1UL << 42,

		// attachment compatibility
		SupportsOptics = 1UL << 43,
		SupportsBarrelMods = 1UL << 44,
		SupportsMagazineMods = 1UL << 45,
		SupportsAmmoMods = 1UL << 46,
		SupportsUnderbarrel = 1UL << 47,

		// special behavior
		AntiMaterial = 1UL << 48,
		AntiArmor = 1UL << 49,
		SiegeWeapon = 1UL << 50,
		CanRicochet = 1UL << 51,
		CanPenetrateTargets = 1UL << 52,
		CanStaggerHeavies = 1UL << 53
	};
};
