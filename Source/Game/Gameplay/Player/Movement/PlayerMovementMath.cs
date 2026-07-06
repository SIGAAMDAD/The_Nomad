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
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Nomad.Game.Gameplay.Player.Movement
{
	internal static class PlayerMovementMath
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 ToPlanar( Vector3 value )
		{
			return new Vector3( value.X, 0.0f, value.Z );
		}

		public static Vector3 MoveToward( Vector3 from, Vector3 to, float delta )
		{
			Vector3 vector = to - from;
			float lengthSquared = vector.LengthSquared();
			if ( lengthSquared <= delta * delta || lengthSquared < 1E-06f ) {
				return to;
			}

			float invLength = 1.0f / MathF.Sqrt( lengthSquared );
			return from + vector * invLength * delta;
		}

		public static Vector3 NormalizePlanarOrDefault( Vector3 value, Vector3 fallback )
		{
			value.Y = 0.0f;
			float lengthSquared = value.LengthSquared();
			if ( lengthSquared > PlayerMovementSettings.EPSILON ) {
				return value / MathF.Sqrt( lengthSquared );
			}

			fallback.Y = 0.0f;
			lengthSquared = fallback.LengthSquared();
			return lengthSquared > PlayerMovementSettings.EPSILON
				? fallback / MathF.Sqrt( lengthSquared )
				: new Vector3( 0.0f, 0.0f, -1.0f );
		}

		public static Vector3 GetPlanarRight( Vector3 forward )
		{
			return NormalizePlanarOrDefault( new Vector3( -forward.Z, 0.0f, forward.X ), Vector3.UnitX );
		}
	};
};
