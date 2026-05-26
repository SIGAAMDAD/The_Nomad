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
using Nomad.Core.OnlineServices;

namespace Nomad.Game.Sdk.Multiplayer.Profile
{
	public sealed record PlayerProfileRecord
	{
		public PeerId PlayerId { get; init; }

		/// <summary>
		/// A player's in-game username.
		/// </summary>
		public string Callsign { get; init; }

		public BanStatus LicenseStatus { get; init; }

		/// <summary>
		/// The faction the player is currently representing.
		/// Titanfall-style: a player can pledge to one faction for current progression flavor/rewards.
		/// </summary>
		public FactionId EquippedFaction { get; init; } = FactionId.None;

		/// <summary>
		/// Faction-specific progression records.
		/// Key: FactionId.
		/// </summary>
		public IReadOnlyDictionary<FactionId, PlayerFactionRecord> Factions { get; init; } =
			new Dictionary<FactionId, PlayerFactionRecord>();

		/// <summary>
		/// Multiplayer-only achievement/reputation badges earned by the player.
		/// </summary>
		public IReadOnlyDictionary<string, PlayerBadgeRecord> Badges { get; init; } =
			new Dictionary<string, PlayerBadgeRecord>();

		/// <summary>
		/// Unlocked cosmetics, titles, banners, executions, dyes, etc.
		/// </summary>
		public IReadOnlySet<string> UnlockedRewardIds { get; init; } =
			new HashSet<string>();

		public PlayerStatsRecord Stats { get; init; }

		public DateTimeOffset CreatedAtUtc { get; init; }
		public DateTimeOffset LastSeenUtc { get; init; }
	}
}
