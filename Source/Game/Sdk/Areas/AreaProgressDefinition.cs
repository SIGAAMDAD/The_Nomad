/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System.Text.Json;

namespace Nomad.Game.Sdk.Areas
{
    /// <summary>
    /// Authored completion budget for map-progress style area tracking.
    /// </summary>
    public readonly struct AreaProgressDefinition
    {
        public readonly ushort LandmarkCount;
        public readonly ushort ActivityCount;
        public readonly ushort CollectibleCount;
        public readonly ushort FastTravelPointCount;

        public ushort TotalCount => (ushort)(LandmarkCount + ActivityCount + CollectibleCount + FastTravelPointCount);

        public AreaProgressDefinition(
            ushort landmarkCount,
            ushort activityCount,
            ushort collectibleCount,
            ushort fastTravelPointCount)
        {
            LandmarkCount = landmarkCount;
            ActivityCount = activityCount;
            CollectibleCount = collectibleCount;
            FastTravelPointCount = fastTravelPointCount;
        }

        public static AreaProgressDefinition Empty => new AreaProgressDefinition(0, 0, 0, 0);

        public static AreaProgressDefinition Load(JsonElement json)
        {
            return new AreaProgressDefinition(
                ReadUInt16(json, nameof(LandmarkCount)),
                ReadUInt16(json, nameof(ActivityCount)),
                ReadUInt16(json, nameof(CollectibleCount)),
                ReadUInt16(json, nameof(FastTravelPointCount))
            );
        }

        private static ushort ReadUInt16(JsonElement json, string name)
        {
            if (json.TryGetProperty(name, out JsonElement value) && value.TryGetUInt16(out ushort result)) {
                return result;
            }

            return 0;
        }
    }
}
