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
using Nomad.Game.Sdk.Story;
using Nomad.Game.Sdk.Story.QuestTree;
using Nomad.Game.Gameplay.Runtime.Story.QuestTree;
using Nomad.Game.Content.Compilation.Story;

namespace Nomad.Game.Gameplay.Story
{
	internal sealed class QuestTreeService : IQuestTreeService
	{
		public IQuestTree? Current => _current;
		private IQuestTree? _current;

		public IReadOnlyCollection<StorylineDefinitionId> Storylines => _trees.Keys;
		private readonly Dictionary<StorylineDefinitionId, IQuestTree> _trees = new();

		public bool TryRegisterTree( IQuestTree tree )
		{
			if ( tree == null || !tree.Definition.StorylineId.IsValid ) {
				return false;
			}

			_trees[tree.Definition.StorylineId] = tree;
			return true;
		}

		public bool TryBuildAndRegisterTree( StorylineDefinition storylineDefinition )
		{
			if ( storylineDefinition == null || !storylineDefinition.Id.IsValid ) {
				return false;
			}

			CompiledQuestTree tree = QuestTreeCompiler.FromStoryline( storylineDefinition );
			return TryRegisterTree( tree );
		}

		public bool TryGetTree( StorylineDefinitionId storylineId, out IQuestTree tree )
		{
			return _trees.TryGetValue( storylineId, out tree! );
		}

		public bool TrySetCurrentTree( StorylineDefinitionId storylineId )
		{
			if ( !_trees.TryGetValue( storylineId, out IQuestTree? tree ) ) {
				return false;
			}

			_current = tree;
			return true;
		}
	}
}
