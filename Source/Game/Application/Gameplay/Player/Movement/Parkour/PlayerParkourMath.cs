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
using Godot;
using Nomad.EngineUtils;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Application.Gameplay.Player.Movement.Parkour
{
	internal static class PlayerParkourMath
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 AnchorPosition( in TraversalAnchor anchor )
		{
			return anchor.Position.ToGodot();
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 AnchorNormal( in TraversalAnchor anchor )
		{
			return anchor.Normal.ToGodot();
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 AnchorUp( in TraversalAnchor anchor )
		{
			return anchor.Up.ToGodot();
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 AnchorRight( in TraversalAnchor anchor )
		{
			Vector3 right = AnchorUp( anchor ).Cross( AnchorNormal( anchor ) );
			return SafeNormalized( right, Vector3.Right );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 SafeNormalized( Vector3 value, Vector3 fallback )
		{
			float lenSq = value.LengthSquared();
			return lenSq < 0.0001f ? fallback : value / Mathf.Sqrt( lenSq );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float SmoothStep( float t )
		{
			t = Mathf.Clamp( t, 0.0f, 1.0f );
			return t * t * (3.0f - 2.0f * t);
		}
	}
}
