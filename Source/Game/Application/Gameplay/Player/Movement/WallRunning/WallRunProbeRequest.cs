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

namespace Nomad.Game.Application.Gameplay.Player.Movement.WallRunning
{
	internal readonly struct WallRunProbeRequest
	{
		public readonly Vector3 PlanarWishDirection;
		public readonly Vector3 HorizontalVelocity;
		public readonly Vector3 FallbackForward;

		public WallRunProbeRequest(
			Vector3 planarWishDirection,
			Vector3 horizontalVelocity,
			Vector3 fallbackForward
		)
		{
			PlanarWishDirection = planarWishDirection;
			HorizontalVelocity = horizontalVelocity;
			FallbackForward = fallbackForward;
		}
	}
}
