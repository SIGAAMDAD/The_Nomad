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
	public struct PlayerStatModifierSet
	{
		public float FlatAdd { get; set; }
		public float AddPercent { get; set; }
		public float MulPercent { get; set; }

		public float Apply( float baseValue )
		{
			float value = baseValue + FlatAdd;
			value *= 1.0f + AddPercent;
			value *= 1.0f + MulPercent;
			return value;
		}
	};
};
