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
using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourInputAdapter
	{
		private Vector2 _externalMoveInput;
		private bool _hasExternalMoveInput;
		private bool _requestedAttach;
		private bool _requestedJump;
		private bool _requestedDrop;

		public PlayerTraversalInput Current { get; private set; } = PlayerTraversalInput.Empty;

		public void Apply( in PlayerInputFrame input )
		{
			Vector2 move = _hasExternalMoveInput ? _externalMoveInput : input.Move;

			Current = new PlayerTraversalInput(
				move,
				input.InteractPressed || _requestedAttach,
				input.JumpPressed || input.DashPressed || _requestedJump,
				input.DropPressed || _requestedDrop
			);
		}

		public void RequestAttach()
		{
			_requestedAttach = true;
		}

		public void RequestJump()
		{
			_requestedJump = true;
		}

		public void RequestDrop()
		{
			_requestedDrop = true;
		}

		public void SetTraversalInput( Vector2 moveInput )
		{
			_externalMoveInput = moveInput;
			_hasExternalMoveInput = true;
		}

		public void ClearFrameRequests()
		{
			_requestedAttach = false;
			_requestedJump = false;
			_requestedDrop = false;
			_hasExternalMoveInput = false;
		}
	};
};
