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

namespace Nomad.Game.Application.Gameplay.Enemy.Planner.Actions
{
	public abstract class AiAction
	{
		public abstract string Name { get; }

		protected abstract void Build( ActionBuilder builder );

		public virtual bool ValidateContext( PlanningContext context ) => true;
		public virtual int GetDynamicCost( PlanningContext context ) => 0;

		public abstract ActionRunStatus Start( NpcAgent agent );
		public virtual ActionRunStatus Tick( NpcAgent agent, float dt ) => ActionRunStatus.Succeeded;
		public virtual void Cancel( NpcAgent agent ) { }

		public PlannerAction Compile()
		{
			ActionBuilder builder = new ActionBuilder();
			Build( builder );

			return new PlannerAction(
				Name,
				builder.BaseCost,
				builder.GetPreconditions(),
				builder.GetEffects(),
				new ActionRunnerAdapter( this ),
				ValidateContext,
				GetDynamicCost );
		}
	}
};
