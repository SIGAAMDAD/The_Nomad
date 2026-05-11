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

namespace Nomad.Game.Domain.Data.Multiplayer.Profile
{
	public sealed record PlayerFactionRecord
	{
		public FactionId FactionId { get; init; }

		/// <summary>
		/// Faction reputation points.
		/// Increased by representing the faction, completing faction contracts,
		/// and completing non-member contracts at reduced standing gain.
		/// </summary>
		public ulong Standing { get; init; }

		/// <summary>
		/// Rank index inside this faction's reward track.
		/// Example: GnG Chicken -> Jockey -> Golden Boy -> Mercenary Master.
		/// </summary>
		public byte RankIndex { get; init; }

		/// <summary>
		/// Derived from RankIndex or Standing.
		/// Used only for faction reward gates, not global player level.
		/// </summary>
		public MercTier MercTier { get; init; }

		/// <summary>
		/// Weekly faction contract currently accepted from this faction.
		/// Null if none.
		/// </summary>
		public ActiveWeeklyContractRecord? ActiveWeeklyContract { get; init; }

		/// <summary>
		/// Last weekly contract completed for this faction.
		/// Useful for preventing repeat reward exploits.
		/// </summary>
		public string? LastCompletedWeeklyContractId { get; init; }

		public DateTimeOffset? JoinedAtUtc { get; init; }

		public DateTimeOffset? LastContractCompletedAtUtc { get; init; }
	};
};
