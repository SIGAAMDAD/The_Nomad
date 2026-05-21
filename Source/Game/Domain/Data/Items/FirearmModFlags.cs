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
	public enum FirearmModFlags : ulong
	{
		None = 0,

		// attachment type
		Optic = 1UL << 0,
		Barrel = 1UL << 1,
		Magazine = 1UL << 2,
		AmmoConversion = 1UL << 3,
		Underbarrel = 1UL << 4,
		Stock = 1UL << 5,
		Grip = 1UL << 6,
		Receiver = 1UL << 7,

		// handling behavior
		ImprovesAimDownSights = 1UL << 8,
		WorsensAimDownSights = 1UL << 9,
		ImprovesHipFire = 1UL << 10,
		WorsensHipFire = 1UL << 11,
		ImprovesRecoilControl = 1UL << 12,
		WorsensRecoilControl = 1UL << 13,
		ImprovesReloadSpeed = 1UL << 14,
		WorsensReloadSpeed = 1UL << 15,
		ImprovesEquipSpeed = 1UL << 16,
		WorsensEquipSpeed = 1UL << 17,

		// reliability / maintenance
		ImprovesReliability = 1UL << 18,
		WorsensReliability = 1UL << 19,
		ReducesHeat = 1UL << 20,
		IncreasesHeat = 1UL << 21,
		ReducesDirtSensitivity = 1UL << 22,
		IncreasesDirtSensitivity = 1UL << 23,

		// signature
		ReducesReport = 1UL << 24,
		IncreasesReport = 1UL << 25,
		ReducesMuzzleFlash = 1UL << 26,
		IncreasesMuzzleFlash = 1UL << 27,
		ConcealsShooter = 1UL << 28,
		RevealsShooter = 1UL << 29,

		// operation changes
		ChangesMagazineSize = 1UL << 30,
		ChangesFireRate = 1UL << 31,
		ChangesCycleTime = 1UL << 32,
		ChangesReloadType = 1UL << 33,
		ChangesAllowedFireStates = 1UL << 34,

		// restrictions
		RequiresTwoHands = 1UL << 35,
		BlocksConcealment = 1UL << 36,
		BlocksSuppressor = 1UL << 37,
		BlocksOptic = 1UL << 38,
		Unique = 1UL << 39
	};
};
