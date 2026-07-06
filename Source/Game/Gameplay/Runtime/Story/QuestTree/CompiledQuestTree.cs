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
using Nomad.Core.Collections;
using Nomad.Game.Sdk.Story;
using Nomad.Game.Sdk.Story.QuestTree;

namespace Nomad.Game.Gameplay.Runtime.Story.QuestTree
{
	internal sealed class CompiledQuestTree : IQuestTree
	{
		public QuestTreeDefinition Definition { get; }
		public int NodeCount => _nodes.Length;
		public int EdgeCount => _edges.Length;
		public bool HasCycles { get; }

		public ReadOnlySpan<QuestNode> Nodes => _publicNodes;
		public ReadOnlySpan<QuestTreeEdge> Edges => _edges;
		public ReadOnlySpan<QuestDefinitionId> EntryQuests => _entryQuests;
		public ReadOnlySpan<QuestDefinitionId> TopologicalOrder => _topologicalOrder;

		private readonly CompiledQuestTreeNode[] _nodes;
		private readonly QuestNode[] _publicNodes;
		private readonly QuestTreeEdge[] _edges;
		private readonly QuestDefinitionId[] _entryQuests;
		private readonly QuestDefinitionId[] _topologicalOrder;
		private readonly QuestTreeNodeIndex _index;

		private readonly int[] _outgoingEdgeOffsets;
		private readonly int[] _incomingEdgeOffsets;
		private readonly QuestTreeEdge[] _outgoingEdges;
		private readonly QuestTreeEdge[] _incomingEdges;
		private readonly CompactGraph _outgoingGraph;
		private readonly CompactGraph _incomingGraph;

		private readonly ulong[] _reachable;
		private readonly int _reachabilityWords;

		internal CompiledQuestTree(
			QuestTreeDefinition definition,
			CompiledQuestTreeNode[] nodes,
			QuestTreeEdge[] edges,
			QuestDefinitionId[] entryQuests,
			QuestDefinitionId[] topologicalOrder,
			QuestTreeNodeIndex index,
			int[] outgoingEdgeOffsets,
			int[] incomingEdgeOffsets,
			QuestTreeEdge[] outgoingEdges,
			QuestTreeEdge[] incomingEdges,
			CompactGraph outgoingGraph,
			CompactGraph incomingGraph,
			ulong[] reachable,
			int reachabilityWords,
			bool hasCycles
		)
		{
			Definition = definition;
			_nodes = nodes;
			_publicNodes = new QuestNode[nodes.Length];
			_edges = edges;
			_entryQuests = entryQuests;
			_topologicalOrder = topologicalOrder;
			_index = index;
			_outgoingEdgeOffsets = outgoingEdgeOffsets;
			_incomingEdgeOffsets = incomingEdgeOffsets;
			_outgoingEdges = outgoingEdges;
			_incomingEdges = incomingEdges;
			_outgoingGraph = outgoingGraph;
			_incomingGraph = incomingGraph;
			_reachable = reachable;
			_reachabilityWords = reachabilityWords;
			HasCycles = hasCycles;

			for ( int i = 0; i < nodes.Length; i++ ) {
				_publicNodes[i] = nodes[i].Source;
			}
		}


		public bool ContainsQuest( QuestDefinitionId questId )
		{
			return _index.TryResolve( questId, out _ );
		}

		public bool TryGetNode( QuestDefinitionId questId, out QuestNode node )
		{
			if ( !_index.TryResolve( questId, out int index ) ) {
				node = default!;
				return false;
			}

			node = _nodes[index].Source;
			return true;
		}

		public bool TryGetNodeTypeInfo( QuestDefinitionId questId, out QuestNodeTypeInfo typeInfo )
		{
			if ( !_index.TryResolve( questId, out int index ) ) {
				typeInfo = default;
				return false;
			}

			typeInfo = _nodes[index].Type.GetInfo();
			return true;
		}

		public QuestRelationship GetRelationship( QuestDefinitionId questId, QuestDefinitionId otherQuestId )
		{
			if ( questId == otherQuestId ) {
				return QuestRelationship.IsSame;
			}

			if ( !_index.TryResolve( questId, out int left ) || !_index.TryResolve( otherQuestId, out int right ) ) {
				return QuestRelationship.None;
			}

			if ( HasDirectEdge( left, right ) ) {
				return QuestRelationship.Parent;
			}

			if ( HasDirectEdge( right, left ) ) {
				return QuestRelationship.Child;
			}

			if ( ShareDirectParent( left, right ) ) {
				return QuestRelationship.Sibling;
			}

			if ( IsReachableByIndex( left, right ) ) {
				return QuestRelationship.Parent;
			}

			if ( IsReachableByIndex( right, left ) ) {
				return QuestRelationship.Child;
			}

			return QuestRelationship.Storyline;
		}

