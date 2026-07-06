/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System.Collections.Generic;

namespace Nomad.Game.Sdk.Story.State
{
    public interface IStorylineStateStore
    {
        bool TrySetStorylineState(StorylineDefinitionId storylineId, StorylineStatus status);
        bool TryGetStorylineState(StorylineDefinitionId storylineId, out StorylineStatus status);
        IReadOnlyDictionary<StorylineDefinitionId, StorylineStatus> GetAllStorylineStates();
    }
}
