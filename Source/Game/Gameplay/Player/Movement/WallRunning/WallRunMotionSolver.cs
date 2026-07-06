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
using Nomad.Game.Sdk.Player.Movement;

namespace Nomad.Game.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunMotionSolver

	===================================================================================
	*/
	/// <summary>
	/// Pure wall-run velocity solver. It does not query physics, read input, publish flags,
	/// or communicate with traversal.
	/// </summary>

	internal sealed class WallRunMotionSolver
	{
		private readonly WallRunSettings _settings;
		private readonly WallRunModeResolver _modeResolver;

		public WallRunMotionSolver( WallRunSettings settings, WallRunModeResolver modeResolver )
		{
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
			_modeResolver = modeResolver ?? throw new ArgumentNullException( nameof( modeResolver ) );
		}

		public WallRunMotionResult Solve(
			float delta,
			WallMountedMoveMode mode,
			in WallSurfaceCandidate surface,
			Vector3 currentVelocity,
			Vector3 currentRunDirection,
			Vector3 planarWishDirection,
			Vector3 horizontalVelocity,
			float movementSpeed
		)
		{
			Vector3 runDirection = _modeResolver.ResolveRunDirection(
				mode,
				planarWishDirection,
				surface,
				currentRunDirection,
				horizontalVelocity
			);

			Vector3 target = CalculateTargetVelocity( mode, runDirection, movementSpeed );
			Vector3 velocity = WallRunMath.RemovePositiveNormalComponent( currentVelocity, surface.Normal );
			velocity = WallRunMath.MoveToward( velocity, target, _settings.TangentialAcceleration * delta );

			if ( mode == WallMountedMoveMode.HorizontalWallRun ) {
				velocity.Y -= _settings.Gravity * _settings.HorizontalGravityScale * delta;
				velocity.Y = MathF.Max( velocity.Y, -1.25f );
			} else if ( mode == WallMountedMoveMode.WallRunUp ) {
				velocity.Y = Godot.Mathf.MoveToward( velocity.Y, _settings.UpRunSpeed, _settings.VerticalAcceleration * delta );
				velocity.Y -= _settings.Gravity * _settings.VerticalGravityScale * delta;
			} else if ( mode == WallMountedMoveMode.WallRunDown ) {
				velocity.Y = Godot.Mathf.MoveToward( velocity.Y, -_settings.DownRunSpeed, _settings.VerticalAcceleration * delta );
				velocity.Y -= _settings.Gravity * _settings.DownGravityScale * delta;
			}

			velocity += -surface.Normal * _settings.WallStickSpeed;

			Vector3 facingDirection = mode == WallMountedMoveMode.HorizontalWallRun
				? WallRunMath.SafePlanarNormalized( runDirection, -surface.Normal )
				: WallRunMath.SafePlanarNormalized( -surface.Normal, runDirection );

			return new WallRunMotionResult( velocity, runDirection, facingDirection );
		}

		private Vector3 CalculateTargetVelocity( WallMountedMoveMode mode, Vector3 runDirection, float movementSpeed )
		{
			float horizontalSpeed = MathF.Max( movementSpeed * _settings.HorizontalSpeedMultiplier, _settings.MinimumHorizontalSpeed );
			if ( mode == WallMountedMoveMode.HorizontalWallRun ) {
				return runDirection * horizontalSpeed;
			}

			if ( mode == WallMountedMoveMode.WallRunUp ) {
				return runDirection * (horizontalSpeed * 0.28f) + Vector3.UnitY * _settings.UpRunSpeed;
			}

			if ( mode == WallMountedMoveMode.WallRunDown ) {
				return runDirection * (horizontalSpeed * 0.22f) + -Vector3.UnitY * _settings.DownRunSpeed;
			}

			return Vector3.Zero;
		}
	};
};
