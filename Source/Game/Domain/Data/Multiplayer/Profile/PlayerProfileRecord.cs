using System;
using System.Collections.Generic;

namespace Nomad.Game.Domain.Data.Multiplayer.Profile
{
	public sealed record PlayerProfileRecord
	{
		public Guid PlayerId { get; init; }

		public string DisplayName { get; init; }

		/// <summary>
		/// Global multiplayer XP. This is not faction-specific.
		/// </summary>
		public ulong MercenaryExperience { get; init; }

		/// <summary>
		/// Global mercenary level derived from MercenaryExperience.
		/// </summary>
		public ushort MercenaryLevel { get; init; }

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
	};
};
