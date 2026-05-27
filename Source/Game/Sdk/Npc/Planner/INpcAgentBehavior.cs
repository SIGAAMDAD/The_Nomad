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

using Nomad.Game.Sdk.Npc.Planner.Goals;

namespace Nomad.Game.Sdk.Npc.Planner
{
    public interface INpcAgentBehavior
    {
        WorkingMemory Memory { get; }
        AgentArchetype Archetype { get; }
        int PlanningBuget { get; }

        void OnPlanBuilt(INpcAgent agent, Plan plan, ReplanReason reasons);
        void OnPlanFailed(INpcAgent agent, GoalDef goal, ReplanReason reasons);
        void OnPlanCompleted(INpcAgent agent, GoalDef goal);
        void OnActionStarted(INpcAgent agent, PlannerAction action);
        void OnActionSucceeded(INpcAgent agent, PlannerAction action);
        void OnActionFailed(INpcAgent agent, PlannerAction action);
    }
}
