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

namespace Nomad.Modules.NomadMain.AI.Sortior.Blader
{
    /*
	===================================================================================

	SortorianBladerAgent

	===================================================================================
	*/
    /// <summary>
    ///
    /// </summary>

    public class SortorianBladerAgent : INpcAgentBehavior
    {
        public WorkingMemory Memory => _memory;
        private readonly SortorianBladerMemory _memory;

        public int PlanningBuget => 96;

        public AgentArchetype Archetype {
            get {
                throw new System.NotImplementedException();
            }
        }

        public SortorianBladerAgent( INpcAgent agent )
        {
            _memory = new SortorianBladerMemory();
        }

        public void OnPlanBuilt( INpcAgent agent, Plan plan, ReplanReason reasons )
        {
            throw new System.NotImplementedException();
        }

        public void OnPlanFailed( INpcAgent agent, GoalDef goal, ReplanReason reasons )
        {
            throw new System.NotImplementedException();
        }

        public void OnPlanCompleted( INpcAgent agent, GoalDef goal )
        {
            throw new System.NotImplementedException();
        }

        public void OnActionStarted( INpcAgent agent, PlannerAction action )
        {
            throw new System.NotImplementedException();
        }

        public void OnActionSucceeded( INpcAgent agent, PlannerAction action )
        {
            throw new System.NotImplementedException();
        }

        public void OnActionFailed( INpcAgent agent, PlannerAction action )
        {
            throw new System.NotImplementedException();
        }
    };
};
