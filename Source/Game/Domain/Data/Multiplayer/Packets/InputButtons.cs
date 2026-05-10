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

namespace Nomad.Game.Domain.Data.Multiplayer.Packets
{
	public enum InputButtons : uint
	{
		None = 0,

		Fire = 1 << 0,
		AltFire = 1 << 1,
		Melee = 1 << 2,
		Parry = 1 << 3,
		Dash = 1 << 4,
		Slide = 1 << 5,
		Interact = 1 << 6,
		Reload = 1 << 7,
		SwapWeapon = 1 << 8,
		UseMetalArm = 1 << 9,
		BulletTime = 1 << 10,
		Crouch = 1 << 11
	};
};
