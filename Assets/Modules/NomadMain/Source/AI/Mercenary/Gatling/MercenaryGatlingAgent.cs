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

using Nomad.Game.Sdk.Npc;
using Nomad.Game.Sdk.Npc.Planner;
using Nomad.Game.Sdk.Npc.Planner.Goals;

namespace Nomad.Modules.NomadMain.AI.Mercenary.Gatling
{
    /*
	===================================================================================

	MercenaryGatlingAgent

	===================================================================================
	*/
    /// <summary>
    ///
    /// </summary>

    public class MercenaryGatlingAgent : INpcAgentBehavior
    {
        public WorkingMemory Memory => _memory;
        private readonly MercenaryGatlingMemory _memory;

        public AgentArchetype Archetype => _archetype;
        private readonly AgentArchetype _archetype;

        public int PlanningBudget => 96;

        public MercenaryGatlingAgent()
        {
            _archetype = new AgentArchetype {
            };
        }

        public void OnActionFailed( INpcAgent agent, PlannerAction action )
        {
        }

        public void OnActionStarted( INpcAgent agent, PlannerAction action )
        {
        }

        public void OnActionSucceeded( INpcAgent agent, PlannerAction action )
        {
        }

        public void OnPlanBuilt( INpcAgent agent, Plan plan, ReplanReason reasons )
        {
        }

        public void OnPlanCompleted( INpcAgent agent, GoalDef goal )
        {
        }

        public void OnPlanFailed( INpcAgent agent, GoalDef goal, ReplanReason reasons )
        {
        }
    }
}
