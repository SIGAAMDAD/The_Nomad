/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System.Collections.Generic;

namespace Nomad.Game.Sdk.Story.State
{
    public interface IQuestStateStore
    {
        bool TrySetQuestState(QuestDefinitionId questId, QuestStatus status);
        bool TryGetQuestState(QuestDefinitionId questId, out QuestStatus status);
        IReadOnlyDictionary<QuestDefinitionId, QuestStatus> GetAllQuestStates();
    }
}
