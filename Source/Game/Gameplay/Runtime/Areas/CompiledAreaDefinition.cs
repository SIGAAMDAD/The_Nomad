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
using Nomad.Game.Sdk.Areas;

namespace Nomad.Game.Gameplay.Runtime.Areas
{
	internal readonly struct CompiledAreaDefinition
	{
		public readonly AreaDefinitionId Id;
		public readonly ushort AreaIndex;
		public readonly AreaKind Kind;
		public readonly AreaDefinitionId ParentAreaId;
		public readonly ushort ParentAreaIndex;
		public readonly ushort FirstBiomeLayer;
		public readonly ushort BiomeLayerCount;
		public readonly ushort LandmarkCount;
		public readonly ushort ActivityCount;
		public readonly ushort CollectibleCount;
		public readonly ushort FastTravelPointCount;
		public readonly AreaFlags Flags;
		public readonly InternString Name;
		public readonly InternString WikiId;
		public readonly InternString JournalEntryDiscoveredId;
		public readonly InternString JournalEntryUndiscoveredId;

		public ushort ProgressTotal => (ushort)(LandmarkCount + ActivityCount + CollectibleCount + FastTravelPointCount);

		public CompiledAreaDefinition(
			AreaDefinitionId id,
			ushort areaIndex,
			AreaKind kind,
			AreaDefinitionId parentAreaId,
			ushort parentAreaIndex,
			ushort firstBiomeLayer,
			ushort biomeLayerCount,
			ushort landmarkCount,
			ushort activityCount,
			ushort collectibleCount,
			ushort fastTravelPointCount,
			AreaFlags flags,
			InternString name,
			InternString wikiId,
			InternString journalEntryDiscoveredId,
			InternString journalEntryUndiscoveredId
		)
		{
			Id = id;
			AreaIndex = areaIndex;
			Kind = kind;
			ParentAreaId = parentAreaId;
			ParentAreaIndex = parentAreaIndex;
			FirstBiomeLayer = firstBiomeLayer;
			BiomeLayerCount = biomeLayerCount;
			LandmarkCount = landmarkCount;
			ActivityCount = activityCount;
			CollectibleCount = collectibleCount;
			FastTravelPointCount = fastTravelPointCount;
			Flags = flags;
			Name = name;
			WikiId = wikiId;
			JournalEntryDiscoveredId = journalEntryDiscoveredId;
			JournalEntryUndiscoveredId = journalEntryUndiscoveredId;
		}
	};
};
