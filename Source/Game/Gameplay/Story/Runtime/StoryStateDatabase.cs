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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nomad.Core.Collections;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Numerics;
using Nomad.Core.Util;
using Nomad.Game.Gameplay.Runtime.Story.State;
using Nomad.Game.Sdk.Story;

namespace Nomad.Game.Gameplay.Story.Runtime
{
	/*
	===================================================================================

	StoryStateDatabase

	===================================================================================
	*/
	/// <summary>
	/// Handles high-performance story/fact state database read/write access.
	/// </summary>

	internal unsafe sealed class StoryStateDatabase : IWorldStateDatabase, IDisposable
	{
		/// <summary>
		/// Meant for both GOAP AI access and quest objective derived condition evaluation.
		/// </summary>
		private readonly InternStringLocalIdTable _factIds;
		private readonly InternStringLocalIdTable _questIds;
		private readonly InternStringLocalIdTable _objectiveIds;
		private readonly InternStringLocalIdTable _storylineIds;

		private readonly InternString[] _factKeys;
		private readonly QuestDefinitionId[] _questKeys;
		private readonly QuestObjectiveId[] _objectiveKeys;
		private readonly StorylineDefinitionId[] _storylineKeys;

		private readonly CompiledQuestRuntimeInfo[] _questInfo;
		private readonly CompiledObjectiveRuntimeInfo[] _objectiveInfo;
		private readonly CompiledStorylineRuntimeInfo[] _storylineInfo;
		private readonly CompiledStoryStateDatabaseData _compiledInfo;

		private readonly DenseIdSet _knownFacts;
		private readonly DenseIdSet _trueFacts;

		private readonly byte* _statusMemoryPtr;
		private readonly byte* _questStatuses;
		private readonly int _questCount = 0;

		private readonly byte* _objectiveStatuses;
		private readonly int _objectiveCount = 0;

		private readonly byte* _storylineStatuses;
		private readonly int _storylineCount = 0;

		private readonly SparseSet _activeQuests;
		private readonly SparseSet _activeObjectives;
		private readonly SparseSet _activeStorylines;

		private readonly DirtySet _dirtyFacts;
		private readonly DirtySet _dirtyQuests;
		private readonly DirtySet _dirtyObjectives;
		private readonly DirtySet _dirtyStorylines;

		private bool _isDisposed = false;

		public int Version {
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get => _version;
		}
		private int _version = 1;

