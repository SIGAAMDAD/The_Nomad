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
using Nomad.Core.Util;
using Nomad.Game.Gameplay.Runtime.Story;
using Nomad.Game.Gameplay.Runtime.Story.QuestTree;
using Nomad.Game.Gameplay.Runtime.Story.State;
using Nomad.Game.Sdk.Story;

namespace Nomad.Game.Content.Compilation.Story
{
	internal sealed class StoryCompiler
	{
		public CompiledStoryDatabase Compile( IEnumerable<StorylineDefinition> storylines, IEnumerable<InternString>? facts = null )
		{
			if ( storylines == null ) {
				return CompiledStoryDatabase.Empty;
			}

			var storylineList = new List<StorylineDefinition>();
			var questInfos = new List<CompiledQuestRuntimeInfo>();
			var objectiveInfos = new List<CompiledObjectiveRuntimeInfo>();
			var storylineInfos = new List<CompiledStorylineRuntimeInfo>();
			var questTrees = new List<CompiledQuestTree>();
			var storylineIndexById = new Dictionary<StorylineDefinitionId, ushort>();

			foreach ( StorylineDefinition storyline in storylines ) {
				if ( !storyline.Id.IsValid || storylineIndexById.ContainsKey( storyline.Id ) ) {
					continue;
				}

				ushort storylineIndex = checked((ushort)storylineList.Count);
				storylineList.Add( storyline );
				storylineIndexById.Add( storyline.Id, storylineIndex );

				int firstQuest = questInfos.Count;

				for ( int questIndex = 0; questIndex < storyline.Quests.Count; questIndex++ ) {
					QuestDefinition quest = storyline.Quests[questIndex];
					int firstObjective = objectiveInfos.Count;

					for ( int objectiveIndex = 0; objectiveIndex < quest.Objectives.Count; objectiveIndex++ ) {
						QuestObjectiveDefinition objective = quest.Objectives[objectiveIndex];
						objectiveInfos.Add(
							new CompiledObjectiveRuntimeInfo(
								objective.Id,
								questInfos.Count,
								objective.Optional ? QuestObjectiveStatus.SideIncomplete : QuestObjectiveStatus.Inactive,
								objective.Optional
							)
						);
					}

					questInfos.Add(
						new CompiledQuestRuntimeInfo(
							quest.Id,
							firstObjective,
							quest.Objectives.Count,
							QuestStatus.Inactive
						)
					);
				}

				storylineInfos.Add(
					new CompiledStorylineRuntimeInfo(
						storyline.Id,
						firstQuest,
						storyline.Quests.Count,
						StorylineStatus.Inactive
					)
				);

				questTrees.Add( QuestTreeCompiler.FromStoryline( storyline ) );
			}

			InternString[] factArray = facts == null ? Array.Empty<InternString>() : new List<InternString>( facts ).ToArray();

			var stateData = new CompiledStoryStateDatabaseData(
				factArray,
				questInfos.ToArray(),
				objectiveInfos.ToArray(),
				storylineInfos.ToArray()
			);

			return new CompiledStoryDatabase(
				stateData,
				questTrees.ToArray(),
				storylineIndexById
			);
		}
	};
};
