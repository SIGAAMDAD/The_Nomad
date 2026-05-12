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
using Nomad.Core.OnlineServices;

namespace Nomad.Game.Domain.Data.Player
{
	/*
	===================================================================================

	PlayerInputFrame

	===================================================================================
	*/
	/// <summary>
	/// A compact, serializable snapshot of one player's gameplay input for a single
	/// network/game tick.
	/// </summary>

	public readonly struct PlayerInputFrame
	{
		public static readonly PlayerInputFrame Empty = new PlayerInputFrame(
			PeerId.Invalid,
			0,
			0,
			Vector2.Zero,
			PlayerInputButtons.None,
			PlayerInputButtons.None
		);

		public PeerId PeerId { get; }
		public uint Tick { get; }
		public ushort Sequence { get; }
		public Vector2 Move { get; }
		public PlayerInputButtons ButtonsDown { get; }
		public PlayerInputButtons ButtonsPressed { get; }

		public bool IsMoving => Move.LengthSquared() > 0.0001f;
		public bool SlidePressed => IsPressed( PlayerInputButtons.Slide );
		public bool SlideDown => IsDown( PlayerInputButtons.Slide );

		public PlayerInputFrame(
			PeerId peerId,
			uint tick,
			ushort sequence,
			Vector2 move,
			PlayerInputButtons buttonsDown,
			PlayerInputButtons buttonsPressed
		)
		{
			PeerId = peerId;
			Tick = tick;
			Sequence = sequence;
			Move = move;
			ButtonsDown = buttonsDown;
			ButtonsPressed = buttonsPressed;
		}

		public bool IsDown( PlayerInputButtons button )
		{
			return (ButtonsDown & button) != 0;
		}

		public bool IsPressed( PlayerInputButtons button )
		{
			return (ButtonsPressed & button) != 0;
		}

		public PlayerInputFrame WithPeer( PeerId peerId )
		{
			return new PlayerInputFrame(
				peerId,
				Tick,
				Sequence,
				Move,
				ButtonsDown,
				ButtonsPressed
			);
		}

		public PlayerInputFrame WithTick( uint tick )
		{
			return new PlayerInputFrame(
				PeerId,
				tick,
				Sequence,
				Move,
				ButtonsDown,
				ButtonsPressed
			);
		}

		public PlayerInputFrame WithoutTransientButtons( uint tick )
		{
			return new PlayerInputFrame(
				PeerId,
				tick,
				Sequence,
				Move,
				ButtonsDown,
				PlayerInputButtons.None
			);
		}
	}
}
