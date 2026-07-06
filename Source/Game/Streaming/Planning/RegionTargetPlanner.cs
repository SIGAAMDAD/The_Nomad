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
	internal sealed class RegionTargetPlanner
	{
		private readonly RegionStreamingSettings _settings;

		public RegionTargetPlanner( RegionStreamingSettings settings )
		{
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
		}

		public void RefreshTargets( RegionPrediction prediction, RegionStateTable state )
		{
			RegionRecord[] regions = state.Regions;

			for ( int i = 0; i < regions.Length; i++ ) {
				ref RegionRecord r = ref regions[i];
				RegionId id = new RegionId( r.X, r.Z );

				int currentDistance = RegionGrid.Chebyshev( id, prediction.CurrentRegion );
				int predictedDistance = RegionGrid.Chebyshev( id, prediction.PredictedRegion );

				RegionFlags oldTarget = r.Flags & RegionFlags.TargetMask;

				RegionFlags target = ComputeTarget(
					currentDistance,
					predictedDistance,
					prediction.HasMeaningfulVelocity,
					oldTarget
				);

				r.Distance = (byte)Math.Clamp( currentDistance, 0, byte.MaxValue );
				r.PredictedDistance = (byte)Math.Clamp( predictedDistance, 0, byte.MaxValue );

				if ( target != oldTarget ) {
					r.Flags = (r.Flags & ~RegionFlags.TargetMask) | target;
				}

				if ( currentDistance <= _settings.ColdRadius ) {
					r.Flags |= RegionFlags.IsNear;
				} else {
					r.Flags &= ~RegionFlags.IsNear;
				}

				if ( predictedDistance < currentDistance ) {
					r.Flags |= RegionFlags.IsPredicted;
				} else {
					r.Flags &= ~RegionFlags.IsPredicted;
				}
			}
		}

		private RegionFlags ComputeTarget(
			int currentDistance,
			int predictedDistance,
			bool usePrediction,
			RegionFlags currentTarget
		)
		{
			RegionFlags current = TargetFromDistance( currentDistance, currentTarget );

			if ( !usePrediction ) {
				return current;
			}

			RegionFlags predicted = TargetFromDistance( predictedDistance + 1, currentTarget );

			// Prediction may preload, but it should not activate physics/gameplay before arrival.
			if ( (predicted & RegionFlags.TargetActive) != 0 ) {
				predicted = RegionFlags.TargetHot;
			}

			return TargetRank( predicted ) > TargetRank( current ) ? predicted : current;
		}

		private RegionFlags TargetFromDistance( int distance, RegionFlags currentTarget )
		{
			int hysteresis = _settings.HysteresisRings;

			if ( (currentTarget & RegionFlags.TargetActive) != 0 && distance <= _settings.ActiveRadius + hysteresis ) {
				return RegionFlags.TargetActive;
			}

			if ( (currentTarget & RegionFlags.TargetHot) != 0 && distance <= _settings.HotRadius + hysteresis ) {
				return RegionFlags.TargetHot;
			}

			if ( (currentTarget & RegionFlags.TargetWarm) != 0 && distance <= _settings.WarmRadius + hysteresis ) {
				return RegionFlags.TargetWarm;
			}

			if ( (currentTarget & RegionFlags.TargetCold) != 0 && distance <= _settings.ColdRadius + hysteresis ) {
				return RegionFlags.TargetCold;
			}

			if ( distance <= _settings.ActiveRadius ) {
				return RegionFlags.TargetActive;
			}

			if ( distance <= _settings.HotRadius ) {
				return RegionFlags.TargetHot;
			}

			if ( distance <= _settings.WarmRadius ) {
				return RegionFlags.TargetWarm;
			}

			if ( distance <= _settings.ColdRadius ) {
				return RegionFlags.TargetCold;
			}

			return RegionFlags.TargetUnloaded;
		}

		public static int TargetRank( RegionFlags target )
		{
			if ( (target & RegionFlags.TargetActive) != 0 ) {
				return 4;
			}

			if ( (target & RegionFlags.TargetHot) != 0 ) {
				return 3;
			}

			if ( (target & RegionFlags.TargetWarm) != 0 ) {
				return 2;
			}

			if ( (target & RegionFlags.TargetCold) != 0 ) {
				return 1;
			}

			return 0;
		}
	};
};