		public bool IsReachable( QuestDefinitionId fromQuestId, QuestDefinitionId toQuestId )
		{
			return _index.TryResolve( fromQuestId, out int fromIndex )
				&& _index.TryResolve( toQuestId, out int toIndex )
				&& IsReachableByIndex( fromIndex, toIndex );
		}

		public bool CanEnterNode( QuestDefinitionId questId, IWorldStateDatabase stateDatabase )
		{
			if ( stateDatabase == null || !_index.TryResolve( questId, out int nodeIndex ) ) {
				return false;
			}

			return CanEnterNodeByIndex( nodeIndex, stateDatabase );
		}

		public bool CanExitNode( QuestDefinitionId questId, IWorldStateDatabase stateDatabase )
		{
			if ( stateDatabase == null || !_index.TryResolve( questId, out int nodeIndex ) ) {
				return false;
			}

			return CanExitNodeByIndex( nodeIndex, stateDatabase );
		}

		public ReadOnlySpan<QuestTreeEdge> GetOutgoingEdges( QuestDefinitionId questId )
		{
			if ( !_index.TryResolve( questId, out int index ) ) {
				return ReadOnlySpan<QuestTreeEdge>.Empty;
			}

			return GetOutgoingEdgesByIndex( index );
		}

		public ReadOnlySpan<QuestTreeEdge> GetIncomingEdges( QuestDefinitionId questId )
		{
			if ( !_index.TryResolve( questId, out int index ) ) {
				return ReadOnlySpan<QuestTreeEdge>.Empty;
			}

			return GetIncomingEdgesByIndex( index );
		}

		public int CopyChildren( QuestDefinitionId questId, Span<QuestDefinitionId> destination )
		{
			if ( !_index.TryResolve( questId, out int index ) ) {
				return 0;
			}

			ReadOnlySpan<int> children = _outgoingGraph.GetNeighbors( index );
			int count = Math.Min( children.Length, destination.Length );
			for ( int i = 0; i < count; i++ ) {
				destination[i] = _nodes[children[i]].QuestId;
			}

			return count;
		}

		public int CopyParents( QuestDefinitionId questId, Span<QuestDefinitionId> destination )
		{
			if ( !_index.TryResolve( questId, out int index ) ) {
				return 0;
			}

			ReadOnlySpan<int> parents = _incomingGraph.GetNeighbors( index );
			int count = Math.Min( parents.Length, destination.Length );
			for ( int i = 0; i < count; i++ ) {
				destination[i] = _nodes[parents[i]].QuestId;
			}

			return count;
		}

		public int CopyTraversableChildren(
			QuestDefinitionId questId,
			IWorldStateDatabase stateDatabase,
			Span<QuestDefinitionId> destination,
			bool includeHidden = false
		)
		{
			if ( stateDatabase == null || destination.Length == 0 || !_index.TryResolve( questId, out int index ) ) {
				return 0;
			}

			if ( !CanExitNodeByIndex( index, stateDatabase ) ) {
				return 0;
			}

			int count = 0;
			ReadOnlySpan<QuestTreeEdge> outgoing = GetOutgoingEdgesByIndex( index );
			for ( int i = 0; i < outgoing.Length && count < destination.Length; i++ ) {
				QuestTreeEdge edge = outgoing[i];
				if ( !includeHidden && (edge.Flags & QuestTreeEdgeFlags.Hidden) != 0 ) {
					continue;
				}

				if ( !_index.TryResolve( edge.To.Id, out int childIndex ) ) {
					continue;
				}

				if ( !includeHidden && _nodes[childIndex].IsHidden ) {
					continue;
				}

				if ( IsEdgeTraversable( edge, stateDatabase ) && CanEnterNodeByIndex( childIndex, stateDatabase ) ) {
					destination[count] = edge.To.Id;
					count++;
				}
			}

			return count;
		}

