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

namespace Nomad.Game.Streaming
{
	internal struct RegionRecord
	{
		public ushort Index;

		// Signed Terrain3D-compatible region location, e.g. -16..15 for a centered 32x32 world.
		public short X;
		public short Z;

		public RegionFlags Flags;

		public byte Distance;
		public byte PredictedDistance;

		public ushort LastTouchedTick;
		public ushort LastUsedTick;

		public ushort EstimatedCost;
	};
};
