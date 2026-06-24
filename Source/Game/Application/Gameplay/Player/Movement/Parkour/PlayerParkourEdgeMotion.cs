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
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Application.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourEdgeMotion
	{
		private readonly PlayerParkourSettings _settings;

		public PlayerParkourEdgeMotion( PlayerParkourSettings settings )
		{
			_settings = settings;
		}

		public Vector3 GetTargetPosition( in TraversalAnchor from, in TraversalAnchor to, in TraversalEdge edge )
		{
			return edge.MoveType switch {
				TraversalMoveType.Mantle => PlayerParkourMath.AnchorPosition( from ) + PlayerParkourMath.AnchorUp( from ) * _settings.MantleUpOffset - PlayerParkourMath.AnchorNormal( from ) * _settings.MantleForwardOffset,
				TraversalMoveType.Vault => PlayerParkourMath.AnchorPosition( from ) + PlayerParkourMath.AnchorUp( from ) * _settings.VaultUpOffset - PlayerParkourMath.AnchorNormal( from ) * _settings.VaultForwardOffset,
				TraversalMoveType.Drop => PlayerParkourMath.AnchorPosition( from ) - PlayerParkourMath.AnchorUp( from ) * 1.0f + PlayerParkourMath.AnchorNormal( from ) * 0.25f,
				_ => PlayerParkourMath.AnchorPosition( to ),
			};
		}

		public Vector3 GetDirection( in TraversalAnchor from, in TraversalAnchor to, TraversalMoveType moveType )
		{
			Vector3 raw = PlayerParkourMath.AnchorPosition( to ) - PlayerParkourMath.AnchorPosition( from );

			if ( raw.LengthSquared() > 0.0001f ) {
				return raw.Normalized();
			}

			Vector3 right = PlayerParkourMath.AnchorRight( from );

			return moveType switch {
				TraversalMoveType.ClimbUp => PlayerParkourMath.AnchorUp( from ),
				TraversalMoveType.ClimbDown => -PlayerParkourMath.AnchorUp( from ),
				TraversalMoveType.ShimmyLeft => -right,
				TraversalMoveType.ShimmyRight => right,
				TraversalMoveType.CornerLeft => -right,
				TraversalMoveType.CornerRight => right,
				TraversalMoveType.Mantle => PlayerParkourMath.SafeNormalized( PlayerParkourMath.AnchorUp( from ) - PlayerParkourMath.AnchorNormal( from ), PlayerParkourMath.AnchorUp( from ) ),
				TraversalMoveType.Vault => -PlayerParkourMath.AnchorNormal( from ),
				TraversalMoveType.Drop => -PlayerParkourMath.AnchorUp( from ),
				TraversalMoveType.JumpAcross => -PlayerParkourMath.AnchorNormal( from ),
				_ => Vector3.Zero,
			};
		}

		public float GetMoveTypeBias( TraversalMoveType moveType, Vector2 input )
		{
			return moveType switch {
				TraversalMoveType.ClimbUp when input.Y > 0.5f => 0.25f,
				TraversalMoveType.ClimbDown when input.Y < -0.5f => 0.25f,
				TraversalMoveType.ShimmyLeft when input.X < -0.5f => 0.20f,
				TraversalMoveType.ShimmyRight when input.X > 0.5f => 0.20f,
				TraversalMoveType.Mantle when input.Y > _settings.MantleInputThreshold => 0.35f,
				TraversalMoveType.Drop when input.Y < -_settings.MantleInputThreshold => 0.25f,
				_ => 0.0f,
			};
		}
	}
}
