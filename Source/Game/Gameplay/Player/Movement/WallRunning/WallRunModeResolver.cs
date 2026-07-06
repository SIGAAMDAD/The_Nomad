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
using System.Numerics;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;

namespace Nomad.Game.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunModeResolver

	===================================================================================
	*/
	/// <summary>
	/// Converts input + surface state into the intended wall-mounted movement mode.
	/// </summary>

	internal sealed class WallRunModeResolver
	{
		private const float TANGENT_INTENT_THRESHOLD_SQUARED = 0.0225f;

		private readonly WallRunSettings _settings;

		public WallRunModeResolver( WallRunSettings settings )
		{
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
		}

		public WallMountedMoveMode ResolveMode(
			in PlayerInputFrame input,
			Vector3 planarWishDirection,
			in WallSurfaceCandidate candidate
		)
		{
			Vector3 wish = planarWishDirection;
			wish.Y = 0.0f;
			float wishLengthSquared = wish.LengthSquared();
			float intoWall = wishLengthSquared > WallRunMath.EPSILON ? Vector3.Dot( wish, -candidate.Normal ) : 0.0f;
			Vector3 tangent = WallRunMath.ProjectOnPlane( wish, candidate.Normal );
			tangent.Y = 0.0f;

			if ( (input.DropDown || input.Move.Y <= _settings.DownIntentThreshold) && candidate.AllowsRunDown ) {
				return WallMountedMoveMode.WallRunDown;
			}

			if ( input.Move.Y >= _settings.UpIntentThreshold && intoWall >= _settings.MinimumIntoWallForVerticalRun && candidate.AllowsRunUp ) {
				return WallMountedMoveMode.WallRunUp;
			}

			if ( tangent.LengthSquared() > TANGENT_INTENT_THRESHOLD_SQUARED && candidate.AllowsHorizontalRun ) {
				return WallMountedMoveMode.HorizontalWallRun;
			}

			if ( intoWall >= _settings.MinimumIntoWallForVerticalRun && candidate.AllowsRunUp ) {
				return WallMountedMoveMode.WallRunUp;
			}

			return candidate.AllowsHorizontalRun
				? WallMountedMoveMode.HorizontalWallRun
				: WallMountedMoveMode.None;
		}

		public Vector3 ResolveRunDirection(
			WallMountedMoveMode mode,
			Vector3 planarWishDirection,
			in WallSurfaceCandidate candidate,
			Vector3 currentRunDirection,
			Vector3 horizontalVelocity
		)
		{
			Vector3 tangent = WallRunMath.ProjectOnPlane( planarWishDirection, candidate.Normal );
			tangent.Y = 0.0f;
			float tangentLengthSquared = tangent.LengthSquared();

			if ( tangentLengthSquared > WallRunMath.EPSILON ) {
				return tangent / MathF.Sqrt( tangentLengthSquared );
			}

			if ( currentRunDirection.LengthSquared() > WallRunMath.EPSILON ) {
				return currentRunDirection;
			}

			Vector3 current = WallRunMath.ProjectOnPlane( horizontalVelocity, candidate.Normal );
			current.Y = 0.0f;
			float currentLengthSquared = current.LengthSquared();

			if ( currentLengthSquared > WallRunMath.EPSILON ) {
				return current / MathF.Sqrt( currentLengthSquared );
			}

			Vector3 right = Vector3.Cross( Vector3.UnitY, candidate.Normal );
			return WallRunMath.SafePlanarNormalized( right, Vector3.UnitX );
		}
	};
};
