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

using Nomad.Game.Application.Gameplay.Enemy.Planner;
using Nomad.Game.Application.Gameplay.Enemy.Planner.Goals;

namespace Nomad.Game.Application.Gameplay.Enemy.Templates.Sortior.Blader
{
	public class EngageHonorablyGoal : AiGoal
	{
		public override string Name => "EngageHonorably";
		public override int BasePriority => 60;

		public override int GetScoreModifier( PlanningContext context )
		{
			var m = (SortorianBladerMemory)context.Memory;
			int score = 0;
			if ( m.EnemyVisible ) {
				score += 25;
			}
			if ( m.PathToTargetClear ) {
				score += 10;
			}
			if ( m.RecentlyHitByGunfire ) {
				score -= 10;
			}
			return score;
		}

		protected override GoalBuilder Build()
		{
			var builder = new GoalBuilder();
			builder.Wants( WorldKey.EnemyVisible, true );
			return builder;
		}
	};
};
