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

using Nomad.Game.Sdk.Gameplay.Npc;
using Nomad.Game.Sdk.Npc.Planner;

namespace Nomad.Game.Sdk.Npc
{
    public interface INpcAgent : INpcEntity
    {
        WorkingMemory Memory { get; }
        WorldState CurrentState { get; }
        Plan CurrentPlan { get; }

        bool HasPlan { get; }
        bool IsExecutingAction { get; }

        NpcAgentId AgentId { get; }

        void ForceReplan(ReplanReason reason = ReplanReason.ContextInvalid);
        void NotifyHeavyDamage();
        void NotifyTargetLost();
    }
}
