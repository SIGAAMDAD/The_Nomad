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

namespace Nomad.Game.Gameplay.Player.Movement
{
	[Flags]
	internal enum PlayerMovementFrameDirtyFlags : uint
	{
		None = 0,
		Input = 1u << 0,
		MovementMode = 1u << 1,
		WishDirection = 1u << 2,
		FacingDirection = 1u << 3,
		HorizontalVelocity = 1u << 4,
		BodyVelocity = 1u << 5,
		RequestedCommit = 1u << 6,
		WallRun = 1u << 7,
		Locomotion = 1u << 8,
		Flags = 1u << 9,
		BodyCommitted = 1u << 10
	}
}
