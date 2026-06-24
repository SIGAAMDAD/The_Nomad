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

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal sealed class PlayerMovementBodyMotor
	{
		private readonly PlayerPrefab _prefab;
		private readonly PlayerMovementRuntime _runtime;

		public PlayerMovementBodyMotor( PlayerPrefab prefab, PlayerMovementRuntime runtime )
		{
			_prefab = prefab;
			_runtime = runtime;
		}

		public void ConfigureBody()
		{
			_prefab.UpDirection = Vector3.Up;
			_prefab.FloorSnapLength = PlayerMovementSettings.FLOOR_SNAP_LENGTH;
			_prefab.FloorMaxAngle = Mathf.DegToRad( PlayerMovementSettings.FLOOR_MAX_ANGLE_DEGREES );
			_prefab.SlideOnCeiling = true;
		}

		public void ApplyGroundVelocity( float delta, Vector3 horizontalVelocity )
		{
			Vector3 velocity = _prefab.Velocity;
			velocity.X = horizontalVelocity.X;
			velocity.Z = horizontalVelocity.Z;

			if ( _prefab.IsOnFloor() ) {
				if ( velocity.Y < 0.0f ) {
					velocity.Y = PlayerMovementSettings.GROUND_STICK_VELOCITY;
				}
			} else {
				velocity.Y -= PlayerMovementSettings.GRAVITY * delta;
			}

			_prefab.Velocity = velocity;
			_prefab.MoveAndSlide();
			_runtime.HorizontalVelocity = new Vector3( _prefab.Velocity.X, 0.0f, _prefab.Velocity.Z );
		}

		public void ApplyLockedVelocity()
		{
			_runtime.HorizontalVelocity = Vector3.Zero;

			Vector3 velocity = _prefab.Velocity;
			velocity.X = 0.0f;
			velocity.Z = 0.0f;
			velocity.Y = _prefab.IsOnFloor() ? PlayerMovementSettings.GROUND_STICK_VELOCITY : velocity.Y;
			_prefab.Velocity = velocity;
			_prefab.MoveAndSlide();
		}
	}
}
