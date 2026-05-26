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
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Application.Gameplay.Player.Stats.DerivedStatEvaluators
{
	internal readonly struct PlayerDerivedStatEvaluationContext
	{
		private readonly Func<DerivedStatType, float> _getDerivedValue;

		public IPlayerBaseStatsRepository BaseStats { get; }

		public PlayerDerivedStatEvaluationContext( IPlayerBaseStatsRepository baseStats, Func<DerivedStatType, float> getDerivedValue )
		{
			BaseStats = baseStats;
			_getDerivedValue = getDerivedValue;
		}

		public float GetBaseStat( BaseStatType type )
		{
			return BaseStats.GetBaseStatValue( type );
		}

		public float GetDerivedStat( DerivedStatType type )
		{
			return _getDerivedValue( type );
		}
	};
};
