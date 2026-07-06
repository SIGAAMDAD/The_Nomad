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
using System.Numerics;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;

namespace Nomad.Game.Gameplay.Player.Movement.WallRunning
{
	/*
	===================================================================================

	WallRunTraversalBridge

	===================================================================================
	*/
	/// <summary>
	/// Boundary adapter from dynamic wall-running into the authored traversal subsystem.
	/// </summary>

	internal sealed class WallRunTraversalBridge
	{
		private readonly WallRunSettings _settings;
		private readonly IPlayerParkourController _parkour;

		public WallRunTraversalBridge( WallRunSettings settings, IPlayerParkourController parkour )
		{
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
			_parkour = parkour ?? throw new ArgumentNullException( nameof( parkour ) );
		}

		public bool ShouldRequestTransfer( in PlayerInputFrame input, WallMountedMoveMode mode, float elapsed )
		{
			if ( elapsed < _settings.MinimumParkourTransferDuration ) {
				return false;
			}

			return input.InteractPressed || (mode == WallMountedMoveMode.WallRunUp && input.Move.Y > 0.55f && input.JumpPressed);
		}

		public bool TryRequestTransfer(
			in PlayerInputFrame input,
			Vector3 currentVelocity,
			out Vector3 transferVelocity
		)
		{
			transferVelocity = currentVelocity;
			if ( _parkour == null ) {
				return false;
			}

			_parkour.SetTraversalInput( input.Move );
			_parkour.RequestAttach();

			transferVelocity = currentVelocity * _settings.TraversalTransferVelocityScale;
			transferVelocity.Y = MathF.Max( transferVelocity.Y, _settings.TraversalTransferUpBias );
			return true;
		}
	};
};
