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
using Nomad.Game.Gameplay.Runtime.Areas;
using Nomad.Game.Gameplay.Runtime.Renown;
using Nomad.Game.Sdk.Renown.Regional;

namespace Nomad.Game.Content.Compilation.Renown
{
	internal sealed class RenownCompiler
	{
		private readonly RegionalRenownCompiler _regionalCompiler = new RegionalRenownCompiler();

		public CompiledRenownDatabase Compile(
			CompiledAreaDatabase areaDatabase,
			IEnumerable<RegionalRenownDefinition>? regionalDefinitions = null
		)
		{
			CompiledRegionalRenownDatabase regional = _regionalCompiler.Compile(
				areaDatabase,
				regionalDefinitions
			);

			var tiers = new[] {
				CompileTier( Constants.TIER_0_DEFINITION ),
				CompileTier( Constants.TIER_1_DEFINITION ),
				CompileTier( Constants.TIER_2_DEFINITION ),
				CompileTier( Constants.TIER_3_DEFINITION ),
				CompileTier( Constants.TIER_4_DEFINITION ),
				CompileTier( Constants.TIER_5_DEFINITION )
			};

			return new CompiledRenownDatabase( regional, tiers );
		}

		private static CompiledRenownTierDefinition CompileTier( RenownTierDefinition tier )
		{
			return new CompiledRenownTierDefinition(
				tier.Id,
				tier.Tier,
				tier.MinimumRenownScore,
				tier.MaximumRenownScore
			);
		}
	};
};
