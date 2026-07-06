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
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Nomad.Game.Sdk.Story;
using Nomad.Game.Sdk.Story.QuestTree;
using Nomad.Game.Gameplay.Runtime.Story.QuestTree;

namespace Nomad.Game.Gameplay.Story.QuestTree
{
	internal sealed class ActiveQuestTree : IDisposable
	{
		private ActiveQuestTreeNode[] _nodes;
		private ushort[] _children;
		private int[] _globalToLocal;

		private int _nodeCount;
		private int _childCount;
		private ulong _completedMask;
		private ulong _failedMask;
		private ulong _availableMask;
		private ulong _dirtyMask;
		private bool _isDisposed;

		public int NodeCount
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get => _nodeCount;
		}

		public ulong CompletedMask
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get => _completedMask;
		}

		public ulong FailedMask
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get => _failedMask;
		}

		public ulong AvailableMask
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get => _availableMask;
		}

		public ActiveQuestTree( int initialNodeCapacity = 16, int initialChildCapacity = 32 )
		{
			_nodes = ArrayPool<ActiveQuestTreeNode>.Shared.Rent( Math.Max( initialNodeCapacity, 1 ) );
			_children = ArrayPool<ushort>.Shared.Rent( Math.Max( initialChildCapacity, 1 ) );
			_globalToLocal = ArrayPool<int>.Shared.Rent( Math.Max( initialNodeCapacity, 1 ) );
		}

		public void LoadAll( CompiledQuestTree tree, IWorldStateDatabase stateDatabase )
		{
			if ( tree == null ) {
				throw new ArgumentNullException( nameof( tree ) );
			}

			EnsureGlobalMapCapacity( tree.NodeCount );
			EnsureNodeCapacity( tree.NodeCount );

			Span<int> mapping = _globalToLocal.AsSpan( 0, tree.NodeCount );
			for ( int i = 0; i < mapping.Length; i++ ) {
				mapping[i] = -1;
			}

			_nodeCount = 0;
			for ( int i = 0; i < tree.NodeCount; i++ ) {
				mapping[i] = _nodeCount;
				CopyNode( tree, i, _nodeCount );
				_nodeCount++;
			}

			BuildChildren( tree, mapping );
			RebuildMasksFromWorldState( stateDatabase );
			RebuildAvailableMask( stateDatabase );
		}

		public void LoadSubset(
			CompiledQuestTree tree,
			ReadOnlySpan<QuestDefinitionId> questIds,
			IWorldStateDatabase stateDatabase
		)
		{
			if ( tree == null ) {
				throw new ArgumentNullException( nameof( tree ) );
			}

			EnsureGlobalMapCapacity( tree.NodeCount );
			EnsureNodeCapacity( questIds.Length );

			Span<int> mapping = _globalToLocal.AsSpan( 0, tree.NodeCount );
			for ( int i = 0; i < mapping.Length; i++ ) {
				mapping[i] = -1;
			}

			_nodeCount = 0;
			for ( int i = 0; i < questIds.Length; i++ ) {
				if ( !tree.TryGetNodeIndex( questIds[i], out int globalIndex ) ) {
					continue;
				}

				if ( mapping[globalIndex] >= 0 ) {
					continue;
				}

				mapping[globalIndex] = _nodeCount;
				CopyNode( tree, globalIndex, _nodeCount );
				_nodeCount++;
			}

			BuildChildren( tree, mapping );
			RebuildMasksFromWorldState( stateDatabase );
			RebuildAvailableMask( stateDatabase );
		}

		public bool TryFindNode( QuestDefinitionId questId, out int nodeIndex )
		{
			for ( int i = 0; i < _nodeCount; i++ ) {
				if ( _nodes[i].QuestId == questId ) {
					nodeIndex = i;
					return true;
				}
			}

			nodeIndex = -1;
			return false;
		}

		public ref ActiveQuestTreeNode GetNode( int nodeIndex )
		{
			if ( (uint)nodeIndex >= (uint)_nodeCount ) {
				throw new ArgumentOutOfRangeException( nameof( nodeIndex ) );
			}

			return ref _nodes[nodeIndex];
		}

		public ReadOnlySpan<ushort> GetChildren( int nodeIndex )
		{
			if ( (uint)nodeIndex >= (uint)_nodeCount ) {
				return ReadOnlySpan<ushort>.Empty;
			}

			ref ActiveQuestTreeNode node = ref _nodes[nodeIndex];
			return _children.AsSpan( node.FirstChild, node.ChildCount );
		}

		public bool CanEnterNode( int nodeIndex, IWorldStateDatabase stateDatabase )
		{
			if ( (uint)nodeIndex >= (uint)_nodeCount ) {
				return false;
			}

			ref ActiveQuestTreeNode node = ref _nodes[nodeIndex];
			if ( ( node.TraversalFlags & QuestNodeTraversalFlags.CanEnter ) == 0 ) {
				return false;
			}

			if ( !EvaluateCondition( node.Condition, stateDatabase ) ) {
				return false;
			}

			ulong terminalMask = _completedMask | _failedMask;
			if ( ( _completedMask & node.RequiredCompletedMask ) != node.RequiredCompletedMask ) {
				return false;
			}

			if ( ( _failedMask & node.RequiredFailedMask ) != node.RequiredFailedMask ) {
				return false;
			}

			if ( ( terminalMask & node.RequiredTerminalMask ) != node.RequiredTerminalMask ) {
				return false;
			}

			return true;
		}

		public void SetQuestStatus( QuestDefinitionId questId, QuestStatus status, IWorldStateDatabase stateDatabase )
		{
			if ( !TryFindNode( questId, out int nodeIndex ) ) {
				return;
			}

			SetQuestStatusByNodeIndex( nodeIndex, status, stateDatabase );
		}

		public void SetQuestStatusByNodeIndex( int nodeIndex, QuestStatus status, IWorldStateDatabase stateDatabase )
		{
			if ( (uint)nodeIndex >= (uint)_nodeCount ) {
				return;
			}

			ulong bit = 1UL << nodeIndex;
			_completedMask &= ~bit;
			_failedMask &= ~bit;

			if ( status == QuestStatus.Completed ) {
				_completedMask |= bit;
			} else if ( status == QuestStatus.Failed ) {
				_failedMask |= bit;
			}

			_dirtyMask |= bit;
			RebuildAvailableMask( stateDatabase );
		}

		public int CopyAvailableQuests( Span<QuestDefinitionId> destination, bool includeHidden = false )
		{
			int written = 0;
			ulong mask = _availableMask;

			while ( mask != 0UL && written < destination.Length ) {
				int nodeIndex = BitOperations.TrailingZeroCount( mask );
				mask &= mask - 1UL;

				if ( !includeHidden && _nodes[nodeIndex].IsHidden ) {
					continue;
				}

				destination[written] = _nodes[nodeIndex].QuestId;
				written++;
			}

			return written;
		}

		public void Clear()
		{
			if ( RuntimeHelpers.IsReferenceOrContainsReferences<ActiveQuestTreeNode>() ) {
				Array.Clear( _nodes, 0, _nodeCount );
			}

			_nodeCount = 0;
			_childCount = 0;
			_completedMask = 0UL;
			_failedMask = 0UL;
			_availableMask = 0UL;
			_dirtyMask = 0UL;
		}

		private void CopyNode( CompiledQuestTree tree, int globalIndex, int localIndex )
		{
			ref readonly CompiledQuestTreeNode source = ref tree.GetCompiledNodeByIndex( globalIndex );
			_nodes[localIndex] = new ActiveQuestTreeNode {
				QuestId = source.QuestId,
				Type = source.Type,
				TraversalFlags = source.TraversalFlags,
				IsHidden = source.IsHidden,
				RequiredCompletedMask = 0UL,
				RequiredFailedMask = 0UL,
				RequiredTerminalMask = 0UL,
				Condition = source.Condition
			};
		}

		private void BuildChildren( CompiledQuestTree tree, Span<int> globalToLocal )
		{
			_childCount = 0;

			for ( int global = 0; global < globalToLocal.Length; global++ ) {
				int local = globalToLocal[global];
				if ( local < 0 ) {
					continue;
				}

				ReadOnlySpan<QuestTreeEdge> outgoing = tree.GetOutgoingEdgesByIndex( global );
				ref ActiveQuestTreeNode node = ref _nodes[local];
				node.FirstChild = (ushort)_childCount;
				node.ChildCount = 0;

				EnsureChildCapacity( _childCount + outgoing.Length );

				for ( int i = 0; i < outgoing.Length; i++ ) {
					QuestTreeEdge edge = outgoing[i];
					if ( !tree.TryGetNodeIndex( edge.To.Id, out int childGlobal ) ) {
						continue;
					}

					int childLocal = globalToLocal[childGlobal];
					if ( childLocal < 0 ) {
						continue;
					}

					_children[_childCount] = (ushort)childLocal;
					_childCount++;
					node.ChildCount++;

					ApplyLocalRequirement( edge, local, ref _nodes[childLocal] );
				}
			}
		}

		private static void ApplyLocalRequirement(
			QuestTreeEdge edge,
			int localParentIndex,
			ref ActiveQuestTreeNode activeChild
		)
		{
			if ( !edge.IsBlocking || edge.Kind == QuestTreeEdgeKind.Related ) {
				return;
			}

			ulong parentBit = 1UL << localParentIndex;

			switch ( edge.Kind ) {
				case QuestTreeEdgeKind.Failure:
					activeChild.RequiredFailedMask |= parentBit;
					break;

				case QuestTreeEdgeKind.Cleanup:
					activeChild.RequiredTerminalMask |= parentBit;
					break;

				default:
					activeChild.RequiredCompletedMask |= parentBit;
					break;
			}
		}

		private void RebuildMasksFromWorldState( IWorldStateDatabase stateDatabase )
		{
			_completedMask = 0UL;
			_failedMask = 0UL;
			_dirtyMask = 0UL;

			if ( stateDatabase == null ) {
				return;
			}

			for ( int i = 0; i < _nodeCount; i++ ) {
				if ( !stateDatabase.TryGetQuestState( _nodes[i].QuestId, out QuestStatus status ) ) {
					continue;
				}

				if ( status == QuestStatus.Completed ) {
					_completedMask |= 1UL << i;
				} else if ( status == QuestStatus.Failed ) {
					_failedMask |= 1UL << i;
				}
			}
		}

		private void RebuildAvailableMask( IWorldStateDatabase stateDatabase )
		{
			ulong available = 0UL;
			ulong terminal = _completedMask | _failedMask;

			for ( int i = 0; i < _nodeCount; i++ ) {
				ulong bit = 1UL << i;
				if ( ( terminal & bit ) != 0UL ) {
					continue;
				}

				if ( CanEnterNode( i, stateDatabase ) ) {
					available |= bit;
				}
			}

			_availableMask = available;
		}

		private static bool EvaluateCondition( CompiledQuestCondition condition, IWorldStateDatabase stateDatabase )
		{
			if ( !condition.HasCondition ) {
				return true;
			}

			if ( stateDatabase == null ) {
				return false;
			}

			switch ( condition.Operator ) {
				case QuestConditionOperator.Exists:
					return stateDatabase.HasFact( condition.Fact );

				case QuestConditionOperator.Equals:
					return stateDatabase.TryGetFact( condition.Fact, out bool equalValue )
						&& equalValue == condition.ExpectedValue;

				case QuestConditionOperator.NotEquals:
					return !stateDatabase.TryGetFact( condition.Fact, out bool notEqualValue )
						|| notEqualValue != condition.ExpectedValue;

				default:
					return false;
			}
		}

		private void EnsureNodeCapacity( int required )
		{
			if ( required <= _nodes.Length ) {
				return;
			}

			ActiveQuestTreeNode[] old = _nodes;
			_nodes = ArrayPool<ActiveQuestTreeNode>.Shared.Rent( NextPowerOfTwo( required ) );
			Array.Copy( old, _nodes, _nodeCount );
			ArrayPool<ActiveQuestTreeNode>.Shared.Return( old, RuntimeHelpers.IsReferenceOrContainsReferences<ActiveQuestTreeNode>() );
		}

		private void EnsureChildCapacity( int required )
		{
			if ( required <= _children.Length ) {
				return;
			}

			ushort[] old = _children;
			_children = ArrayPool<ushort>.Shared.Rent( NextPowerOfTwo( required ) );
			Array.Copy( old, _children, _childCount );
			ArrayPool<ushort>.Shared.Return( old, clearArray: false );
		}

		private void EnsureGlobalMapCapacity( int required )
		{
			if ( required <= _globalToLocal.Length ) {
				return;
			}

			int[] old = _globalToLocal;
			_globalToLocal = ArrayPool<int>.Shared.Rent( NextPowerOfTwo( required ) );
			ArrayPool<int>.Shared.Return( old, clearArray: false );
		}

		private static int NextPowerOfTwo( int value )
		{
			if ( value <= 1 ) {
				return 1;
			}

			value--;
			value |= value >> 1;
			value |= value >> 2;
			value |= value >> 4;
			value |= value >> 8;
			value |= value >> 16;
			value++;
			return value;
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_isDisposed = true;
			ArrayPool<ActiveQuestTreeNode>.Shared.Return( _nodes, RuntimeHelpers.IsReferenceOrContainsReferences<ActiveQuestTreeNode>() );
			ArrayPool<ushort>.Shared.Return( _children, clearArray: false );
			ArrayPool<int>.Shared.Return( _globalToLocal, clearArray: false );

			_nodes = Array.Empty<ActiveQuestTreeNode>();
			_children = Array.Empty<ushort>();
			_globalToLocal = Array.Empty<int>();
			_nodeCount = 0;
			_childCount = 0;
		}
	};
};
