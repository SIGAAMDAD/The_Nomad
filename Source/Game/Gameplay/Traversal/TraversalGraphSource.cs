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

using Nomad.Game.Streaming;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Gameplay.Traversal
{
	internal sealed class TraversalGraphSource
	{
		public readonly int SourceId;
		public readonly RegionId RegionId;
		public readonly TraversalDatabase Database;
		public readonly TraversalGraph Graph;
		public readonly bool IsFallback;

		public TraversalGraphSource(
			int sourceId,
			RegionId regionId,
			TraversalDatabase database,
			TraversalGraph graph,
			bool isFallback
		)
		{
			SourceId = sourceId;
			RegionId = regionId;
			Database = database;
			Graph = graph;
			IsFallback = isFallback;
		}
	}
}
