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

namespace Nomad.Game.Domain.Data.Items
{
	public enum FirearmModStat : ushort
	{
		None = 0,

		FireRate,
		MagazineSize,

		AimDownSightsSpeed,
		AimDownSightsAccuracy,
		HipFireAccuracy,
		MovementAccuracy,

		RecoilKick,
		RecoilRecovery,
		SpreadBloom,
		SpreadRecovery,

		ReloadSpeed,
		EquipSpeed,
		DrawSpeed,
		StowSpeed,

		JamChance,
		MisfireChance,
		HeatPerShot,
		HeatRecovery,
		DirtAccumulation,

		Loudness,
		MuzzleFlash,
		ShooterRevealStrength,

		Weight,
		Handling
	};
};
