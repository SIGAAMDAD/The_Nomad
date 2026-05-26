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

namespace Nomad.Game.Application.Gameplay.Enemy.Planner.Goals
{
	public abstract class AiGoal
	{
		public abstract string Name { get; }
		public abstract int BasePriority { get; }

		protected abstract GoalBuilder Build();

		public virtual int GetScoreModifier( PlanningContext context ) => 0;

		public GoalDef Compile()
		{
			GoalBuilder builder = Build();
			return new GoalDef( Name, BasePriority, builder.GetDesiredState(), GetScoreModifier );
		}
	}
};
