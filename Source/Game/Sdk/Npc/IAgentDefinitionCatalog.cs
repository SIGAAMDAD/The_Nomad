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

namespace Nomad.Game.Sdk.Npc
{
    /// <summary>
    /// Read-only catalog access for authored NPC agent definitions.
    /// </summary>
    public interface IAgentDefinitionCatalog
    {
        AgentDefinition? GetAgent(AgentDefinitionId id);
        AgentGoalDefinition? GetGoal(AgentGoalDefinitionId id);
        AgentActionDefinition? GetAction(AgentActionDefinitionId id);
        AgentSensorDefinition? GetSensor(AgentSensorDefinitionId id);

        bool TryGetAgent(AgentDefinitionId id, out AgentDefinition? definition);
        bool TryGetGoal(AgentGoalDefinitionId id, out AgentGoalDefinition? definition);
        bool TryGetAction(AgentActionDefinitionId id, out AgentActionDefinition? definition);
        bool TryGetSensor(AgentSensorDefinitionId id, out AgentSensorDefinition? definition);

        void Clear();
    }
}
