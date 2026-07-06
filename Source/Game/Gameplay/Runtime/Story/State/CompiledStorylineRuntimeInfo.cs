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

namespace Nomad.Game.Gameplay.Runtime.Story.State
{
	internal readonly struct CompiledStorylineRuntimeInfo
	{
		public readonly StorylineDefinitionId DefinitionId;
		public readonly int FirstQuest;
		public readonly int QuestCount;
		public readonly StorylineStatus DefaultStatus;

		public CompiledStorylineRuntimeInfo(
			StorylineDefinitionId definitionId,
			int firstQuest,
			int questCount,
			StorylineStatus defaultStatus
		)
		{
			DefinitionId = definitionId;
			FirstQuest = firstQuest;
			QuestCount = questCount;
			DefaultStatus = defaultStatus;
		}
	};
};
