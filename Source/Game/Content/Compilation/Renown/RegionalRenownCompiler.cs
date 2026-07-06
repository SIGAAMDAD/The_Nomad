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
using System.Collections.Generic;
using Nomad.Game.Gameplay.Runtime.Areas;
using Nomad.Game.Gameplay.Runtime.Renown;
using Nomad.Game.Sdk.Areas;
using Nomad.Game.Sdk.Renown.Regional;

namespace Nomad.Game.Content.Compilation.Renown
{
	internal sealed class RegionalRenownCompiler
	{
		public CompiledRegionalRenownDatabase Compile(
			CompiledAreaDatabase areaDatabase,
			IEnumerable<RegionalRenownDefinition>? definitions = null
		)
		{
			if ( areaDatabase == null || areaDatabase.Count == 0 ) {
				return CompiledRegionalRenownDatabase.Empty;
			}

			var authoredByAreaId = new Dictionary<AreaDefinitionId, RegionalRenownDefinition>();

			if ( definitions != null ) {
				foreach ( RegionalRenownDefinition definition in definitions ) {
					if ( definition.AreaId.IsValid ) {
						authoredByAreaId[definition.AreaId] = definition;
					}
				}
			}

			var compiled = new CompiledRegionalRenownDefinition[areaDatabase.Count];
			var indexByAreaId = new Dictionary<AreaDefinitionId, ushort>( areaDatabase.Count );

			ReadOnlySpan<CompiledAreaDefinition> areas = areaDatabase.Areas;

			for ( int i = 0; i < areas.Length; i++ ) {
				CompiledAreaDefinition area = areas[i];
				ushort index = checked((ushort)i);
				indexByAreaId[area.Id] = index;

				if ( !authoredByAreaId.TryGetValue( area.Id, out RegionalRenownDefinition? authored ) ) {
					authored = new RegionalRenownDefinition {
						AreaId = area.Id,
						InitialRenownScore = 0.0f,
						DecayPerDay = 0.0f,
						SpreadMultiplier = 1.0f,
						HistoricalFloor = 0.0f
					};
				}

				compiled[i] = new CompiledRegionalRenownDefinition(
					area.Id,
					area.AreaIndex,
					authored.InitialRenownScore,
					ResolveTier( authored.InitialRenownScore ),
					authored.DecayPerDay,
					authored.SpreadMultiplier,
					authored.HistoricalFloor
				);
			}

			return new CompiledRegionalRenownDatabase( compiled, indexByAreaId );
		}

		private static RenownTier ResolveTier( float score )
		{
			if ( score < Constants.RENOWN_REQUIREMENT_TIER_1 ) {
				return RenownTier.WhisperInTheWinds;
			}

			if ( score < Constants.RENOWN_REQUIREMENT_TIER_2 ) {
				return RenownTier.TavernTalk;
			}

			if ( score < Constants.RENOWN_REQUIREMENT_TIER_3 ) {
				return RenownTier.HeroFromTheHills;
			}

			if ( score < Constants.RENOWN_REQUIREMENT_TIER_4 ) {
				return RenownTier.LocalLegend;
			}

			if ( score < Constants.RENOWN_REQUIREMENT_TIER_5 ) {
				return RenownTier.WanderingWarrior;
			}

			return RenownTier.TheNomad;
		}
	};
};
