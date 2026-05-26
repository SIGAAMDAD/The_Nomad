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

namespace Nomad.Game.Sdk.Multiplayer.Profile
{
	public sealed record WeeklyContractDefinitionRecord
	{
		public string ContractId { get; init; }

		public FactionId FactionId { get; init; }

		public string Name { get; init; }

		public string Description { get; init; }

		public ContractRefreshCadence Cadence { get; init; }

		public IReadOnlyList<ContractObjectiveRecord> Objectives { get; init; }

		/// <summary>
		/// Global mercenary XP granted on completion.
		/// </summary>
		public uint MercenaryExperienceReward { get; init; }

		/// <summary>
		/// Standing gained when the player is currently representing this faction.
		/// </summary>
		public uint MemberStandingReward { get; init; }

		/// <summary>
		/// Standing gained when the player completes the contract while not pledged to this faction.
		/// </summary>
		public uint NonMemberStandingReward { get; init; }

		public IReadOnlyList<FactionRewardRecord> BonusRewards { get; init; }

		public DateTimeOffset? StartsAtUtc { get; init; }

		public DateTimeOffset? ExpiresAtUtc { get; init; }
	}
}
