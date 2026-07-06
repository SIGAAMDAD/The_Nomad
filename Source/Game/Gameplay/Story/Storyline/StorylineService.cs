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
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Game.Gameplay.Story.Runtime;
using Nomad.Game.Sdk.Events.Story;
using Nomad.Game.Sdk.Story;
using Nomad.Game.Sdk.Story.QuestTree;
using Nomad.Game.Gameplay.Runtime.Story.QuestTree;

namespace Nomad.Game.Gameplay.Story
{
	internal sealed class StorylineService : IStorylineService, IDisposable
	{
		public StorylineDefinitionId CurrentStorylineId => Current.Id;

		public IStorylineInstance? Current {
			get {
				throw new NotImplementedException();
			}
		}

		public IGameEvent<StorylineStatusChangedEventArgs> StorylineStatusChanged {
			get {
				throw new NotImplementedException();
			}
		}

		public IGameEvent<StorylineChangedEventArgs> StorylineChanged {
			get {
				throw new NotImplementedException();
			}
		}

		private readonly StoryStateDatabase _database;
		private readonly ILoggerCategory _category;

		private readonly CompiledQuestTree _questTree;

		private bool _isDisposed = false;

		public StorylineService( StoryStateDatabase database )
		{
			_database = database ?? throw new ArgumentNullException( nameof( database ) );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_category.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public bool TrySetActiveStoryline( StorylineDefinitionId storylineId )
		{
			if ( !storylineId.IsValid ) {
				_category.PrintError( $"TrySetActiveStoryline: invalid storylineId." );
				return false;
			}

			return true;
		}

		public IReadOnlyList<StorylineDefinitionId> GetAvailableStorylines()
		{
			throw new System.NotImplementedException();
		}

		public IReadOnlyList<StorylineDefinitionId> GetCompletedStorylines()
		{
			throw new System.NotImplementedException();
		}

		public bool TrySetStoryline( StorylineDefinitionId storylineId )
		{
			throw new System.NotImplementedException();
		}

		public bool TryGetCurrentStoryline( out IStorylineInstance storyline )
		{
			throw new NotImplementedException();
		}

		public bool TryActivateStoryline( StorylineDefinitionId storylineId )
		{
			throw new NotImplementedException();
		}

		public bool TryClearCurrentStoryline()
		{
			throw new NotImplementedException();
		}

		public bool TryGetStorylineStatus( StorylineDefinitionId storylineId, out StorylineStatus status )
		{
			throw new NotImplementedException();
		}

		public bool TrySetStorylineStatus( StorylineDefinitionId storylineId, StorylineStatus status )
		{
			throw new NotImplementedException();
		}

		public bool IsStorylineAvailable( StorylineDefinitionId storylineId )
		{
			throw new NotImplementedException();
		}

		public bool IsStorylineCompleted( StorylineDefinitionId storylineId )
		{
			throw new NotImplementedException();
		}

		public bool IsStorylineFailed( StorylineDefinitionId storylineId )
		{
			throw new NotImplementedException();
		}

		public int CopyAvailableStorylines( Span<StorylineDefinitionId> destination )
		{
			throw new NotImplementedException();
		}

		public int CopyCompletedStorylines( Span<StorylineDefinitionId> destination )
		{
			throw new NotImplementedException();
		}

		public int CopyFailedStorylines( Span<StorylineDefinitionId> destination )
		{
			throw new NotImplementedException();
		}
	};
};
