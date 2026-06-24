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
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;

namespace Nomad.Game.Application.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunModeResolver

	===================================================================================
	*/
	/// <summary>
	/// Converts input + surface state into the intended wall movement mode and tangent.
	/// </summary>

	internal sealed class WallRunModeResolver
	{
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
			Vector3 wish = WallRunMath.SafePlanarNormalized( planarWishDirection, Vector3.Zero );
			float intoWall = wish.LengthSquared() > WallRunMath.Epsilon ? wish.Dot( -candidate.Normal ) : 0.0f;
			Vector3 tangent = WallRunMath.ProjectOnPlane( wish, candidate.Normal );
			tangent.Y = 0.0f;
			float tangentStrengthSquared = tangent.LengthSquared();

			if ( (input.DropDown || input.Move.Y <= _settings.DownIntentThreshold) && candidate.AllowsRunDown ) {
				return WallMountedMoveMode.WallRunDown;
			}

			if ( input.Move.Y >= _settings.UpIntentThreshold && intoWall >= _settings.MinimumIntoWallForVerticalRun && candidate.AllowsRunUp ) {
				return WallMountedMoveMode.WallRunUp;
			}

			if ( tangentStrengthSquared > 0.0225f && candidate.AllowsHorizontalRun ) {
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

			if ( tangent.LengthSquared() > WallRunMath.Epsilon ) {
				return tangent.Normalized();
			}

			if ( currentRunDirection.LengthSquared() > WallRunMath.Epsilon ) {
				return currentRunDirection;
			}

			Vector3 current = WallRunMath.ProjectOnPlane( horizontalVelocity, candidate.Normal );
			current.Y = 0.0f;

			if ( current.LengthSquared() > WallRunMath.Epsilon ) {
				return current.Normalized();
			}

			Vector3 right = Vector3.Up.Cross( candidate.Normal );
			return WallRunMath.SafePlanarNormalized( right, Vector3.Right );
		}
	}
}
