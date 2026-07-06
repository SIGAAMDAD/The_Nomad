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

namespace Nomad.Game.Sdk.Story
{
    public interface IStorylineInstance
    {
        StorylineDefinitionId Id { get; }
        StorylineDefinition Definition { get; }
        StorylineStatus Status { get; }

        int QuestCount { get; }

        QuestDefinition GetQuest(int index);
        QuestStatus GetQuestStatus(int index);

        bool TryGetQuest(QuestDefinitionId questId, out QuestDefinition quest);
        bool TryGetQuestStatus(QuestDefinitionId questId, out QuestStatus status);

        bool TrySetStatus(StorylineStatus status);
        bool TryActivateQuest(QuestDefinitionId questId);

        int CopyAvailableQuests(Span<QuestDefinitionId> destination);
        int CopyCompletedQuests(Span<QuestDefinitionId> destination);
        int CopyFailedQuests(Span<QuestDefinitionId> destination);
    }
}
