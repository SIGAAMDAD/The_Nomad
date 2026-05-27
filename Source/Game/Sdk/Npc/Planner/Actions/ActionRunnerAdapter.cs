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

namespace Nomad.Game.Sdk.Npc.Planner.Actions
{
    public sealed class ActionRunnerAdapter : IActionRunner
    {
        private readonly AiAction _owner;

        public ActionRunnerAdapter(AiAction owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public ActionRunStatus Start(INpcAgent agent)
        {
            return _owner.Start(agent);
        }

        public ActionRunStatus Tick(INpcAgent agent, float dt)
        {
            return _owner.Tick(agent, dt);
        }

        public void Cancel(INpcAgent agent)
        {
            _owner.Cancel(agent);
        }
    }
}