		/*
		===============
		StoryStateDatabase
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="compiled"></param>
		public StoryStateDatabase( CompiledStoryStateDatabaseData compiled )
		{
			ArgumentGuard.ThrowIfNull( compiled, nameof( compiled ) );
			_compiledInfo = compiled;

			int factCount = compiled.Facts.Length;
			_questCount = compiled.Quests.Length;
			_objectiveCount = compiled.Objectives.Length;
			_storylineCount = compiled.Storylines.Length;

			_factIds = new InternStringLocalIdTable( factCount + 1, factCount );
			_questIds = new InternStringLocalIdTable( _questCount + 1, _questCount );
			_objectiveIds = new InternStringLocalIdTable( _objectiveCount + 1, _objectiveCount );
			_storylineIds = new InternStringLocalIdTable( _storylineCount + 1, _storylineCount );

			_factKeys = new InternString[factCount];
			_questKeys = new QuestDefinitionId[_questCount];
			_objectiveKeys = new QuestObjectiveId[_objectiveCount];
			_storylineKeys = new StorylineDefinitionId[_storylineCount];

			_questInfo = compiled.Quests;
			_objectiveInfo = compiled.Objectives;
			_storylineInfo = compiled.Storylines;

			_knownFacts = new DenseIdSet( factCount );
			_trueFacts = new DenseIdSet( factCount );

			int statusByteCount = MemoryMath.AlignUp(
				_questCount + _objectiveCount + _storylineCount * sizeof( byte ),
				MemoryMath.BitsPerUInt64
			);

			_statusMemoryPtr = (byte*)NativeMemory.AlignedAlloc( (nuint)statusByteCount, MemoryMath.BitsPerUInt64 );
			_questStatuses = _statusMemoryPtr;
			_objectiveStatuses = (byte*)MemoryMath.AlignUp( (UIntPtr)(_questStatuses + _questCount), MemoryMath.BitsPerUInt64 );
			_storylineStatuses = (byte*)MemoryMath.AlignUp( (UIntPtr)(_objectiveStatuses + _objectiveCount), MemoryMath.BitsPerUInt64 );

			_activeQuests = new SparseSet( _questCount, Math.Min( _questCount, 64 ) );
			_activeObjectives = new SparseSet( _objectiveCount, Math.Min( _objectiveCount, 128 ) );
			_activeStorylines = new SparseSet( _storylineCount, Math.Min( _storylineCount, 32 ) );

			_dirtyFacts = new DirtySet( factCount, Math.Min( factCount, 64 ) );
			_dirtyQuests = new DirtySet( _questCount, Math.Min( _questCount, 64 ) );
			_dirtyObjectives = new DirtySet( _objectiveCount, Math.Min( _objectiveCount, 128 ) );
			_dirtyStorylines = new DirtySet( _storylineCount, Math.Min( _storylineCount, 32 ) );

			RegisterFacts( compiled.Facts );
			RegisterQuests( compiled.Quests );
			RegisterObjectives( compiled.Objectives );
			RegisterStorylines( compiled.Storylines );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			if ( _statusMemoryPtr != null ) {
				NativeMemory.AlignedFree( _statusMemoryPtr );
			}

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		ResolveFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fact"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool ResolveFact( InternString fact, out WorldFactId id )
		{
			if ( _factIds.TryResolve( fact, out int local ) ) {
				id = new WorldFactId( local );
				return true;
			}

			id = WorldFactId.Invalid;
			return false;
		}

		/*
		===============
		ResolveQuest
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool ResolveQuest( QuestDefinitionId questId, out QuestId id )
		{
			if ( !questId.IsValid ) {
				id = QuestId.Invalid;
				return false;
			}

			if ( _questIds.TryResolve( questId.Value, out int local ) ) {
				id = new QuestId( local );
				return true;
			}

			id = QuestId.Invalid;
			return false;
		}

		/*
		===============
		ResolveObjective
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="objectiveId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool ResolveObjective( QuestObjectiveId objectiveId, out ObjectiveId id )
		{
			if ( !objectiveId.IsValid ) {
				id = ObjectiveId.Invalid;
				return false;
			}

			if ( _objectiveIds.TryResolve( objectiveId.Value, out int local ) ) {
				id = new ObjectiveId( local );
				return true;
			}

			id = ObjectiveId.Invalid;
			return false;
		}

		/*
		===============
		ResolveStoryline
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="storylineId"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool ResolveStoryline( StorylineDefinitionId storylineId, out StorylineId id )
		{
			if ( !storylineId.IsValid ) {
				id = StorylineId.Invalid;
				return false;
			}

			if ( _storylineIds.TryResolve( storylineId.Value, out int local ) ) {
				id = new StorylineId( local );
				return true;
			}

			id = StorylineId.Invalid;
			return false;
		}

		/*
		===============
		HasFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool HasFact( WorldFactId id )
		{
			return id.IsValid && _knownFacts.Contains( id.Value ) && _trueFacts.Contains( id.Value );
		}

		/*
		===============
		IsFactKnown
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool IsFactKnown( WorldFactId id )
		{
			return id.IsValid && _knownFacts.Contains( id.Value );
		}

		/*
		===============
		TryGetFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool TryGetFact( WorldFactId id, out bool value )
		{
			if ( !id.IsValid || !_knownFacts.Contains( id.Value ) ) {
				value = false;
				return false;
			}

			value = _trueFacts.Contains( id.Value );
			return true;
		}

		/*
		===============
		TrySetFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TrySetFact( WorldFactId id, bool value )
		{
			if ( !id.IsValid ) {
				return false;
			}

			bool wasKnown = _knownFacts.Contains( id.Value );
			bool oldValue = _trueFacts.Contains( id.Value );

			if ( wasKnown && oldValue == value ) {
				return false;
			}

			_knownFacts.Add( id.Value );

			if ( value ) {
				_trueFacts.Add( id.Value );
			} else {
				_trueFacts.Remove( id.Value );
			}

			_dirtyFacts.MarkDirty( id.Value );
			_version++;
			return true;
		}

		/*
		===============
		TryRemoveFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool TryRemoveFact( WorldFactId id )
		{
			if ( !id.IsValid ) {
				return false;
			}

			bool wasKnown = _knownFacts.Remove( id.Value );
			bool wasTrue = _trueFacts.Remove( id.Value );

			if ( !wasKnown && !wasTrue ) {
				return false;
			}

			_dirtyFacts.MarkDirty( id.Value );
			_version++;
			return true;
		}

		/*
		===============
		TryGetQuestState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool TryGetQuestState( QuestId id, out QuestStatus status )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_questCount ) {
				status = default;
				return false;
			}

			if ( !_activeQuests.Contains( id.Value ) ) {
				status = _questInfo[id.Value].DefaultStatus;
				return false;
			}

			status = (QuestStatus)_questStatuses[id.Value];
			return true;
		}

		/*
		===============
		GetQuestStateOrDefault
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public QuestStatus GetQuestStateOrDefault( QuestId id )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_questCount ) {
				return default;
			}

			if ( !_activeQuests.Contains( id.Value ) ) {
				return _questInfo[id.Value].DefaultStatus;
			}

			return (QuestStatus)_questStatuses[id.Value];
		}

		/*
		===============
		TrySetQuestState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool TrySetQuestState( QuestId id, QuestStatus status )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_questCount ) {
				return false;
			}

			QuestStatus oldStatus = GetQuestStateOrDefault( id );
			if ( oldStatus == status && _activeQuests.Contains( id.Value ) ) {
				return false;
			}

			_activeQuests.Add( id.Value );
			_questStatuses[id.Value] = (byte)status;

			_dirtyQuests.MarkDirty( id.Value );
			_version++;
			return true;
		}

		/*
		===============
		TryGetObjectiveState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool TryGetObjectiveState( ObjectiveId id, out QuestObjectiveStatus status )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_objectiveCount ) {
				status = default;
				return false;
			}

			if ( !_activeObjectives.Contains( id.Value ) ) {
				status = _objectiveInfo[id.Value].DefaultStatus;
				return false;
			}

			status = (QuestObjectiveStatus)_objectiveStatuses[id.Value];
			return true;
		}

		/*
		===============
		GetObjectiveStateOrDefault
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public QuestObjectiveStatus GetObjectiveStateOrDefault( ObjectiveId id )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_objectiveCount ) {
				return default;
			}

			if ( !_activeObjectives.Contains( id.Value ) ) {
				return _objectiveInfo[id.Value].DefaultStatus;
			}

			return (QuestObjectiveStatus)_objectiveStatuses[id.Value];
		}

		/*
		===============
		TrySetObjectiveState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool TrySetObjectiveState( ObjectiveId id, QuestObjectiveStatus status )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_objectiveCount ) {
				return false;
			}

			QuestObjectiveStatus oldStatus = GetObjectiveStateOrDefault( id );
			if ( oldStatus == status && _activeObjectives.Contains( id.Value ) ) {
				return false;
			}

			_activeObjectives.Add( id.Value );
			_objectiveStatuses[id.Value] = (byte)status;

			int questId = _objectiveInfo[id.Value].QuestId;
			if ( questId >= 0 ) {
				_activeQuests.Add( questId );
				_dirtyQuests.MarkDirty( questId );
			}

			_dirtyObjectives.MarkDirty( id.Value );
			_version++;
			return true;
		}

		/*
		===============
		TryGetStorylineState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool TryGetStorylineState( StorylineId id, out StorylineStatus status )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_storylineCount ) {
				status = default;
				return false;
			}

			if ( !_activeStorylines.Contains( id.Value ) ) {
				status = _storylineInfo[id.Value].DefaultStatus;
				return false;
			}

			status = (StorylineStatus)_storylineStatuses[id.Value];
			return true;
		}

		/*
		===============
		GetStorylineStateOrDefault
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public StorylineStatus GetStorylineStateOrDefault( StorylineId id )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_storylineCount ) {
				return default;
			}

			if ( !_activeStorylines.Contains( id.Value ) ) {
				return _storylineInfo[id.Value].DefaultStatus;
			}

			return (StorylineStatus)_storylineStatuses[id.Value];
		}

		/*
		===============
		TrySetStorylineState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool TrySetStorylineState( StorylineId id, StorylineStatus status )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_storylineCount ) {
				return false;
			}

			StorylineStatus oldStatus = GetStorylineStateOrDefault( id );
			if ( oldStatus == status && _activeStorylines.Contains( id.Value ) ) {
				return false;
			}

			_activeStorylines.Add( id.Value );
			_storylineStatuses[id.Value] = (byte)status;

			_dirtyStorylines.MarkDirty( id.Value );
			_version++;
			return true;
		}

		/*
		===============
		GetQuestObjectives
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ReadOnlySpan<CompiledObjectiveRuntimeInfo> GetQuestObjectives( QuestId id )
		{
			if ( !id.IsValid || (uint)id.Value >= (uint)_questInfo.Length ) {
				return ReadOnlySpan<CompiledObjectiveRuntimeInfo>.Empty;
			}

			CompiledQuestRuntimeInfo info = _questInfo[id.Value];

			return _objectiveInfo.AsSpan( info.FirstObjective, info.ObjectiveCount );
		}

		/*
		===============
		GetDirtyFacts
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Span<int> GetDirtyFacts()
		{
			return _dirtyFacts.DirtyIds;
		}

		/*
		===============
		GetDirtyQuests
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Span<int> GetDirtyQuests()
		{
			return _dirtyQuests.DirtyIds;
		}

		/*
		===============
		GetDirtyObjectives
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Span<int> GetDirtyObjectives()
		{
			return _dirtyObjectives.DirtyIds;
		}

		/*
		===============
		GetDirtyStorylines
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Span<int> GetDirtyStorylines()
		{
			return _dirtyStorylines.DirtyIds;
		}

		/*
		===============
		ClearDirty
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void ClearDirty()
		{
			_dirtyFacts.ClearDirty();
			_dirtyQuests.ClearDirty();
			_dirtyObjectives.ClearDirty();
			_dirtyStorylines.ClearDirty();
		}

		/*
		===============
		HasFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fact"></param>
		/// <returns></returns>
		public bool HasFact( InternString fact )
		{
			return ResolveFact( fact, out WorldFactId id ) && HasFact( id );
		}

		/*
		===============
		TryGetFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fact"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetFact( InternString fact, out bool value )
		{
			if ( !ResolveFact( fact, out WorldFactId id ) ) {
				value = false;
				return false;
			}

			return TryGetFact( id, out value );
		}

		/*
		===============
		TrySetFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fact"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TrySetFact( InternString fact, bool value )
		{
			if ( !ResolveFact( fact, out WorldFactId id ) ) {
				return false;
			}

			return TrySetFact( id, value );
		}

		/*
		===============
		TryRemoveFact
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fact"></param>
		/// <returns></returns>
		public bool TryRemoveFact( InternString fact )
		{
			if ( !ResolveFact( fact, out WorldFactId id ) ) {
				return false;
			}

			return TryRemoveFact( id );
		}

		/*
		===============
		TryGetQuestState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool TryGetQuestState( QuestDefinitionId questId, out QuestStatus status )
		{
			if ( !ResolveQuest( questId, out QuestId id ) ) {
				status = default;
				return false;
			}

			return TryGetQuestState( id, out status );
		}

		/*
		===============
		TrySetQuestState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="questId"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool TrySetQuestState( QuestDefinitionId questId, QuestStatus status )
		{
			if ( !ResolveQuest( questId, out QuestId id ) ) {
				return false;
			}

			return TrySetQuestState( id, status );
		}

		/*
		===============
		TryGetStorylineState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="storylineId"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool TryGetStorylineState( StorylineDefinitionId storylineId, out StorylineStatus status )
		{
			if ( !ResolveStoryline( storylineId, out StorylineId id ) ) {
				status = default;
				return false;
			}

			return TryGetStorylineState( id, out status );
		}

		/*
		===============
		TrySetStorylineState
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="storylineId"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool TrySetStorylineState( StorylineDefinitionId storylineId, StorylineStatus status )
		{
			if ( !ResolveStoryline( storylineId, out StorylineId id ) ) {
				return false;
			}

			return TrySetStorylineState( id, status );
		}

		/*
		===============
		GetAllQuestStates
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public IReadOnlyDictionary<QuestDefinitionId, QuestStatus> GetAllQuestStates()
		{
			var result = new Dictionary<QuestDefinitionId, QuestStatus>( _activeQuests.Count );

			Span<int> active = _activeQuests.DenseIds;
			for ( int i = 0; i < active.Length; i++ ) {
				int quest = active[i];
				result[_questKeys[quest]] = (QuestStatus)_questStatuses[quest];
			}

			return result;
		}

		/*
		===============
		GetAllStorylineStates
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public IReadOnlyDictionary<StorylineDefinitionId, StorylineStatus> GetAllStorylineStates()
		{
			var result = new Dictionary<StorylineDefinitionId, StorylineStatus>( _activeStorylines.Count );

			Span<int> active = _activeStorylines.DenseIds;
			for ( int i = 0; i < active.Length; i++ ) {
				int storyline = active[i];
				result[_storylineKeys[storyline]] = (StorylineStatus)_storylineStatuses[storyline];
			}

			return result;
		}

		/*
		===============
		RegisterFacts
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="facts"></param>
		private void RegisterFacts( InternString[] facts )
		{
			for ( int i = 0; i < facts.Length; i++ ) {
				int id = _factIds.Register( facts[i] );
				_factKeys[id] = facts[i];
			}
		}

		/*
		===============
		RegisterQuests
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="quests"></param>
		private void RegisterQuests( CompiledQuestRuntimeInfo[] quests )
		{
			for ( int i = 0; i < quests.Length; i++ ) {
				int id = _questIds.Register( quests[i].DefinitionId.Value );
				_questKeys[id] = quests[i].DefinitionId;
				_questStatuses[id] = (byte)quests[i].DefaultStatus;
			}
		}

		/*
		===============
		RegisterObjectives
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="objectives"></param>
		private void RegisterObjectives( CompiledObjectiveRuntimeInfo[] objectives )
		{
			for ( int i = 0; i < objectives.Length; i++ ) {
				int id = _objectiveIds.Register( objectives[i].DefinitionId.Value );
				_objectiveKeys[id] = objectives[i].DefinitionId;
				_objectiveStatuses[id] = (byte)objectives[i].DefaultStatus;
			}
		}

		/*
		===============
		RegisterStorylines
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="storylines"></param>
		private void RegisterStorylines( CompiledStorylineRuntimeInfo[] storylines )
		{
			for ( int i = 0; i < storylines.Length; i++ ) {
				int id = _storylineIds.Register( storylines[i].DefinitionId.Value );
				_storylineKeys[id] = storylines[i].DefinitionId;
				_storylineStatuses[id] = (byte)storylines[i].DefaultStatus;
			}
		}
	};
};
