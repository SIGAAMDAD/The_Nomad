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

namespace Nomad.Game.Domain.Data.Multiplayer.Profile.Badges
{
	public sealed record BadgeDefinition
	{
		public string BadgeId { get; init; }
		public string Name { get; init; }
		public string Description { get; init; }
		public BadgeCategory Category { get; init; }
		public BadgeRarity Rarity { get; init; }

		/// <summary>
		/// Optional faction that owns or grants this badge.
		/// Null means neutral/global multiplayer badge.
		/// </summary>
		public FactionId? FactionId { get; init; }

		/// <summary>
		/// Hidden badges are not shown until discovered/unlocked.
		/// </summary>
		public bool Hidden { get; init; }

		/// <summary>
		/// Cosmetic/reward unlocked with the badge, if any.
		/// </summary>
		public string? GrantedRewardId { get; init; }
	};
};
