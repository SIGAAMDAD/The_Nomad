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
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer.Match;

namespace Nomad.Game.Domain.Interfaces.Multiplayer
{
	public interface IMatchRulesService
	{
		MatchRules? Current { get; }
		bool HasRules { get; }

		void SetRules( MatchRules rules );
		MatchRules CreateDefaultRules( Mode mode, string mapId );

		[ResultObject( "MatchRulesValidationResult", Namespace = "Nomad.Game.Domain.Data.Multiplayer.Match" )]
		[ResultObjectPayload( "Error", typeof( string ), order: 1 )]
		[ResultObjectSuccess( MethodName = "Ok" )]
		[ResultObjectFailure( "Error", MethodName = "Fail" )]
		MatchRulesValidationResult ValidateOrThrow( MatchRules rules );

		bool IsRespawnAllowed();
		bool IsSpectatingAllowed();
		bool IsLateJoinAllowed();
		bool IsFriendlyFireEnabled();
		bool IsRanked();
		bool IsScoreLimitReached( int score );
		bool IsRoundLimitReached( byte roundIndex );
		bool IsMatchTimeLimitEnabled();
		bool IsRoundTimeLimitEnabled();
	};
};