		public int CopyTraversableParents(
			QuestDefinitionId questId,
			IWorldStateDatabase stateDatabase,
			Span<QuestDefinitionId> destination,
			bool includeHidden = false
		)
		{
			if ( stateDatabase == null || destination.Length == 0 || !_index.TryResolve( questId, out int index ) ) {
				return 0;
			}

			int count = 0;
			ReadOnlySpan<QuestTreeEdge> incoming = GetIncomingEdgesByIndex( index );
			for ( int i = 0; i < incoming.Length && count < destination.Length; i++ ) {
				QuestTreeEdge edge = incoming[i];
				if ( !includeHidden && (edge.Flags & QuestTreeEdgeFlags.Hidden) != 0 ) {
					continue;
				}

				if ( !_index.TryResolve( edge.From.Id, out int parentIndex ) ) {
					continue;
				}

				if ( !includeHidden && _nodes[parentIndex].IsHidden ) {
					continue;
				}

				if ( IsEdgeTraversable( edge, stateDatabase ) && CanExitNodeByIndex( parentIndex, stateDatabase ) ) {
					destination[count] = edge.From.Id;
					count++;
				}
			}

			return count;
		}

		public int CopyAvailableQuests( IWorldStateDatabase stateDatabase, Span<QuestDefinitionId> destination )
		{
			if ( stateDatabase == null || destination.Length == 0 ) {
				return 0;
			}

			ReadOnlySpan<QuestDefinitionId> order = _topologicalOrder.Length == 0 ? _entryQuests : _topologicalOrder;
			int count = 0;

			for ( int i = 0; i < order.Length && count < destination.Length; i++ ) {
				if ( CanStartQuest( order[i], stateDatabase ) ) {
					destination[count] = order[i];
					count++;
				}
			}

			return count;
		}

		public bool CanStartQuest( QuestDefinitionId questId, IWorldStateDatabase stateDatabase )
		{
			if ( stateDatabase == null || !_index.TryResolve( questId, out int index ) ) {
				return false;
			}

			if ( !CanEnterNodeByIndex( index, stateDatabase ) ) {
				return false;
			}

			if ( stateDatabase.TryGetQuestState( questId, out QuestStatus status ) ) {
				if ( status == QuestStatus.Current || status == QuestStatus.Completed || status == QuestStatus.Failed ) {
					return false;
				}
			}

			return true;
		}

		internal bool TryGetNodeIndex( QuestDefinitionId questId, out int index )
		{
			return _index.TryResolve( questId, out index );
		}

		internal ref readonly CompiledQuestTreeNode GetCompiledNodeByIndex( int index )
		{
			return ref _nodes[index];
		}

		internal ReadOnlySpan<int> GetOutgoingNeighborIndicesByIndex( int index )
		{
			return _outgoingGraph.GetNeighbors( index );
		}

		internal ReadOnlySpan<QuestTreeEdge> GetOutgoingEdgesByIndex( int index )
		{
			int start = _outgoingEdgeOffsets[index];
			return _outgoingEdges.AsSpan( start, _outgoingEdgeOffsets[index + 1] - start );
		}

		internal ReadOnlySpan<QuestTreeEdge> GetIncomingEdgesByIndex( int index )
		{
			int start = _incomingEdgeOffsets[index];
			return _incomingEdges.AsSpan( start, _incomingEdgeOffsets[index + 1] - start );
		}

		internal bool CanEnterNodeByIndex( int nodeIndex, IWorldStateDatabase stateDatabase )
		{
			ref readonly CompiledQuestTreeNode node = ref _nodes[nodeIndex];
			if ( (node.TraversalFlags & QuestNodeTraversalFlags.CanEnter) == 0 ) {
				return false;
			}

			if ( !EvaluateCondition( node, stateDatabase ) ) {
				return false;
			}

			if ( !AreRequirementMasksMet( node, stateDatabase ) ) {
				return false;
			}

			return true;
		}

		internal bool CanExitNodeByIndex( int nodeIndex, IWorldStateDatabase stateDatabase )
		{
			ref readonly CompiledQuestTreeNode node = ref _nodes[nodeIndex];
			if ( (node.TraversalFlags & QuestNodeTraversalFlags.CanExit) == 0 ) {
				return false;
			}

			if ( node.Type == QuestNodeType.Condition ) {
				return EvaluateCondition( node, stateDatabase );
			}

			if ( (node.TraversalFlags & QuestNodeTraversalFlags.RequiresQuestCompletionToExit) != 0 ) {
				return stateDatabase.TryGetQuestState( node.QuestId, out QuestStatus status )
					&& status == QuestStatus.Completed;
			}

			if ( (node.TraversalFlags & QuestNodeTraversalFlags.RequiresQuestFailureToExit) != 0 ) {
				return stateDatabase.TryGetQuestState( node.QuestId, out QuestStatus status )
					&& status == QuestStatus.Failed;
			}

			return true;
		}

