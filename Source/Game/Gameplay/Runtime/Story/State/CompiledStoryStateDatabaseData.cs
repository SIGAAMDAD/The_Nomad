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
using Nomad.Core.Util;

namespace Nomad.Game.Gameplay.Runtime.Story.State
{
	internal sealed class CompiledStoryStateDatabaseData
	{
		public static readonly CompiledStoryStateDatabaseData Empty = new CompiledStoryStateDatabaseData(
			Array.Empty<InternString>(),
			Array.Empty<CompiledQuestRuntimeInfo>(),
			Array.Empty<CompiledObjectiveRuntimeInfo>(),
			Array.Empty<CompiledStorylineRuntimeInfo>()
		);

		public readonly InternString[] Facts;
		public readonly CompiledQuestRuntimeInfo[] Quests;
		public readonly CompiledObjectiveRuntimeInfo[] Objectives;
		public readonly CompiledStorylineRuntimeInfo[] Storylines;

		public CompiledStoryStateDatabaseData(
			InternString[] facts,
			CompiledQuestRuntimeInfo[] quests,
			CompiledObjectiveRuntimeInfo[] objectives,
			CompiledStorylineRuntimeInfo[] storylines
		)
		{
			Facts = facts;
			Quests = quests;
			Objectives = objectives;
			Storylines = storylines;
		}
	};
};
