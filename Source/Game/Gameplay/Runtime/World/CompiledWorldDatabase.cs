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
using Nomad.Core.Util;
using Nomad.Game.Gameplay.Runtime.Biomes;
using Nomad.Game.Sdk.World;

namespace Nomad.Game.Gameplay.Runtime.World
{
	/*
	===================================================================================

	CompiledWorldDatabase

	===================================================================================
	*/
	/// <summary>
	/// Immutable global world simulation data. Area/biome identity is compiled separately.
	/// </summary>

	internal sealed class CompiledWorldDatabase
	{
		public static readonly CompiledWorldDatabase Empty = new CompiledWorldDatabase(
			InternString.Empty,
			default,
			null,
			Array.Empty<SeasonDefinition>(),
			CompiledBiomeDatabase.Empty
		);

		public readonly InternString Name;
		public readonly WorldTime StartTime;
		public readonly CalendarDefinition? Calendar;
		public readonly SeasonDefinition[] Seasons;
		public readonly CompiledBiomeDatabase Biomes;

		public CompiledWorldDatabase(
			InternString name,
			WorldTime startTime,
			CalendarDefinition? calendar,
			SeasonDefinition[] seasons,
			CompiledBiomeDatabase biomes
		)
		{
			Name = name;
			StartTime = startTime;
			Calendar = calendar;
			Seasons = seasons ?? Array.Empty<SeasonDefinition>();
			Biomes = biomes ?? CompiledBiomeDatabase.Empty;
		}
	};
};
