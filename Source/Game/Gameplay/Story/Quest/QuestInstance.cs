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
using Nomad.Game.Gameplay.Story.Quest;
using Nomad.Game.Gameplay.Story.Runtime;
using Nomad.Game.Sdk.Story;

namespace Nomad.Game.Gameplay.Story
{
	/*
	===================================================================================

	QuestInstance

	===================================================================================
	*/
	/// <summary>
	/// A temporary active handle/facade to a quest row in the world-state database.
	/// </summary>

	internal sealed class QuestInstance : IQuestInstance
	{
		public QuestDefinition Definition {
			get {
				throw new NotImplementedException();
			}
		}
		private readonly QuestDefinition _definition;

		public QuestStatus Status => _status;
		private QuestStatus _status;

		public int ObjectiveCount => 0;

		private readonly QuestId _questId;
		private readonly StoryStateDatabase _database;
		private readonly QuestService _service;

		public QuestInstance( StoryStateDatabase database )
		{
			_database = database ?? throw new ArgumentNullException( nameof( database ) );
		}

		public void Dispose()
		{
		}

		public int CopyCompletedObjectives( Span<QuestObjectiveId> destination )
		{
			throw new NotImplementedException();
		}

		public int CopyFailedObjectives( Span<QuestObjectiveId> destination )
		{
			throw new NotImplementedException();
		}

		public int CopyOptionalObjectives( Span<QuestObjectiveId> destination )
		{
			throw new NotImplementedException();
		}

		public QuestObjectiveDefinition GetObjective( int index )
		{
			throw new NotImplementedException();
		}

		public QuestObjectiveStatus GetObjectiveStatus( int index )
		{
			throw new NotImplementedException();
		}

		public QuestRelationship GetRelationship( QuestDefinitionId otherQuestId )
		{
			throw new NotImplementedException();
		}

		public bool HasCompletedRequiredObjectives()
		{
			throw new NotImplementedException();
		}

		public bool HasFailedRequiredObjectives()
		{
			throw new NotImplementedException();
		}

		public bool TryGetObjective( QuestObjectiveId objectiveId, out QuestObjectiveDefinition objective )
		{
			objective = null;
			if ( !objectiveId.IsValid ) {
				return false;
			}

			return true;
		}

		public bool TryGetObjectiveStatus( QuestObjectiveId objectiveId, out QuestObjectiveStatus status )
		{
			throw new NotImplementedException();
		}

		public bool TrySetObjectiveStatus( QuestObjectiveId objectiveId, QuestObjectiveStatus status )
		{
			throw new NotImplementedException();
		}

		public bool TrySetStatus( QuestStatus status )
		{
			throw new NotImplementedException();
		}
	};
};
