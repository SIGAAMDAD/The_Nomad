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
using Godot;

namespace Nomad.Game.Application.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunMath

	===================================================================================
	*/

	internal static class WallRunMath
	{
		public const float Epsilon = 0.0001f;
		public const float TinyLength = 1E-06f;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 ProjectOnPlane( Vector3 value, Vector3 normal )
		{
			return value - normal * value.Dot( normal );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 RemovePositiveNormalComponent( Vector3 value, Vector3 normal )
		{
			float normalSpeed = value.Dot( normal );
			return normalSpeed > 0.0f ? value - normal * normalSpeed : value;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 SafeNormalized( Vector3 value, Vector3 fallback )
		{
			float lengthSquared = value.LengthSquared();
			if ( lengthSquared <= Epsilon ) {
				return fallback;
			}

			return value / MathF.Sqrt( lengthSquared );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 SafePlanarNormalized( Vector3 value, Vector3 fallback )
		{
			value.Y = 0.0f;
			fallback.Y = 0.0f;
			return SafeNormalized( value, SafeNormalized( fallback, Vector3.Forward ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 MoveToward( Vector3 from, Vector3 to, float delta )
		{
			Vector3 vector = to - from;
			float lengthSquared = vector.LengthSquared();
			if ( lengthSquared <= TinyLength ) {
				return to;
			}

			float length = MathF.Sqrt( lengthSquared );
			if ( length <= delta ) {
				return to;
			}

			return from + vector * (delta / length);
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 PlanarRight( Vector3 forward )
		{
			Vector3 right = new Vector3( -forward.Z, 0.0f, forward.X );
			return SafePlanarNormalized( right, Vector3.Right );
		}
	}
}
