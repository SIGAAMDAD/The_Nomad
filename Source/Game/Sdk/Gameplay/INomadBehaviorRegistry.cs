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

using Nomad.Core.Util;
using Nomad.Game.Sdk.Mods;

namespace Nomad.Game.Sdk.Gameplay
{
    /// <summary>
    ///
    /// </summary>
    public interface INomadBehaviorRegistry
    {
        void Add<TBehavior, TDefinition>(
            string behaviorId,
            NomadBehaviorFactory<TBehavior, TDefinition> factory
        )
            where TBehavior : class;

        void Add<TBehavior, TDefinition>(
            InternString behaviorId,
            NomadBehaviorFactory<TBehavior, TDefinition> factory
        )
            where TBehavior : class;

        void Register<TBehavior, TDefinition>(
            InternString behaviorId,
            NomadBehaviorFactory<TBehavior, TDefinition> factory
        )
            where TBehavior : class;

        void Register<TBehavior, TDefinition>(
            InternString behaviorId,
            IModuleContext ownerContext,
            NomadBehaviorFactory<TBehavior, TDefinition> factory
        )
            where TBehavior : class;

        bool Contains<TBehavior>(string behaviorId)
            where TBehavior : class;

        bool Contains<TBehavior>(InternString behaviorId)
            where TBehavior : class;

        TBehavior Create<TBehavior, TDefinition>(
            InternString behaviorId,
            TDefinition definition
        )
            where TBehavior : class;

        TBehavior Create<TBehavior, TDefinition>(
            InternString behaviorId,
            IModuleContext context,
            TDefinition definition
        )
            where TBehavior : class;

        bool TryCreate<TBehavior, TDefinition>(
            InternString behaviorId,
            TDefinition definition,
            out TBehavior behavior
        )
            where TBehavior : class;

        bool TryCreate<TBehavior, TDefinition>(
            InternString behaviorId,
            IModuleContext context,
            TDefinition definition,
            out TBehavior behavior
        )
            where TBehavior : class;
    }
}
