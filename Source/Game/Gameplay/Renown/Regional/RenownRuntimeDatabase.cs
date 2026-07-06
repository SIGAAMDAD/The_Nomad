/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using Nomad.Game.Gameplay.Runtime.Renown;

namespace Nomad.Game.Gameplay.Renown.Regional
{
    /// <summary>
    /// Compatibility name for the regional renown runtime state container.
    /// </summary>
    [Obsolete("Use RegionalRenownRuntimeState.")]
    internal sealed class RenownRuntimeDatabase : RegionalRenownRuntimeState
    {
        public RenownRuntimeDatabase(CompiledRegionalRenownDatabase database)
            : base(database)
        {
        }
    }
}
