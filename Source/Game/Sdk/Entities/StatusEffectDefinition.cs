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

using Nomad.Core.Util;
using System;
using System.Collections.Generic;

namespace Nomad.Game.Sdk.Entities
{
	public sealed record StatusEffectDefinition
	{
		public InternString Id { get; init; }
		public InternString DisplayName { get; init; }
		public InternString? Description { get; init; }

		public StatusEffectFlags Flags { get; init; }

		public StatusEffectStackingPolicy StackPolicy { get; init; } = StatusEffectStackingPolicy.RefreshDuration;

		public byte MaxStacks { get; init; } = 1;
		public float DurationSeconds { get; init; }
		public float TickIntervalSeconds { get; init; }

		public float DamagePerTick { get; init; }
		public float HealPerTick { get; init; }

		public IReadOnlyList<StatusEffectModifier> Modifiers { get; init; }
			= Array.Empty<StatusEffectModifier>();
	}
}
