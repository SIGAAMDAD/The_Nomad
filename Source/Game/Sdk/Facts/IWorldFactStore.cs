/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Facts
{
    /// <summary>
    /// Shared gameplay fact store used by story, dialogue, AI, areas, and world triggers.
    /// </summary>
    public interface IWorldFactStore
    {
        bool TryGetFact(InternString fact, out bool value);
        bool TrySetFact(InternString fact, bool value);
        bool HasFact(InternString fact);
        bool TryRemoveFact(InternString fact);
    }
}
