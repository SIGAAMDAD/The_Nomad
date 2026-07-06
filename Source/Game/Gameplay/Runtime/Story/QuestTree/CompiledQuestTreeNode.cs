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

using Nomad.Game.Sdk.Story;
using Nomad.Game.Sdk.Story.QuestTree;

namespace Nomad.Game.Gameplay.Runtime.Story.QuestTree
{
	internal readonly struct CompiledQuestTreeNode
	{
		public readonly QuestNode Source;
		public readonly QuestNodeTraversalFlags TraversalFlags;
		public readonly ulong RequiredCompletedMask;
		public readonly ulong RequiredFailedMask;
		public readonly ulong RequiredTerminalMask;
		public readonly CompiledQuestCondition Condition;

		public QuestDefinitionId QuestId => Source.QuestId;
		public QuestNodeType Type => Source.Type;
		public bool IsHidden => Source.IsHidden;

		public CompiledQuestTreeNode(
			QuestNode source,
			ulong requiredCompletedMask,
			ulong requiredFailedMask,
			ulong requiredTerminalMask,
			CompiledQuestCondition condition
		)
		{
			Source = source;
			TraversalFlags = source.Type.GetTraversalFlags();
			RequiredCompletedMask = requiredCompletedMask;
			RequiredFailedMask = requiredFailedMask;
			RequiredTerminalMask = requiredTerminalMask;
			Condition = condition;
		}
	};
};
