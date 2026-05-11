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

namespace Nomad.Game.Domain.Data.Multiplayer.Profile
{
	public sealed record FactionRewardRecord
	{
		public string RewardId { get; init; }
		public FactionId FactionId { get; init; }
		public RewardType Type { get; init; }
		public string Name { get; init; }

		public string? Description { get; init; }

		/// <summary>
		/// Minimum faction rank required.
		/// </summary>
		public byte RequiredRankIndex { get; init; }

		/// <summary>
		/// Minimum faction tier required.
		/// </summary>
		public MercTier RequiredTier { get; init; }

		/// <summary>
		/// If true, the reward is only usable while representing this faction.
		/// If false, permanently unlocked once earned.
		/// </summary>
		public bool RequiresFactionEquipped { get; init; }
	};
};
