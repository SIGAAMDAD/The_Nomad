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
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Multiplayer.PlayerInput
{
	/*
	===================================================================================

	PlayerInputCommandRpc

	===================================================================================
	*/
	/// <summary>
	/// Client-to-host player input command.
	///
	/// Long-term, the RPC bus should provide the authenticated sender PeerId to
	/// handlers. Until then, the host should validate PeerId against the session.
	/// </summary>

	public readonly struct PlayerInputCommandRpc
	{
		public PlayerId PlayerId { get; }
		public uint Tick { get; }
		public ushort Sequence { get; }
		public float MoveX { get; }
		public float MoveY { get; }
		public float AimDirectionX { get; }
		public float AimDirectionY { get; }
		public float AimAngle { get; }
		public PlayerInputButtons ButtonsDown { get; }
		public PlayerInputButtons ButtonsPressed { get; }

		public PlayerInputCommandRpc(
			PlayerId playerId,
			uint tick,
			ushort sequence,
			float moveX,
			float moveY,
			float aimDirectionX,
			float aimDirectionY,
			float aimAngle,
			PlayerInputButtons buttonsDown,
			PlayerInputButtons buttonsPressed
		)
		{
			PlayerId = playerId;
			Tick = tick;
			Sequence = sequence;
			MoveX = moveX;
			MoveY = moveY;
			AimDirectionX = aimDirectionX;
			AimDirectionY = aimDirectionY;
			AimAngle = aimAngle;
			ButtonsDown = buttonsDown;
			ButtonsPressed = buttonsPressed;
		}

		public PlayerInputCommandRpc( in PlayerInputFrame frame )
			: this(
				frame.PlayerId,
				frame.Tick,
				frame.Sequence,
				frame.Move.X,
				frame.Move.Y,
				frame.AimDirection.X,
				frame.AimDirection.Y,
				frame.AimAngle,
				frame.ButtonsDown,
				frame.ButtonsPressed
			)
		{
		}

		public PlayerInputFrame ToFrame()
		{
			return new PlayerInputFrame(
				PlayerId,
				Tick,
				Sequence,
				new Vector2( MoveX, MoveY ),
				new Vector2( AimDirectionX, AimDirectionY ),
				AimAngle,
				ButtonsDown,
				ButtonsPressed
			);
		}
	}
}
