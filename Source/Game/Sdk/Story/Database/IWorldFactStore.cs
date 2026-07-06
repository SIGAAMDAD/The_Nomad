/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using Nomad.Game.Sdk.Facts;

namespace Nomad.Game.Sdk.Story.Database
{
    /// <summary>
    /// Compatibility alias. Facts are shared gameplay state, not story-only data.
    /// </summary>
    [Obsolete("Use Nomad.Game.Sdk.Facts.IWorldFactStore.")]
    public interface IWorldFactStore : Nomad.Game.Sdk.Facts.IWorldFactStore
    {
    }
}
