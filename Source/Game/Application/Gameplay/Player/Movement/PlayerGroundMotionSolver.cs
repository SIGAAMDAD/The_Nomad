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
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Player.State;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal sealed class PlayerGroundMotionSolver
	{
		private readonly PlayerPrefab _prefab;
		private readonly PlayerMovementRuntime _runtime;
		private readonly IPlayerFlagService _flags;
		private readonly PlayerMovementSpeeds _speeds;

		public PlayerGroundMotionSolver(
			PlayerPrefab prefab,
			PlayerMovementRuntime runtime,
			IPlayerFlagService flags,
			PlayerMovementSpeeds speeds
		)
		{
			_prefab = prefab;
			_runtime = runtime;
			_flags = flags;
			_speeds = speeds;
		}

		public Vector3 Solve( float delta, Vector3 wishDirection )
		{
			Vector3 targetVelocity;
			float acceleration = PlayerMovementSettings.GameplayUnitsToWorldUnits( Sdk.Player.Constants.MOVEMENT_ACCELERATION );

			if ( _flags.GetFlags( PlayerFlags.Dashing ) ) {
				targetVelocity = _runtime.DashDirection * _speeds.GetDashSpeed();
				acceleration *= 3.0f;
			} else if ( _flags.GetFlags( PlayerFlags.Sliding ) ) {
				targetVelocity = _runtime.SlideDirection * _speeds.GetSlideSpeed();
				acceleration *= PlayerMovementSettings.SLIDE_FRICTION_MULTIPLIER;
			} else if ( wishDirection.LengthSquared() > PlayerMovementSettings.EPSILON ) {
				targetVelocity = wishDirection * _speeds.GetMovementSpeed();
			} else {
				targetVelocity = Vector3.Zero;
				acceleration = PlayerMovementSettings.GameplayUnitsToWorldUnits( Sdk.Player.Constants.MOVEMENT_FRICTION );
			}

			if ( !_prefab.IsOnFloor() ) {
				acceleration *= PlayerMovementSettings.AIR_CONTROL_MULTIPLIER;
			}

			return PlayerMovementMath.MoveToward(
				_runtime.HorizontalVelocity,
				targetVelocity,
				acceleration * delta
			);
		}
	}
}
