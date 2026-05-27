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

namespace Nomad.Game.Sdk.Npc.Planner
{
	[Flags]
	public enum ReplanReason : uint
	{
		None = 0,
		NoPlan = 1 << 0,
		ActionFailed = 1 << 1,
		GoalChanged = 1 << 2,
		ContextInvalid = 1 << 3,
		TargetLost = 1 << 4,
		TookHeavyDamage = 1 << 5,
		PeriodicRefresh = 1 << 6
	}
}
