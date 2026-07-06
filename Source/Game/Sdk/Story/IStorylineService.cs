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
using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Story;

namespace Nomad.Game.Sdk.Story
{
    /// <summary>
    ///
    /// </summary>
    public interface IStorylineService
    {
        StorylineDefinitionId CurrentStorylineId { get; }
        IStorylineInstance? Current { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Story")]
        [EventPayload("StorylineId", typeof(StorylineDefinitionId), Order = 1)]
        [EventPayload("OldStatus", typeof(StorylineStatus), Order = 2)]
        [EventPayload("NewStatus", typeof(StorylineStatus), Order = 3)]
        IGameEvent<StorylineStatusChangedEventArgs> StorylineStatusChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Story")]
        [EventPayload("OldStorylineId", typeof(StorylineDefinitionId), Order = 1)]
        [EventPayload("NewStorylineId", typeof(StorylineDefinitionId), Order = 2)]
        IGameEvent<StorylineChangedEventArgs> StorylineChanged { get; }

        bool TryGetCurrentStoryline(out IStorylineInstance storyline);

        bool TryActivateStoryline(StorylineDefinitionId storylineId);
        bool TryClearCurrentStoryline();

        bool TryGetStorylineStatus(StorylineDefinitionId storylineId, out StorylineStatus status);
        bool TrySetStorylineStatus(StorylineDefinitionId storylineId, StorylineStatus status);

        bool IsStorylineAvailable(StorylineDefinitionId storylineId);
        bool IsStorylineCompleted(StorylineDefinitionId storylineId);
        bool IsStorylineFailed(StorylineDefinitionId storylineId);

        int CopyAvailableStorylines(Span<StorylineDefinitionId> destination);
        int CopyCompletedStorylines(Span<StorylineDefinitionId> destination);
        int CopyFailedStorylines(Span<StorylineDefinitionId> destination);
    }
}
