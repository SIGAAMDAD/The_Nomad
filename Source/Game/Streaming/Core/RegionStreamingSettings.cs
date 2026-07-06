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

namespace Nomad.Game.Streaming
{
	internal sealed class RegionStreamingSettings
	{
		public int RegionSizeMeters = 512;

		// Centered 32x32 grid. Terrain3D region map addresses are signed locations.
		public int MinRegionX = -16;
		public int MinRegionZ = -16;

		public byte ActiveRadius = 1;
		public byte HotRadius = 3;
		public byte WarmRadius = 5;
		public byte ColdRadius = 7;

		// Keep an existing tier for one extra ring before demoting.
		public byte HysteresisRings = 1;

		public float MinPredictionSeconds = 1.0f;
		public float MaxPredictionSeconds = 4.0f;
		public float MinMeaningfulSpeed = 1.0f;
		public float MaxLookaheadRegions = 3.0f;
		public float IdleCameraLookaheadRegions = 0.5f;
		public float InputLookaheadRegions = 1.0f;

		// Fallback used when project-level movement constants are not injected.
		public float MaxExpectedMoveSpeed = 10.0f;

		public int DefaultPromotionBudget = 1;
		public int DefaultDemotionBudget = 2;

		public bool EnableHotLighting = true;
		public bool EnableHotAudio = true;
		public bool EnableActiveShadows = true;

		public void Validate()
		{
			if ( RegionSizeMeters <= 0 ) {
				throw new InvalidOperationException( "RegionSizeMeters must be greater than zero." );
			}

			if ( ActiveRadius > HotRadius || HotRadius > WarmRadius || WarmRadius > ColdRadius ) {
				throw new InvalidOperationException( "Streaming radii must satisfy Active <= Hot <= Warm <= Cold." );
			}

			if ( MinPredictionSeconds < 0.0f || MaxPredictionSeconds < MinPredictionSeconds ) {
				throw new InvalidOperationException( "Invalid prediction second range." );
			}

			if ( MaxLookaheadRegions < 0.0f || IdleCameraLookaheadRegions < 0.0f || InputLookaheadRegions < 0.0f ) {
				throw new InvalidOperationException( "Lookahead regions cannot be negative." );
			}
		}
	};
};
