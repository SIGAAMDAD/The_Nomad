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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Game.Gameplay.Story.Runtime;
using Nomad.Game.Sdk.Events.Story;
using Nomad.Game.Sdk.Story;

namespace Nomad.Game.Gameplay.Story.Quest
{
	internal sealed class QuestService : IQuestService, IDisposable
	{
		public QuestDefinitionId CurrentQuestId {
			get {
				throw new NotImplementedException();
			}
		}

		public IQuestInstance? Current => _current;
		private IQuestInstance? _current = null;

		private readonly StoryStateDatabase _database;
		private readonly ILoggerCategory _category;

		private bool _isDisposed = false;

		public IGameEvent<QuestStatusChangedEventArgs> QuestStatusChanged => _questStatusChanged;
		private readonly IGameEvent<QuestStatusChangedEventArgs> _questStatusChanged = null;

		public IGameEvent<QuestObjectiveStatusChangedEventArgs> QuestObjectiveStatusChanged => _questObjectiveStatusChanged;
		private readonly IGameEvent<QuestObjectiveStatusChangedEventArgs> _questObjectiveStatusChanged = null;

		public QuestService(
			StoryStateDatabase database,
			IGameEventRegistryService eventFactory
		)
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_database = database ?? throw new ArgumentNullException( nameof( database ) );

			_questStatusChanged = eventFactory
				.GetEvent<QuestStatusChangedEventArgs>(
					QuestStatusChangedEventArgs.Name,
					QuestStatusChangedEventArgs.NameSpace
				);

			_questObjectiveStatusChanged = eventFactory
				.GetEvent<QuestObjectiveStatusChangedEventArgs>(
					QuestObjectiveStatusChangedEventArgs.Name,
					QuestObjectiveStatusChangedEventArgs.NameSpace
				);
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_category.Dispose();
			_questStatusChanged.Dispose();
			_questObjectiveStatusChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public bool HasFailedRequiredObjective( QuestId questId )
		{
			return true;
		}

		public bool HasCompletedRequiredObjectives( QuestId questId )
		{
			return true;
		}

		public bool TryGetCurrentQuest( out IQuestInstance quest )
		{
			quest = _current;
			return _current != null;
		}

		public bool TryActivateQuest( QuestDefinitionId questId )
		{
			if ( !questId.IsValid ) {
				return false;
			}

			return true;
		}

		public bool TryClearCurrentQuest()
		{
			throw new NotImplementedException();
		}

		public bool TryGetQuestStatus( QuestDefinitionId questId, out QuestStatus status )
		{
			throw new NotImplementedException();
		}

		public bool TrySetQuestStatus( QuestDefinitionId questId, QuestStatus status )
		{
			throw new NotImplementedException();
		}

		public bool IsQuestActive( QuestDefinitionId questId )
		{
			throw new NotImplementedException();
		}

		public bool IsQuestCompleted( QuestDefinitionId questId )
		{
			throw new NotImplementedException();
		}

		public bool IsQuestFailed( QuestDefinitionId questId )
		{
			throw new NotImplementedException();
		}
	};
};
