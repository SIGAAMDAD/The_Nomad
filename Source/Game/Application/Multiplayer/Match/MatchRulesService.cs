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
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Multiplayer.Match;

namespace Nomad.Game.Application.Multiplayer.Match
{
	/*
	===================================================================================

	MatchRulesService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class MatchRulesService : IMatchRulesService
	{
		public MatchRules Current {
			get {
				if ( _current == null ) {
					throw new InvalidOperationException( "Match rules have not been set." );
				}

				return _current;
			}
		}
		private MatchRules? _current;

		public bool HasRules => _current != null;

		public void SetRules( MatchRules rules )
		{
			MatchRulesValidationResult result = ValidateOrThrow( rules );

			if ( !result.IsSuccess ) {
				throw new InvalidOperationException( result.Error );
			}

			_current = Clone( rules );
		}

		public MatchRules CreateDefaultRules( MultiplayerMode mode, string mapId )
		{
			MatchRules rules = new MatchRules {
				Mode = mode,
				MapId = mapId ?? string.Empty
			};

			switch ( mode ) {
				case MultiplayerMode.Duel:
					rules.MaxPlayers = 2;
					rules.MaxTeams = 0;
					rules.ScoreLimit = 3;
					rules.RoundLimit = 5;
					rules.AllowRespawns = false;
					rules.AllowLateJoin = false;
					rules.CountdownTicks = 180;
					rules.RoundDurationTicks = 0;
					break;

				case MultiplayerMode.Deathmatch:
					rules.MaxPlayers = 16;
					rules.MaxTeams = 0;
					rules.ScoreLimit = 20;
					rules.RoundLimit = 1;
					rules.AllowRespawns = true;
					rules.MatchDurationTicks = 60 * 5 * 60;
					break;

				case MultiplayerMode.TeamBrawl:
					rules.MaxPlayers = 16;
					rules.MaxTeams = 2;
					rules.ScoreLimit = 30;
					rules.RoundLimit = 1;
					rules.AllowRespawns = true;
					rules.MatchDurationTicks = 60 * 8 * 60;
					break;

				case MultiplayerMode.KingOfTheHill:
					rules.MaxPlayers = 16;
					rules.MaxTeams = 2;
					rules.ScoreLimit = 100;
					rules.RoundLimit = 1;
					rules.AllowRespawns = true;
					rules.MatchDurationTicks = 60 * 10 * 60;
					break;

				case MultiplayerMode.CaptureTheFlag:
					rules.MaxPlayers = 16;
					rules.MaxTeams = 2;
					rules.ScoreLimit = 3;
					rules.RoundLimit = 1;
					rules.AllowRespawns = true;
					rules.MatchDurationTicks = 60 * 5 * 60;
					break;

				default:
					rules.MaxPlayers = 8;
					rules.MaxTeams = 0;
					rules.ScoreLimit = 20;
					rules.RoundLimit = 1;
					rules.AllowRespawns = true;
					break;
			}

			return rules;
		}

		public MatchRulesValidationResult ValidateOrThrow( MatchRules? rules )
		{
			if ( rules == null ) {
				return MatchRulesValidationResult.Fail( "Match rules are null." );
			}

			if ( rules.Mode == MultiplayerMode.None ) {
				return MatchRulesValidationResult.Fail( "Match mode cannot be None." );
			}

			if ( string.IsNullOrWhiteSpace( rules.MapId ) ) {
				return MatchRulesValidationResult.Fail( "MapId cannot be empty." );
			}

			if ( rules.MaxPlayers == 0 ) {
				return MatchRulesValidationResult.Fail( "MaxPlayers must be greater than zero." );
			}

			if ( rules.RoundLimit == 0 ) {
				return MatchRulesValidationResult.Fail( "RoundLimit must be greater than zero." );
			}

			if ( rules.Mode == MultiplayerMode.Duel && rules.MaxPlayers != 2 ) {
				return MatchRulesValidationResult.Fail( "Duel requires exactly two players." );
			}

			if ( (rules.Mode == MultiplayerMode.TeamBrawl ||
				 rules.Mode == MultiplayerMode.KingOfTheHill ||
				 rules.Mode == MultiplayerMode.CaptureTheFlag) &&
				rules.MaxTeams < 2
			) {
				return MatchRulesValidationResult.Fail( "Team modes require at least two teams." );
			}

			if ( rules.ScoreLimit == 0 &&
				rules.MatchDurationTicks == 0 &&
				rules.RoundDurationTicks == 0
			) {
				return MatchRulesValidationResult.Fail(
					"At least one win/end condition must exist: score limit, match duration, or round duration."
				);
			}

			return MatchRulesValidationResult.Ok();
		}

		public bool IsRespawnAllowed()
		{
			return Current.AllowRespawns;
		}

		public bool IsSpectatingAllowed()
		{
			return Current.AllowSpectators;
		}

		public bool IsLateJoinAllowed()
		{
			return Current.AllowLateJoin;
		}

		public bool IsFriendlyFireEnabled()
		{
			return Current.FriendlyFire;
		}

		public bool IsRanked()
		{
			return Current.Ranked;
		}

		public bool IsScoreLimitReached( int score )
		{
			return Current.ScoreLimit > 0 && score >= Current.ScoreLimit;
		}

		public bool IsRoundLimitReached( byte roundIndex )
		{
			return Current.RoundLimit > 0 && roundIndex >= Current.RoundLimit;
		}

		public bool IsMatchTimeLimitEnabled()
		{
			return Current.MatchDurationTicks > 0;
		}

		public bool IsRoundTimeLimitEnabled()
		{
			return Current.RoundDurationTicks > 0;
		}

		private static MatchRules Clone( MatchRules rules )
		{
			return new MatchRules {
				Mode = rules.Mode,
				MapId = rules.MapId,
				MaxPlayers = rules.MaxPlayers,
				MaxTeams = rules.MaxTeams,
				ScoreLimit = rules.ScoreLimit,
				RoundLimit = rules.RoundLimit,
				MatchDurationTicks = rules.MatchDurationTicks,
				RoundDurationTicks = rules.RoundDurationTicks,
				WarmupTicks = rules.WarmupTicks,
				CountdownTicks = rules.CountdownTicks,
				PostMatchTicks = rules.PostMatchTicks,
				FriendlyFire = rules.FriendlyFire,
				AllowRespawns = rules.AllowRespawns,
				AllowSpectators = rules.AllowSpectators,
				AllowLateJoin = rules.AllowLateJoin,
				LockLoadoutsOnStart = rules.LockLoadoutsOnStart,
				Ranked = rules.Ranked
			};
		}
	};
};
