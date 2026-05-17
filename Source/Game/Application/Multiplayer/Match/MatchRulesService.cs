using System;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer.Match;
using Nomad.Game.Domain.Interfaces.Multiplayer;

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

		public MatchRules CreateDefaultRules( Mode mode, string mapId )
		{
			MatchRules rules = new MatchRules {
				Mode = mode,
				MapId = mapId ?? string.Empty
			};

			switch ( mode ) {
				case Mode.Duel:
					rules.MaxPlayers = 2;
					rules.MaxTeams = 0;
					rules.ScoreLimit = 3;
					rules.RoundLimit = 5;
					rules.AllowRespawns = false;
					rules.AllowLateJoin = false;
					rules.CountdownTicks = 180;
					rules.RoundDurationTicks = 0;
					break;

				case Mode.Deathmatch:
					rules.MaxPlayers = 16;
					rules.MaxTeams = 0;
					rules.ScoreLimit = 20;
					rules.RoundLimit = 1;
					rules.AllowRespawns = true;
					rules.MatchDurationTicks = 60 * 5 * 60;
					break;

				case Mode.TeamBrawl:
					rules.MaxPlayers = 16;
					rules.MaxTeams = 2;
					rules.ScoreLimit = 30;
					rules.RoundLimit = 1;
					rules.AllowRespawns = true;
					rules.MatchDurationTicks = 60 * 8 * 60;
					break;

				case Mode.KingOfTheHill:
					rules.MaxPlayers = 16;
					rules.MaxTeams = 2;
					rules.ScoreLimit = 100;
					rules.RoundLimit = 1;
					rules.AllowRespawns = true;
					rules.MatchDurationTicks = 60 * 10 * 60;
					break;

				case Mode.CaptureTheFlag:
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

			if ( rules.Mode == Mode.None ) {
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

			if ( rules.Mode == Mode.Duel && rules.MaxPlayers != 2 ) {
				return MatchRulesValidationResult.Fail( "Duel requires exactly two players." );
			}

			if ( (rules.Mode == Mode.TeamBrawl ||
				 rules.Mode == Mode.KingOfTheHill ||
				 rules.Mode == Mode.CaptureTheFlag) &&
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
