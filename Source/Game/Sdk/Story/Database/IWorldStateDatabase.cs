/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using Nomad.Game.Sdk.Facts;
using Nomad.Game.Sdk.Story.State;

namespace Nomad.Game.Sdk.Story
{
    public interface IWorldStateDatabase :
        IWorldFactStore,
        IQuestStateStore,
        IStorylineStateStore
    {
    }
}
