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

namespace Nomad.Game.Sdk.Player.JumpKit
{
	[Flags]
	public enum PlayerJumpKitFlags : byte
	{
		IsBurnedOut = 1 << 0,
		IsDashing = 1 << 1,
		DashEnded = 1 << 2,
		BurnedOutThisFrame = 1 << 3,
		RechargedThisFrame = 1 << 4
	}
}
