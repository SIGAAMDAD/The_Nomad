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
using Nomad.Game.Application.Gameplay.Player.Stats.DerivedStatEvaluators;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Application.Gameplay.Player.Stats.DerivedStatEvaluators
{
	internal sealed class SanityDerivedStatEvaluator : IPlayerDerivedStatEvaluator
	{
		public bool CanEvaluate( DerivedStatType type )
		{
			return type == DerivedStatType.EffectiveSanityMax
				|| type == DerivedStatType.SanityDrainMultiplier;
		}

		public float Evaluate( DerivedStatType type, in PlayerDerivedStatEvaluationContext context )
		{
			return type switch {
				DerivedStatType.EffectiveSanityMax => Math.Max( 0.0f, context.GetBaseStat( BaseStatType.BaseSanity ) ),
				DerivedStatType.SanityDrainMultiplier => 1.0f,
				_ => throw new ArgumentOutOfRangeException( nameof( type ) )
			};
		}
	};
};
