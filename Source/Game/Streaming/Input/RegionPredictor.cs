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
using Godot;

namespace Nomad.Game.Streaming
{
	internal sealed class RegionPredictor
	{
		private readonly RegionGrid _grid;
		private readonly RegionStreamingSettings _settings;

		public RegionPredictor( RegionGrid grid, RegionStreamingSettings settings )
		{
			_grid = grid ?? throw new ArgumentNullException( nameof( grid ) );
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
		}

		public RegionPrediction Build( StreamFocusState focus )
		{
			RegionId currentRegion = _grid.WorldToRegion( focus.Origin );

			float speed = focus.Velocity.Length();
			bool hasVelocity = speed >= _settings.MinMeaningfulSpeed;
			bool hasInput = focus.MoveInput.LengthSquared() > 0.01f;
			bool hasCameraForward = focus.CameraForward.LengthSquared() > 0.01f;

			Vector2 direction;

			if ( hasVelocity ) {
				direction = focus.Velocity.Normalized();
			} else if ( hasInput ) {
				direction = focus.MoveInput.Normalized();
			} else if ( hasCameraForward ) {
				direction = new Vector2( focus.CameraForward.X, focus.CameraForward.Z ).Normalized();
			} else {
				direction = Vector2.Zero;
			}

			float predictionSeconds = ComputePredictionSeconds();
			float lookaheadMeters = 0.0f;

			if ( hasVelocity ) {
				lookaheadMeters = speed * predictionSeconds;
			} else if ( hasInput ) {
				lookaheadMeters = _settings.RegionSizeMeters * _settings.InputLookaheadRegions;
			} else if ( hasCameraForward ) {
				lookaheadMeters = _settings.RegionSizeMeters * _settings.IdleCameraLookaheadRegions;
			}

			float maxLookaheadMeters = _settings.MaxLookaheadRegions * _settings.RegionSizeMeters;
			lookaheadMeters = MathF.Min( lookaheadMeters, maxLookaheadMeters );

			Vector3 predictedOrigin = focus.Origin + new Vector3(
				direction.X * lookaheadMeters,
				0.0f,
				direction.Y * lookaheadMeters
			);

			return new RegionPrediction(
				currentRegion,
				_grid.WorldToRegion( predictedOrigin ),
				direction,
				speed,
				lookaheadMeters,
				hasVelocity
			);
		}

		private float ComputePredictionSeconds()
		{
			float t = 1.0f + _settings.MaxExpectedMoveSpeed * 0.05f;
			return Math.Clamp( t, _settings.MinPredictionSeconds, _settings.MaxPredictionSeconds );
		}
	};
};
