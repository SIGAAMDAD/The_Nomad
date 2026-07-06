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

using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Story;

namespace Nomad.Game.Sdk.Story
{
    public interface IQuestService
    {
        QuestDefinitionId CurrentQuestId { get; }
        IQuestInstance? Current { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Story")]
        [EventPayload("QuestId", typeof(QuestDefinitionId), Order = 1)]
        [EventPayload("OldStatus", typeof(QuestStatus), Order = 2)]
        [EventPayload("NewStatus", typeof(QuestStatus), Order = 3)]
        IGameEvent<QuestStatusChangedEventArgs> QuestStatusChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Story")]
        [EventPayload("QuestId", typeof(QuestDefinitionId), Order = 1)]
        [EventPayload("ObjectiveId", typeof(QuestObjectiveId), Order = 2)]
        [EventPayload("OldStatus", typeof(QuestObjectiveStatus), Order = 3)]
        [EventPayload("NewStatus", typeof(QuestObjectiveStatus), Order = 4)]
        IGameEvent<QuestObjectiveStatusChangedEventArgs> QuestObjectiveStatusChanged { get; }

        bool TryGetCurrentQuest(out IQuestInstance quest);

        bool TryActivateQuest(QuestDefinitionId questId);
        bool TryClearCurrentQuest();

        bool TryGetQuestStatus(QuestDefinitionId questId, out QuestStatus status);
        bool TrySetQuestStatus(QuestDefinitionId questId, QuestStatus status);

        bool IsQuestActive(QuestDefinitionId questId);
        bool IsQuestCompleted(QuestDefinitionId questId);
        bool IsQuestFailed(QuestDefinitionId questId);
    }
}
