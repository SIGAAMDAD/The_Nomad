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

using Nomad.Game.Sdk.Areas;
using Nomad.Game.Sdk.Renown.Regional;

namespace Nomad.Game.Gameplay.Runtime.Renown
{
	internal readonly struct CompiledRegionalRenownDefinition
	{
		public readonly AreaDefinitionId AreaId;
		public readonly ushort AreaIndex;
		public readonly float InitialRenownScore;
		public readonly RenownTier InitialTier;
		public readonly float DecayPerDay;
		public readonly float SpreadMultiplier;
		public readonly float HistoricalFloor;

		public CompiledRegionalRenownDefinition(
			AreaDefinitionId areaId,
			ushort areaIndex,
			float initialRenownScore,
			RenownTier initialTier,
			float decayPerDay,
			float spreadMultiplier,
			float historicalFloor
		)
		{
			AreaId = areaId;
			AreaIndex = areaIndex;
			InitialRenownScore = initialRenownScore;
			InitialTier = initialTier;
			DecayPerDay = decayPerDay;
			SpreadMultiplier = spreadMultiplier;
			HistoricalFloor = historicalFloor;
		}
	};
};
