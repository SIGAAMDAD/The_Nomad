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

namespace Nomad.Game.Sdk.Npc.Planner.Actions
{
    public abstract class AiAction
    {
        public abstract string Name { get; }

        protected abstract void Build(ActionBuilder builder);

        public virtual bool ValidateContext(PlanningContext context)
        {
            return true;
        }

        public virtual int GetDynamicCost(PlanningContext context)
        {
            return 0;
        }

        public abstract ActionRunStatus Start(INpcAgent agent);

        public virtual ActionRunStatus Tick(INpcAgent agent, float dt)
        {
            return ActionRunStatus.Succeeded;
        }

        public virtual void Cancel(INpcAgent agent)
        {
        }

        public PlannerAction Compile()
        {
            ActionBuilder builder = new ActionBuilder();
            Build(builder);

            return new PlannerAction(
                Name,
                builder.BaseCost,
                builder.GetPreconditions(),
                builder.GetEffects(),
                new ActionRunnerAdapter(this),
                ValidateContext,
                GetDynamicCost
            );
        }
    }
}
