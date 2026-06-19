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

using Godot;

namespace Nomad.Game.Infrastructure.Streaming
{
	internal readonly struct RegionPrediction
	{
		public readonly RegionId CurrentRegion;
		public readonly RegionId PredictedRegion;

		public readonly Vector2 Direction;
		public readonly float SpeedMetersPerSecond;
		public readonly float LookaheadMeters;

		public readonly bool HasMeaningfulVelocity;

		public RegionPrediction(
			RegionId currentRegion,
			RegionId predictedRegion,
			Vector2 direction,
			float speedMetersPerSecond,
			float lookaheadMeters,
			bool hasMeaningfulVelocity
		)
		{
			CurrentRegion = currentRegion;
			PredictedRegion = predictedRegion;
			Direction = direction;
			SpeedMetersPerSecond = speedMetersPerSecond;
			LookaheadMeters = lookaheadMeters;
			HasMeaningfulVelocity = hasMeaningfulVelocity;
		}
	};
};
