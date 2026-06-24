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

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal static class PlayerMovementMath
	{
		public static Vector3 MoveToward( Vector3 from, Vector3 to, float delta )
		{
			Vector3 vector = to - from;
			float length = vector.Length();
			if ( length <= delta || length < 1E-06f ) {
				return to;
			}

			return from + vector / length * delta;
		}

		public static Vector3 NormalizePlanarOrDefault( Vector3 value, Vector3 fallback )
		{
			value.Y = 0.0f;
			if ( value.LengthSquared() > PlayerMovementSettings.EPSILON ) {
				return value.Normalized();
			}

			fallback.Y = 0.0f;
			return fallback.LengthSquared() > PlayerMovementSettings.EPSILON
				? fallback.Normalized()
				: new Vector3( 0.0f, 0.0f, -1.0f );
		}

		public static Vector3 GetPlanarRight( Vector3 forward )
		{
			return new Vector3( -forward.Z, 0.0f, forward.X ).Normalized();
		}
	}
}
