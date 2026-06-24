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
using System.Runtime.CompilerServices;
using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal readonly struct PlayerMovementInputState
	{
		public readonly PlayerInputFrame Frame;
		public readonly Vector2 Move;
		public readonly bool HasMoveInput;

		private PlayerMovementInputState( PlayerInputFrame frame, Vector2 move, bool hasMoveInput )
		{
			Frame = frame;
			Move = move;
			HasMoveInput = hasMoveInput;
		}

		public static PlayerMovementInputState FromFrame( in PlayerInputFrame frame )
		{
			Vector2 move = ClampInput( frame.Move );
			return new PlayerMovementInputState(
				frame,
				move,
				move.LengthSquared() > PlayerMovementSettings.MOVING_THRESHOLD
			);
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static Vector2 ClampInput( Vector2 input )
		{
			float lengthSquared = input.LengthSquared();
			return lengthSquared <= 1.0f
				? input
				: Vector2.Normalize( input );
		}
	}
}
