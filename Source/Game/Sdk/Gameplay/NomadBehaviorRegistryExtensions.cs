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

using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Mods;
using Nomad.Game.Sdk.Npc.Planner;

namespace Nomad.Game.Sdk.Gameplay
{
    public static class NomadBehaviorRegistryExtensions
    {
        public static void AddConsumable(
            this INomadBehaviorRegistry behaviors,
            string behaviorId,
            NomadBehaviorFactory<IConsumableBehavior, ConsumableDefinition> factory
        )
        {
            behaviors.Add(behaviorId, factory);
        }

        public static void AddConsumable<TBehavior>(
            this INomadBehaviorRegistry behaviors,
            string behaviorId,
            NomadBehaviorFactory<TBehavior, ConsumableDefinition> factory
        )
            where TBehavior : class, IConsumableBehavior
        {
            behaviors.Add<IConsumableBehavior, ConsumableDefinition>(
                behaviorId,
                (IModuleContext context, ConsumableDefinition definition) => factory(context, definition)
            );
        }

        public static void AddNpcAgent<TBehavior, TDefinition>(
            this INomadBehaviorRegistry behaviors,
            string behaviorId,
            NomadBehaviorFactory<TBehavior, TDefinition> factory
        )
            where TBehavior : class, INpcAgentBehavior
        {
            behaviors.Add<INpcAgentBehavior, TDefinition>(
                behaviorId,
                (IModuleContext context, TDefinition definition) => factory(context, definition)
            );
        }
    };
};
