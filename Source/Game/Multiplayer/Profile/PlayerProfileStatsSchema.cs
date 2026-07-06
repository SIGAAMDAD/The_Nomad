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

using Nomad.Game.Sdk.Multiplayer.Profile;

namespace Nomad.Game.Multiplayer.Profile
{
	internal static class PlayerProfileStatsSchema
	{
		public static PlayerProfileStatDescriptor[] Stats => _stats;

		private static readonly PlayerProfileStatDescriptor[] _stats = {
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.TotalDamageDealt ), PlayerProfileStatKeys.LIFETIME_DAMAGE_DEALT, s => s.TotalDamageDealt, (s, v) => s with { TotalDamageDealt = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.TotalDamageTaken ), PlayerProfileStatKeys.LIFETIME_DAMAGE_TAKEN, s => s.TotalDamageTaken, (s, v) => s with { TotalDamageTaken = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.BestKillStreak ), PlayerProfileStatKeys.BEST_KILL_STREAK, s => s.BestKillStreak, (s, v) => s with { BestKillStreak = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.MatchesPlayed ), PlayerProfileStatKeys.LIFETIME_MATCHES, s => s.MatchesPlayed, (s, v) => s with { MatchesPlayed = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.MatchesWon ), PlayerProfileStatKeys.LIFETIME_WINS, s => s.MatchesWon, (s, v) => s with { MatchesWon = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.MatchesLost ), PlayerProfileStatKeys.LIFETIME_LOSSES, s => s.MatchesLost, (s, v) => s with { MatchesLost = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.Kills ), PlayerProfileStatKeys.LIFETIME_KILLS, s => s.Kills, (s, v) => s with { Kills = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.Deaths ), PlayerProfileStatKeys.LIFETIME_DEATHS, s => s.Deaths, (s, v) => s with { Deaths = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.Assists ), PlayerProfileStatKeys.LIFETIME_ASSISTS, s => s.Assists, (s, v) => s with { Assists = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.DuelRoundsWon ), PlayerProfileStatKeys.DUEL_ROUND_WINS, s => s.DuelRoundsWon, (s, v) => s with { DuelRoundsWon = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.DuelRoundsLost ), PlayerProfileStatKeys.DUEL_ROUND_LOSSES, s => s.DuelRoundsLost, (s, v) => s with { DuelRoundsLost = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.DuelMatchesWon ), PlayerProfileStatKeys.DUEL_MATCH_WINS, s => s.DuelMatchesWon, (s, v) => s with { DuelMatchesWon = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.DuelMatchesLost ), PlayerProfileStatKeys.DUEL_MATCH_LOSSES, s => s.DuelMatchesLost, (s, v) => s with { DuelMatchesLost = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.DuelNoDamageRounds ), PlayerProfileStatKeys.DUEL_NO_DAMAGE_ROUNDS, s => s.DuelNoDamageRounds, (s, v) => s with { DuelNoDamageRounds = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.DuelNoBulletTimeRounds ), PlayerProfileStatKeys.DUEL_NO_BULLET_TIME_ROUNDS, s => s.DuelNoBulletTimeRounds, (s, v) => s with { DuelNoBulletTimeRounds = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.BountiesClaimed ), PlayerProfileStatKeys.BOUNTIES_CLAIMED, s => s.BountiesClaimed, (s, v) => s with { BountiesClaimed = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.BountyTargetsKilled ), PlayerProfileStatKeys.BOUNTY_TARGETS_KILLED, s => s.BountyTargetsKilled, (s, v) => s with { BountyTargetsKilled = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.BountyHuntsSurvived ), PlayerProfileStatKeys.BOUNTY_HUNTS_SURVIVED, s => s.BountyHuntsSurvived, (s, v) => s with { BountyHuntsSurvived = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.ExtractionsCompleted ), PlayerProfileStatKeys.EXTRACTIONS_COMPLETED, s => s.ExtractionsCompleted, (s, v) => s with { ExtractionsCompleted = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.ExtractionsFailed ), PlayerProfileStatKeys.EXTRACTIONS_FAILED, s => s.ExtractionsFailed, (s, v) => s with { ExtractionsFailed = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.FlagsCaptured ), PlayerProfileStatKeys.FLAGS_CAPTURED, s => s.FlagsCaptured, (s, v) => s with { FlagsCaptured = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.FlagsReturned ), PlayerProfileStatKeys.FLAGS_RETURNED, s => s.FlagsReturned, (s, v) => s with { FlagsReturned = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.HillSecondsHeld ), PlayerProfileStatKeys.HILL_SECONDS_HELD, s => s.HillSecondsHeld, (s, v) => s with { HillSecondsHeld = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.ParryKills ), PlayerProfileStatKeys.PARRY_KILLS, s => s.ParryKills, (s, v) => s with { ParryKills = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.HeavyParrySuccesses ), PlayerProfileStatKeys.HEAVY_PARRY_SUCCESSES, s => s.HeavyParrySuccesses, (s, v) => s with { HeavyParrySuccesses = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.GrabParrySuccesses ), PlayerProfileStatKeys.GRAB_PARRY_SUCCESSES, s => s.GrabParrySuccesses, (s, v) => s with { GrabParrySuccesses = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.DashOverheatKills ), PlayerProfileStatKeys.DASH_OVERHEAT_KILLS, s => s.DashOverheatKills, (s, v) => s with { DashOverheatKills = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.NoDamageWins ), PlayerProfileStatKeys.NO_DAMAGE_WINS, s => s.NoDamageWins, (s, v) => s with { NoDamageWins = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.NoDeathWins ), PlayerProfileStatKeys.NO_DEATH_WINS, s => s.NoDeathWins, (s, v) => s with { NoDeathWins = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.NoBulletTimeWins ), PlayerProfileStatKeys.NO_BULLET_TIME_WINS, s => s.NoBulletTimeWins, (s, v) => s with { NoBulletTimeWins = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.ContractsCompleted ), PlayerProfileStatKeys.CONTRACTS_COMPLETED, s => s.ContractsCompleted, (s, v) => s with { ContractsCompleted = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.WeeklyContractsCompleted ), PlayerProfileStatKeys.WEEKLY_CONTRACTS_COMPLETED, s => s.WeeklyContractsCompleted, (s, v) => s with { WeeklyContractsCompleted = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.FactionContractsCompleted ), PlayerProfileStatKeys.FACTION_CONTRACTS_COMPLETED, s => s.FactionContractsCompleted, (s, v) => s with { FactionContractsCompleted = v } ),
			new PlayerProfileStatDescriptor( nameof( PlayerStatsRecord.NonMemberFactionContractsCompleted ), PlayerProfileStatKeys.NON_MEMBER_FACTION_CONTRACTS_COMPLETED, s => s.NonMemberFactionContractsCompleted, (s, v) => s with { NonMemberFactionContractsCompleted = v } )
		};

		public static int ToOnlineValue( ulong value )
		{
			return value > int.MaxValue ? int.MaxValue : (int)value;
		}

		public static ulong FromOnlineValue( int value )
		{
			return value < 0 ? 0UL : (ulong)value;
		}
	};
};