		private bool AreRequirementMasksMet( in CompiledQuestTreeNode node, IWorldStateDatabase stateDatabase )
		{
			if ( _nodes.Length <= 64 ) {
				BuildStatusMasks( stateDatabase, out ulong completed, out ulong failed );

				ulong terminal = completed | failed;
				if ( (completed & node.RequiredCompletedMask) != node.RequiredCompletedMask ) {
					return false;
				}

				if ( (failed & node.RequiredFailedMask) != node.RequiredFailedMask ) {
					return false;
				}

				if ( (terminal & node.RequiredTerminalMask) != node.RequiredTerminalMask ) {
					return false;
				}

				return true;
			}

			if ( !_index.TryResolve( node.QuestId, out int nodeIndex ) ) {
				return false;
			}

			ReadOnlySpan<QuestTreeEdge> incoming = GetIncomingEdgesByIndex( nodeIndex );
			for ( int i = 0; i < incoming.Length; i++ ) {
				QuestTreeEdge edge = incoming[i];
				if ( !edge.IsBlocking || edge.Kind == QuestTreeEdgeKind.Related ) {
					continue;
				}

				if ( !IsEdgeTraversable( edge, stateDatabase ) ) {
					return false;
				}
			}

			return true;
		}

		private void BuildStatusMasks( IWorldStateDatabase stateDatabase, out ulong completed, out ulong failed )
		{
			completed = 0UL;
			failed = 0UL;

			for ( int i = 0; i < _nodes.Length && i < 64; i++ ) {
				if ( !stateDatabase.TryGetQuestState( _nodes[i].QuestId, out QuestStatus status ) ) {
					continue;
				}

				if ( status == QuestStatus.Completed ) {
					completed |= 1UL << i;
				} else if ( status == QuestStatus.Failed ) {
					failed |= 1UL << i;
				}
			}
		}

		private static bool EvaluateCondition( in CompiledQuestTreeNode node, IWorldStateDatabase stateDatabase )
		{
			CompiledQuestCondition condition = node.Condition;
			if ( !condition.HasCondition ) {
				return true;
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

		private bool IsEdgeTraversable( QuestTreeEdge edge, IWorldStateDatabase stateDatabase )
		{
			if ( !edge.IsValid ) {
				return false;
			}

			if ( !edge.IsBlocking || edge.Kind == QuestTreeEdgeKind.Related ) {
				return true;
			}

			switch ( edge.Kind ) {
				case QuestTreeEdgeKind.Failure:
					return stateDatabase.TryGetQuestState( edge.From.Id, out QuestStatus failedStatus )
						&& failedStatus == QuestStatus.Failed;

				case QuestTreeEdgeKind.Cleanup:
					return stateDatabase.TryGetQuestState( edge.From.Id, out QuestStatus terminalStatus )
						&& (terminalStatus == QuestStatus.Completed || terminalStatus == QuestStatus.Failed);

				default:
					return stateDatabase.TryGetQuestState( edge.From.Id, out QuestStatus completedStatus )
						&& completedStatus == QuestStatus.Completed;
			}
		}

		private bool HasDirectEdge( int fromIndex, int toIndex )
		{
			ReadOnlySpan<int> neighbors = _outgoingGraph.GetNeighbors( fromIndex );
			for ( int i = 0; i < neighbors.Length; i++ ) {
				if ( neighbors[i] == toIndex ) {
					return true;
				}
			}

			return false;
		}

		private bool ShareDirectParent( int left, int right )
		{
			ReadOnlySpan<int> parents = _incomingGraph.GetNeighbors( left );
			for ( int i = 0; i < parents.Length; i++ ) {
				if ( HasDirectEdge( parents[i], right ) ) {
					return true;
				}
			}

			return false;
		}

		private bool IsReachableByIndex( int fromIndex, int toIndex )
		{
			int word = toIndex >> 6;
			ulong mask = 1UL << (toIndex & 63);
			return (_reachable[(fromIndex * _reachabilityWords) + word] & mask) != 0UL;
		}
	};
};
