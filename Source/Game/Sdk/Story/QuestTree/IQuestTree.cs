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
using Nomad.Game.Sdk.Story;

namespace Nomad.Game.Sdk.Story.QuestTree
{
    public interface IQuestTree
    {
        QuestTreeDefinition Definition { get; }
        int NodeCount { get; }
        int EdgeCount { get; }
        bool HasCycles { get; }

        ReadOnlySpan<QuestNode> Nodes { get; }
        ReadOnlySpan<QuestTreeEdge> Edges { get; }
        ReadOnlySpan<QuestDefinitionId> EntryQuests { get; }
        ReadOnlySpan<QuestDefinitionId> TopologicalOrder { get; }

        bool ContainsQuest(QuestDefinitionId questId);
        bool TryGetNode(QuestDefinitionId questId, out QuestNode node);
        bool TryGetNodeTypeInfo(QuestDefinitionId questId, out QuestNodeTypeInfo typeInfo);
        QuestRelationship GetRelationship(QuestDefinitionId questId, QuestDefinitionId otherQuestId);
        bool IsReachable(QuestDefinitionId fromQuestId, QuestDefinitionId toQuestId);
        bool CanEnterNode(QuestDefinitionId questId, IWorldStateDatabase stateDatabase);
        bool CanExitNode(QuestDefinitionId questId, IWorldStateDatabase stateDatabase);

        ReadOnlySpan<QuestTreeEdge> GetOutgoingEdges(QuestDefinitionId questId);
        ReadOnlySpan<QuestTreeEdge> GetIncomingEdges(QuestDefinitionId questId);

        int CopyChildren(QuestDefinitionId questId, Span<QuestDefinitionId> destination);
        int CopyParents(QuestDefinitionId questId, Span<QuestDefinitionId> destination);
        int CopyTraversableChildren(QuestDefinitionId questId, IWorldStateDatabase stateDatabase, Span<QuestDefinitionId> destination, bool includeHidden = false);
        int CopyTraversableParents(QuestDefinitionId questId, IWorldStateDatabase stateDatabase, Span<QuestDefinitionId> destination, bool includeHidden = false);
        int CopyAvailableQuests(IWorldStateDatabase stateDatabase, Span<QuestDefinitionId> destination);
        bool CanStartQuest(QuestDefinitionId questId, IWorldStateDatabase stateDatabase);
    }
}
