/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using Nomad.Game.Gameplay.Runtime.Renown;
using Nomad.Game.Sdk.Renown.Regional;

namespace Nomad.Game.Gameplay.Renown.Regional
{
    internal class RegionalRenownRuntimeState
    {
        private readonly RenownStatus[] _statuses;

        public int Count => _statuses.Length;

        public RegionalRenownRuntimeState(CompiledRegionalRenownDatabase? database)
        {
            database ??= CompiledRegionalRenownDatabase.Empty;
            int count = database.Count;
            _statuses = new RenownStatus[count];

            for (ushort i = 0; i < _statuses.Length; i++) {
                ref readonly CompiledRegionalRenownDefinition definition = ref database.GetByIndex(i);

                _statuses[i] = new RenownStatus
                {
                    Region = definition.AreaId,
                    CurrentRenown = definition.InitialRenownScore,
                    PeakRenown = definition.InitialRenownScore,
                    CurrentTier = ResolveTierDefinition(definition.InitialTier),
                    HighestTierReached = ResolveTierDefinition(definition.InitialTier),
                    DecayFloor = definition.HistoricalFloor
                };
            }
        }

        public RenownStatus GetStatus(ushort index)
        {
            return _statuses[index];
        }

        public ReadOnlySpan<RenownStatus> Statuses => _statuses;

        private static RenownTierDefinition ResolveTierDefinition(RenownTier tier)
        {
            switch (tier) {
                case RenownTier.WhisperInTheWinds:
                    return Constants.TIER_0_DEFINITION;
                case RenownTier.TavernTalk:
                    return Constants.TIER_1_DEFINITION;
                case RenownTier.HeroFromTheHills:
                    return Constants.TIER_2_DEFINITION;
                case RenownTier.LocalLegend:
                    return Constants.TIER_3_DEFINITION;
                case RenownTier.WanderingWarrior:
                    return Constants.TIER_4_DEFINITION;
                case RenownTier.TheNomad:
                    return Constants.TIER_5_DEFINITION;
                default:
                    return Constants.TIER_0_DEFINITION;
            }
        }
    }
}
