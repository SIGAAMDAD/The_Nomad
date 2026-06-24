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

using System.Numerics;

namespace Nomad.Game.Application.Gameplay.Player.Movement.Parkour
{
	internal readonly struct PlayerTraversalInput
	{
		public static readonly PlayerTraversalInput Empty = new PlayerTraversalInput(
			Vector2.Zero,
			wantsAttach: false,
			wantsJump: false,
			wantsDrop: false
		);

		public readonly Vector2 Move;
		public readonly bool WantsAttach;
		public readonly bool WantsJump;
		public readonly bool WantsDrop;

		public PlayerTraversalInput( Vector2 move, bool wantsAttach, bool wantsJump, bool wantsDrop )
		{
			Move = Clamp( move );
			WantsAttach = wantsAttach;
			WantsJump = wantsJump;
			WantsDrop = wantsDrop;
		}

		private static Vector2 Clamp( Vector2 value )
		{
			float lenSq = value.LengthSquared();
			return lenSq <= 1.0f || lenSq <= 0.0001f
				? value
				: Vector2.Normalize( value );
		}
	}
}
