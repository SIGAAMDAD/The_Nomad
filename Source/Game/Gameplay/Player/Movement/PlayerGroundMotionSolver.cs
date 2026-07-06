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

using Nomad.EngineUtils;
using System.Numerics;
namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerGroundMotionSolver

	===================================================================================
	*/
	/// <summary>
	/// Solves ordinary planar movement from the already-cached frame state. It does not
	/// query the flag service; dash/slide status is captured once by PlayerMovementFrame.
	/// </summary>

	internal sealed class PlayerGroundMotionSolver
	{
		private readonly PlayerMovementRuntime _runtime;
		private readonly PlayerMovementSpeeds _speeds;
		private readonly float _movementAcceleration;
		private readonly float _movementFriction;

		public PlayerGroundMotionSolver(
			PlayerMovementRuntime runtime,
			PlayerMovementSpeeds speeds
		)
		{
			_runtime = runtime;
			_speeds = speeds;
			_movementAcceleration = PlayerMovementSettings.GameplayUnitsToWorldUnits( Sdk.Player.Constants.MOVEMENT_ACCELERATION );
			_movementFriction = PlayerMovementSettings.GameplayUnitsToWorldUnits( Sdk.Player.Constants.MOVEMENT_FRICTION );
		}

		public Vector3 Solve( ref PlayerMovementFrame frame )
		{
			Vector3 targetVelocity;
			float acceleration = _movementAcceleration;

			if ( frame.IsDashing ) {
				targetVelocity = _runtime.Actions.DashDirection * _speeds.GetDashSpeed();
				acceleration *= 3.0f;
				frame.SetMode( PlayerMovementMode.Dash );
			} else if ( frame.IsSliding ) {
				targetVelocity = _runtime.Actions.SlideDirection * _speeds.GetSlideSpeed();
				acceleration *= PlayerMovementSettings.SLIDE_FRICTION_MULTIPLIER;
				frame.SetMode( PlayerMovementMode.Slide );
			} else if ( frame.WishDirectionLengthSquared > PlayerMovementSettings.EPSILON ) {
				targetVelocity = frame.WishDirection * _speeds.GetMovementSpeed();
				frame.SetMode( frame.IsGrounded ? PlayerMovementMode.Ground : PlayerMovementMode.Falling );
			} else {
				targetVelocity = Vector3.Zero;
				acceleration = _movementFriction;
				frame.SetMode( frame.IsGrounded ? PlayerMovementMode.Ground : PlayerMovementMode.Falling );
			}

			if ( !frame.IsGrounded ) {
				acceleration *= PlayerMovementSettings.AIR_CONTROL_MULTIPLIER;
			}

			return PlayerMovementMath.MoveToward(
				frame.HorizontalVelocity,
				targetVelocity,
				acceleration * frame.DeltaTime
			);
		}
	};
};
