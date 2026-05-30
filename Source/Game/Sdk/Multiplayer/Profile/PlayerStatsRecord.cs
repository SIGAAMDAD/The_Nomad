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

namespace Nomad.Game.Sdk.Multiplayer.Profile
{
    public sealed record PlayerStatsRecord
    {
        /// <summary>
        /// Lifetime damage dealt.
        /// </summary>
        public ulong TotalDamageDealt { get; init; }

        /// <summary>
        /// Lifetime damage taken.
        /// </summary>
        public ulong TotalDamageTaken { get; init; }

        /// <summary>
        ///
        /// </summary>
        public ulong BestKillStreak { get; init; }

        public ulong MatchesPlayed { get; init; }
        public ulong MatchesWon { get; init; }
        public ulong MatchesLost { get; init; }

        public ulong Kills { get; init; }
        public ulong Deaths { get; init; }
        public ulong Assists { get; init; }

        public ulong DuelRoundsWon { get; init; }
        public ulong DuelRoundsLost { get; init; }
        public ulong DuelMatchesWon { get; init; }
        public ulong DuelMatchesLost { get; init; }

        /// <summary>
        /// The number of duel rounds you have gone without taking hits.
        /// </summary>
        public ulong DuelNoDamageRounds { get; init; }

        /// <summary>
        /// The number of duel rounds you have gone without using bullet time.
        /// </summary>
        public ulong DuelNoBulletTimeRounds { get; init; }

        public ulong BountiesClaimed { get; init; }
        public ulong BountyTargetsKilled { get; init; }
        public ulong BountyHuntsSurvived { get; init; }

        public ulong ExtractionsCompleted { get; init; }
        public ulong ExtractionsFailed { get; init; }

        public ulong FlagsCaptured { get; init; }
        public ulong FlagsReturned { get; init; }

        public ulong HillSecondsHeld { get; init; }

        public ulong ParryKills { get; init; }
        public ulong HeavyParrySuccesses { get; init; }
        public ulong GrabParrySuccesses { get; init; }

        public ulong DashOverheatKills { get; init; }

        /// <summary>
        /// The amount of games won without taking any damage
        /// </summary>
        public ulong NoDamageWins { get; init; }

        /// <summary>
        /// The amount of games won without dying.
        /// </summary>
        public ulong NoDeathWins { get; init; }

        /// <summary>
        /// The amount of games won without utilizing bullet time at all.
        /// </summary>
        public ulong NoBulletTimeWins { get; init; }

        public ulong ContractsCompleted { get; init; }
        public ulong WeeklyContractsCompleted { get; init; }

        public ulong FactionContractsCompleted { get; init; }
        public ulong NonMemberFactionContractsCompleted { get; init; }
    }
}
