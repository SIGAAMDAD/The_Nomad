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
using Nomad.Core.Events;
using Nomad.Game.Gameplay.Story.Runtime;
using Nomad.Game.Sdk.Story;

namespace Nomad.Game.Gameplay.Story
{
	internal sealed class StorylineInstance : IStorylineInstance
	{
		private readonly StoryStateDatabase _database;

		public StorylineStatus Status { get; }

		public StorylineDefinitionId Id {
			get {
				throw new NotImplementedException();
			}
		}

		public StorylineDefinition Definition {
			get {
				throw new NotImplementedException();
			}
		}

		public int QuestCount {
			get {
				throw new NotImplementedException();
			}
		}

		public StorylineInstance(
			StoryStateDatabase database,
			IGameEventRegistryService eventFactory
		)
		{
			_database = database ?? throw new ArgumentNullException( nameof( database ) );
		}

		public QuestDefinition GetQuest( int index )
		{
			throw new NotImplementedException();
		}

		public QuestStatus GetQuestStatus( int index )
		{
			throw new NotImplementedException();
		}

		public bool TryGetQuest( QuestDefinitionId questId, out QuestDefinition quest )
		{
			throw new NotImplementedException();
		}

		public bool TryGetQuestStatus( QuestDefinitionId questId, out QuestStatus status )
		{
			throw new NotImplementedException();
		}

		public bool TrySetStatus( StorylineStatus status )
		{
			throw new NotImplementedException();
		}

		public bool TryActivateQuest( QuestDefinitionId questId )
		{
			throw new NotImplementedException();
		}

		public int CopyAvailableQuests( Span<QuestDefinitionId> destination )
		{
			throw new NotImplementedException();
		}

		public int CopyCompletedQuests( Span<QuestDefinitionId> destination )
		{
			throw new NotImplementedException();
		}

		public int CopyFailedQuests( Span<QuestDefinitionId> destination )
		{
			throw new NotImplementedException();
		}
	}
}
