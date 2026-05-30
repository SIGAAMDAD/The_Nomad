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
    /// <summary>
    ///
    /// </summary>
    public static class PlayerProfileStatKeys
    {
        public const string MERCENARY_EXPERIENCE = "mp_merc_xp";
        public const string MERCENARY_LEVEL = "mp_merc_level";
        public const string MERCENARY_TIER = "mp_merc_tier";

        public const string MERCENARY_FACTION = "mp_faction";
        public const string MERCENARY_FACTION_STANDING = "mp_faction_standing";
        public const string MERCENARY_FACTION_RANK = "mp_faction_rank";

        public const string LIFETIME_KILLS = "mp_lifetime_kills";
        public const string LIFETIME_DEATHS = "mp_lifetime_deaths";
        public const string LIFETIME_ASSISTS = "mp_lifetime_assists";
        public const string LIFETIME_WINS = "mp_lifetime_wins";
        public const string LIFETIME_LOSSES = "mp_lifetime_losses";
        public const string LIFETIME_MATCHES = "mp_lifetime_matches";
        public const string LIFETIME_DAMAGE_DEALT = "mp_lifetime_damage_dealt";
        public const string LIFETIME_DAMAGE_TAKEN = "mp_lifetime_damage_taken";
        public const string BEST_KILL_STREAK = "mp_best_kill_streak";

        public const string CONTRACTS_COMPLETED = "mp_contracts_completed";
        public const string WEEKLY_CONTRACTS_COMPLETED = "mp_weekly_contracts_completed";
        public const string FACTION_CONTRACTS_COMPLETED = "mp_faction_contracts_completed";
        public const string NON_MEMBER_FACTION_CONTRACTS_COMPLETED = "mp_non_member_faction_contracts_completed";

        public const string DUEL_ROUND_WINS = "mp_duel_round_wins";
        public const string DUEL_ROUND_LOSSES = "mp_duel_round_losses";
        public const string DUEL_MATCH_WINS = "mp_duel_match_wins";
        public const string DUEL_MATCH_LOSSES = "mp_duel_match_losses";
        public const string DUEL_NO_DAMAGE_ROUNDS = "mp_duel_no_damage_rounds";
        public const string DUEL_NO_BULLET_TIME_ROUNDS = "mp_duel_no_bullet_time_rounds";

        public const string BOUNTIES_CLAIMED = "mp_bounties_claimed";
        public const string BOUNTY_TARGETS_KILLED = "mp_bounty_targets_killed";
        public const string BOUNTY_HUNTS_SURVIVED = "mp_bounty_hunts_survived";

        public const string EXTRACTIONS_COMPLETED = "mp_extractions_completed";
        public const string EXTRACTIONS_FAILED = "mp_extractions_failed";

        public const string FLAGS_CAPTURED = "mp_flags_captured";
        public const string FLAGS_RETURNED = "mp_flags_returned";
        public const string HILL_SECONDS_HELD = "mp_hill_seconds_held";

        public const string PARRY_KILLS = "mp_parry_kills";
        public const string HEAVY_PARRY_SUCCESSES = "mp_heavy_parry_successes";
        public const string GRAB_PARRY_SUCCESSES = "mp_grab_parry_successes";
        public const string DASH_OVERHEAT_KILLS = "mp_dash_overheat_kills";

        public const string NO_DAMAGE_WINS = "mp_no_damage_wins";
        public const string NO_DEATH_WINS = "mp_no_death_wins";
        public const string NO_BULLET_TIME_WINS = "mp_no_bullet_time_wins";
    }
}
