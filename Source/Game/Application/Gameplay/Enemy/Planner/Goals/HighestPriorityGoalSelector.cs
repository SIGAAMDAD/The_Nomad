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

namespace Nomad.Game.Application.Gameplay.Enemy.Planner.Goals
{
	public sealed class HighestPriorityGoalSelector : IGoalSelector
	{
		public GoalDef SelectBestGoal( NpcAgent agent, GoalDef[] goals )
		{
			if ( goals == null || goals.Length == 0 ) {
				throw new InvalidOperationException( "No goals available." );
			}

			PlanningContext context = new PlanningContext( agent.Memory );
			GoalDef best = goals[0];
			int bestScore = best.GetPriority( context );

			for ( int i = 1; i < goals.Length; i++ ) {
				int score = goals[i].GetPriority( context );
				if ( score > bestScore ) {
					best = goals[i];
					bestScore = score;
				}
			}

			return best;
		}
	}
};
