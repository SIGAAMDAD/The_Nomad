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

using System.Runtime.CompilerServices;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal static class PlayerMovementSettings
	{
		public const float EPSILON = 0.0001f;
		public const float MOVING_THRESHOLD = 0.001f;
		public const float GAMEPLAY_UNITS_PER_WORLD_UNIT = 32.0f;
		public const float HARD_START_THRESHOLD = 64.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		public const float HARD_STOP_THRESHOLD = 64.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;

		public const float GRAVITY = 1800.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		public const float GROUND_STICK_VELOCITY = -32.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		public const float AIR_CONTROL_MULTIPLIER = 0.35f;
		public const float DASH_SPEED_FALLBACK_MULTIPLIER = 2.60f;
		public const float SLIDE_SPEED_MULTIPLIER = 1.65f;
		public const float SLIDE_FRICTION_MULTIPLIER = 0.45f;
		public const float FLOOR_SNAP_LENGTH = 12.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		public const float FLOOR_MAX_ANGLE_DEGREES = 46.0f;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float GameplayUnitsToWorldUnits( float value )
		{
			return value / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float WorldUnitsToGameplayUnits( float value )
		{
			return value * GAMEPLAY_UNITS_PER_WORLD_UNIT;
		}
	}
}
