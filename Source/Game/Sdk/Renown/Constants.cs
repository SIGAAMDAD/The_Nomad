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

using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Renown
{
    public static class Constants
    {
        public const float MIN_RENOWN = 0.0f;
        public const float MAX_RENOWN = 2500.0f;

        /// <summary>
        /// "Whisper in the Winds" renown tier score requirement.
        /// </summary>
        public const float RENOWN_REQUIREMENT_TIER_0 = 0.0f;
        public static readonly RenownTierDefinition TIER_0_DEFINITION = new RenownTierDefinition(
            id: new InternString("renown.tier0.id"),
            tier: RenownTier.WhisperInTheWinds,
            minimumRenownScore: MIN_RENOWN,
            maximumRenownScore: RENOWN_REQUIREMENT_TIER_1
        );

        /// <summary>
        /// "Tavern Talk" renown tier score requirement.
        /// </summary>
        public const float RENOWN_REQUIREMENT_TIER_1 = 50.0f;
        public static readonly RenownTierDefinition TIER_1_DEFINITION = new RenownTierDefinition(
            id: new InternString("renown.tier1.id"),
            tier: RenownTier.TavernTalk,
            minimumRenownScore: RENOWN_REQUIREMENT_TIER_1,
            maximumRenownScore: RENOWN_REQUIREMENT_TIER_2
        );

        /// <summary>
        /// "Hero from the Hills" renown tier score requirement.
        /// </summary>
        public const float RENOWN_REQUIREMENT_TIER_2 = 250.0f;
        public static readonly RenownTierDefinition TIER_2_DEFINITION = new RenownTierDefinition(
            id: new InternString("renown.tier2.id"),
            tier: RenownTier.HeroFromTheHills,
            minimumRenownScore: RENOWN_REQUIREMENT_TIER_2,
            maximumRenownScore: RENOWN_REQUIREMENT_TIER_3
        );

        /// <summary>
        /// "Local Legend" renown tier score requirement.
        /// </summary>
        public const float RENOWN_REQUIREMENT_TIER_3 = 500.0f;
        public static readonly RenownTierDefinition TIER_3_DEFINITION = new RenownTierDefinition(
            id: new InternString("renown.tier3.id"),
            tier: RenownTier.LocalLegend,
            minimumRenownScore: RENOWN_REQUIREMENT_TIER_3,
            maximumRenownScore: RENOWN_REQUIREMENT_TIER_4
        );

        /// <summary>
        /// "Wandering Warrior" renown tier score requirement.
        /// </summary>
        public const float RENOWN_REQUIREMENT_TIER_4 = 900.0f;
        public static readonly RenownTierDefinition TIER_4_DEFINITION = new RenownTierDefinition(
            id: new InternString("renown.tier4.id"),
            tier: RenownTier.WanderingWarrior,
            minimumRenownScore: RENOWN_REQUIREMENT_TIER_4,
            maximumRenownScore: RENOWN_REQUIREMENT_TIER_5
        );

        /// <summary>
        /// "The Nomad" renown tier score requirement.
        /// </summary>
        public const float RENOWN_REQUIREMENT_TIER_5 = 1500.0f;
        public static readonly RenownTierDefinition TIER_5_DEFINITION = new RenownTierDefinition(
            id: new InternString("renown.tier5.id"),
            tier: RenownTier.TheNomad,
            minimumRenownScore: RENOWN_REQUIREMENT_TIER_5,
            maximumRenownScore: MAX_RENOWN
        );

        public const float OUTSTANDING_TRAIT_SCORE = 65.0f;
    }
}
