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
using Nomad.Core.Collections;
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Gameplay.Runtime.Story.QuestTree;
using Nomad.Game.Sdk.Story;
using Nomad.Game.Sdk.Story.QuestTree;

namespace Nomad.Game.Content.Compilation.Story
{
	internal static class QuestTreeCompiler
	{
		public static CompiledQuestTree Compile( QuestTreeDefinition definition )
		{
			ArgumentGuard.ThrowIfNull( definition, nameof( definition ) );

			using PooledList<QuestNode> nodeBuffer = new PooledList<QuestNode>( Math.Max( 1, definition.Nodes.Count ), clearOnReturn: true );
			using PooledList<QuestTreeEdge> edgeBuffer = new PooledList<QuestTreeEdge>( Math.Max( 1, definition.Edges.Count ), clearOnReturn: false );
			using PooledList<QuestDefinitionId> entryBuffer = new PooledList<QuestDefinitionId>( Math.Max( 1, definition.EntryQuests.Count ), clearOnReturn: false );

			Dictionary<QuestDefinitionId, int> buildIndices = new Dictionary<QuestDefinitionId, int>( Math.Max( 1, definition.Nodes.Count ) );

			BuildNodes( definition, nodeBuffer, edgeBuffer, buildIndices );
			BuildEntries( definition, nodeBuffer.Span, edgeBuffer.Span, buildIndices, entryBuffer );

			QuestNode[] nodes = nodeBuffer.Span.ToArray();
			QuestTreeEdge[] edges = edgeBuffer.Span.ToArray();
			QuestDefinitionId[] entries = entryBuffer.Span.ToArray();

			QuestTreeNodeIndex index = BuildRuntimeIndex( nodes );
			CompiledQuestCondition[] conditions = BuildConditions( definition, nodes.Length, index );

			BuildEdgeRanges(
				nodes.Length,
				edges,
				index,
				out int[] outgoingOffsets,
				out int[] incomingOffsets,
				out QuestTreeEdge[] outgoingEdges,
				out QuestTreeEdge[] incomingEdges,
				out CompactGraph outgoingGraph,
				out CompactGraph incomingGraph
			);

			BuildRequirementMasks(
				nodes.Length,
				incomingOffsets,
				incomingEdges,
				index,
				out ulong[] requiredCompletedMasks,
				out ulong[] requiredFailedMasks,
				out ulong[] requiredTerminalMasks
			);

			CompiledQuestTreeNode[] compiledNodes = new CompiledQuestTreeNode[nodes.Length];
			for ( int i = 0; i < nodes.Length; i++ ) {
				compiledNodes[i] = new CompiledQuestTreeNode(
					nodes[i],
					requiredCompletedMasks[i],
					requiredFailedMasks[i],
					requiredTerminalMasks[i],
					conditions[i]
				);
			}

			QuestDefinitionId[] topologicalOrder = BuildTopologicalOrder(
				nodes,
				outgoingGraph,
				incomingGraph,
				out bool hasCycles
			);

			ulong[] reachable = BuildReachability( nodes.Length, outgoingGraph, out int reachabilityWords );

			return new CompiledQuestTree(
				definition,
				compiledNodes,
				edges,
				entries,
				topologicalOrder,
				index,
				outgoingOffsets,
				incomingOffsets,
				outgoingEdges,
				incomingEdges,
				outgoingGraph,
				incomingGraph,
				reachable,
				reachabilityWords,
				hasCycles
			);
		}

		public static CompiledQuestTree FromStoryline( StorylineDefinition storylineDefinition )
		{
			if ( storylineDefinition == null ) {
				throw new ArgumentNullException( nameof( storylineDefinition ) );
			}

			return Compile( CreateDefinition( storylineDefinition ) );
		}

		private static QuestTreeDefinition CreateDefinition( StorylineDefinition storylineDefinition )
		{
			QuestTreeDefinition? authoredTree = storylineDefinition.QuestTree;
			Dictionary<QuestDefinitionId, QuestNode> nodeMap = new Dictionary<QuestDefinitionId, QuestNode>(
				storylineDefinition.Quests.Count + (authoredTree?.Nodes.Count ?? 0)
			);

			for ( int i = 0; i < storylineDefinition.Quests.Count; i++ ) {
				QuestDefinition quest = storylineDefinition.Quests[i];
				if ( !quest.Id.IsValid || nodeMap.ContainsKey( quest.Id ) ) {
					continue;
				}

				nodeMap.Add(
					quest.Id,
					new QuestNode {
						QuestId = quest.Id,
						Type = i == 0 ? QuestNodeType.Entry : QuestNodeType.Objective,
						IsHidden = false
					}
				);
			}

			if ( authoredTree != null ) {
				for ( int i = 0; i < authoredTree.Nodes.Count; i++ ) {
					QuestNode node = authoredTree.Nodes[i];
					if ( !node.QuestId.IsValid ) {
						continue;
					}

					nodeMap[node.QuestId] = node;
				}
			}

			QuestNode[] nodes = new QuestNode[nodeMap.Count];
			nodeMap.Values.CopyTo( nodes, 0 );

			IReadOnlyList<QuestTreeEdge> edges = LinkEdges( authoredTree, storylineDefinition );
			IReadOnlyList<QuestDefinitionId> entries = GetEntryQuests( authoredTree, storylineDefinition );

			return new QuestTreeDefinition {
				StorylineId = storylineDefinition.Id.IsValid
					? storylineDefinition.Id
					: authoredTree?.StorylineId ?? StorylineDefinitionId.Invalid,
				Nodes = nodes,
				Edges = edges,
				Conditions = authoredTree?.Conditions ?? Array.Empty<QuestConditionNode>(),
				EntryQuests = entries
			};
		}

		private static IReadOnlyList<QuestDefinitionId> GetEntryQuests( QuestTreeDefinition? authoredTree, StorylineDefinition storyline )
		{
			IReadOnlyList<QuestDefinitionId> entries = authoredTree?.EntryQuests ?? Array.Empty<QuestDefinitionId>();

			if ( entries.Count > 0 || storyline.Quests.Count == 0 ) {
				return entries;
			}

			QuestDefinitionId entry = storyline.Quests[0].Id;
			entries = entry.IsValid ? new[] { entry } : Array.Empty<QuestDefinitionId>();

			return entries;
		}

		private static IReadOnlyList<QuestTreeEdge> LinkEdges( QuestTreeDefinition? authoredTree, StorylineDefinition storyline )
		{
			IReadOnlyList<QuestTreeEdge> edges = authoredTree?.Edges ?? Array.Empty<QuestTreeEdge>();

			if ( edges.Count > 0 || storyline.Quests.Count < 1 ) {
				return edges;
			}

			QuestTreeEdge[] linearEdges = new QuestTreeEdge[storyline.Quests.Count - 1];
			int count = 0;

			for ( int i = 1; i < storyline.Quests.Count; i++ ) {
				QuestDefinition from = storyline.Quests[i - 1];
				QuestDefinition to = storyline.Quests[i];

				if ( !from.Id.IsValid || !to.Id.IsValid || from == to ) {
					continue;
				}

				linearEdges[count] = new QuestTreeEdge( from, to );
				count++;
			}

			if ( count == linearEdges.Length ) {
				edges = linearEdges;
			} else {
				QuestTreeEdge[] trimmed = new QuestTreeEdge[count];
				Array.Copy( linearEdges, trimmed, count );
				edges = trimmed;
			}

			return edges;
		}

		private static void BuildNodes(
			QuestTreeDefinition definition,
			PooledList<QuestNode> nodes,
			PooledList<QuestTreeEdge> edges,
			Dictionary<QuestDefinitionId, int> buildIndices
		)
		{
			for ( int i = 0; i < definition.Nodes.Count; i++ ) {
				QuestNode node = definition.Nodes[i];
				EnsureNode( node.QuestId, node.Type, node.IsHidden, buildIndices, nodes );
			}

			for ( int i = 0; i < definition.Edges.Count; i++ ) {
				QuestTreeEdge edge = definition.Edges[i];
				if ( !edge.IsValid ) {
					continue;
				}

				EnsureNode( edge.From.Id, QuestNodeType.Objective, false, buildIndices, nodes );
				EnsureNode( edge.To.Id, QuestNodeType.Objective, false, buildIndices, nodes );
				edges.Add( edge );
			}

			for ( int i = 0; i < definition.Conditions.Count; i++ ) {
				QuestConditionNode condition = definition.Conditions[i];
				if ( condition.QuestId.IsValid ) {
					EnsureNode( condition.QuestId, QuestNodeType.Condition, false, buildIndices, nodes );
				}
			}
		}

		private static void EnsureNode(
			QuestDefinitionId questId,
			QuestNodeType fallbackType,
			bool hidden,
			Dictionary<QuestDefinitionId, int> buildIndices,
			PooledList<QuestNode> nodes
		)
		{
			if ( !questId.IsValid || buildIndices.ContainsKey( questId ) ) {
				return;
			}

			buildIndices.Add( questId, nodes.Count );
			nodes.Add( new QuestNode {
				QuestId = questId,
				Type = fallbackType,
				IsHidden = hidden
			} );
		}

		private static void BuildEntries(
			QuestTreeDefinition definition,
			ReadOnlySpan<QuestNode> nodes,
			ReadOnlySpan<QuestTreeEdge> edges,
			Dictionary<QuestDefinitionId, int> buildIndices,
			PooledList<QuestDefinitionId> entries
		)
		{
			for ( int i = 0; i < definition.EntryQuests.Count; i++ ) {
				QuestDefinitionId questId = definition.EntryQuests[i];
				if ( questId.IsValid && buildIndices.ContainsKey( questId ) ) {
					entries.Add( questId );
				}
			}

			if ( entries.Count != 0 ) {
				return;
			}

			Span<bool> hasIncoming = nodes.Length <= 256
				? stackalloc bool[nodes.Length]
				: new bool[nodes.Length];

			for ( int i = 0; i < edges.Length; i++ ) {
				QuestTreeEdge edge = edges[i];
				if ( buildIndices.TryGetValue( edge.To.Id, out int toIndex ) ) {
					hasIncoming[toIndex] = true;
				}
			}

			for ( int i = 0; i < nodes.Length; i++ ) {
				if ( !hasIncoming[i] ) {
					entries.Add( nodes[i].QuestId );
				}
			}

			if ( entries.Count == 0 && nodes.Length > 0 ) {
				entries.Add( nodes[0].QuestId );
			}
		}

		private static QuestTreeNodeIndex BuildRuntimeIndex( ReadOnlySpan<QuestNode> nodes )
		{
			QuestTreeNodeIndex index = new QuestTreeNodeIndex( nodes.Length + 1 );
			for ( int i = 0; i < nodes.Length; i++ ) {
				index.Register( nodes[i].QuestId, i );
			}

			return index;
		}

		private static CompiledQuestCondition[] BuildConditions(
			QuestTreeDefinition definition,
			int nodeCount,
			QuestTreeNodeIndex index
		)
		{
			CompiledQuestCondition[] conditions = new CompiledQuestCondition[nodeCount];

			for ( int i = 0; i < definition.Conditions.Count; i++ ) {
				QuestConditionNode condition = definition.Conditions[i];
				if ( !index.TryResolve( condition.QuestId, out int nodeIndex ) ) {
					continue;
				}

				conditions[nodeIndex] = new CompiledQuestCondition(
					condition.Fact,
					condition.Operator,
					condition.ExpectedValue
				);
			}

			return conditions;
		}

		private static void BuildEdgeRanges(
			int nodeCount,
			QuestTreeEdge[] edges,
			QuestTreeNodeIndex index,
			out int[] outgoingOffsets,
			out int[] incomingOffsets,
			out QuestTreeEdge[] outgoingEdges,
			out QuestTreeEdge[] incomingEdges,
			out CompactGraph outgoingGraph,
			out CompactGraph incomingGraph
		)
		{
			int[] outgoingCounts = new int[nodeCount];
			int[] incomingCounts = new int[nodeCount];
			int validEdgeCount = 0;

			for ( int i = 0; i < edges.Length; i++ ) {
				QuestTreeEdge edge = edges[i];
				if ( !index.TryResolve( edge.From.Id, out int from ) || !index.TryResolve( edge.To.Id, out int to ) ) {
					continue;
				}

				outgoingCounts[from]++;
				incomingCounts[to]++;
				validEdgeCount++;
			}

			outgoingOffsets = BuildOffsets( outgoingCounts );
			incomingOffsets = BuildOffsets( incomingCounts );

			outgoingEdges = new QuestTreeEdge[validEdgeCount];
			incomingEdges = new QuestTreeEdge[validEdgeCount];
			int[] outgoingWrite = new int[nodeCount];
			int[] incomingWrite = new int[nodeCount];

			CompactGraphBuilder outgoingBuilder = new CompactGraphBuilder( nodeCount );
			CompactGraphBuilder incomingBuilder = new CompactGraphBuilder( nodeCount );

			for ( int i = 0; i < edges.Length; i++ ) {
				QuestTreeEdge edge = edges[i];
				if ( !index.TryResolve( edge.From.Id, out int from ) || !index.TryResolve( edge.To.Id, out int to ) ) {
					continue;
				}

				int outgoingIndex = outgoingOffsets[from] + outgoingWrite[from];
				int incomingIndex = incomingOffsets[to] + incomingWrite[to];

				outgoingEdges[outgoingIndex] = edge;
				incomingEdges[incomingIndex] = edge;
				outgoingWrite[from]++;
				incomingWrite[to]++;

				outgoingBuilder.AddEdge( from, to );
				incomingBuilder.AddEdge( to, from );
			}

			outgoingGraph = outgoingBuilder.Build();
			incomingGraph = incomingBuilder.Build();
		}

		private static int[] BuildOffsets( int[] counts )
		{
			int[] offsets = new int[counts.Length + 1];
			int total = 0;

			for ( int i = 0; i < counts.Length; i++ ) {
				offsets[i] = total;
				total += counts[i];
			}

			offsets[counts.Length] = total;
			return offsets;
		}

		private static void BuildRequirementMasks(
			int nodeCount,
			int[] incomingOffsets,
			QuestTreeEdge[] incomingEdges,
			QuestTreeNodeIndex index,
			out ulong[] requiredCompletedMasks,
			out ulong[] requiredFailedMasks,
			out ulong[] requiredTerminalMasks
		)
		{
			requiredCompletedMasks = new ulong[nodeCount];
			requiredFailedMasks = new ulong[nodeCount];
			requiredTerminalMasks = new ulong[nodeCount];

			if ( nodeCount > 64 ) {
				return;
			}

			for ( int to = 0; to < nodeCount; to++ ) {
				int start = incomingOffsets[to];
				int end = incomingOffsets[to + 1];

				for ( int i = start; i < end; i++ ) {
					QuestTreeEdge edge = incomingEdges[i];
					if ( !edge.IsBlocking || edge.Kind == QuestTreeEdgeKind.Related ) {
						continue;
					}

					if ( !index.TryResolve( edge.From.Id, out int from ) ) {
						continue;
					}

					ulong fromMask = 1UL << from;
					switch ( edge.Kind ) {
						case QuestTreeEdgeKind.Failure:
							requiredFailedMasks[to] |= fromMask;
							break;

						case QuestTreeEdgeKind.Cleanup:
							requiredTerminalMasks[to] |= fromMask;
							break;

						default:
							requiredCompletedMasks[to] |= fromMask;
							break;
					}
				}
			}
		}

		private static QuestDefinitionId[] BuildTopologicalOrder(
			QuestNode[] nodes,
			CompactGraph outgoingGraph,
			CompactGraph incomingGraph,
			out bool hasCycles
		)
		{
			int nodeCount = nodes.Length;
			int[] indegree = new int[nodeCount];
			Queue<int> queue = new Queue<int>( nodeCount );
			QuestDefinitionId[] order = new QuestDefinitionId[nodeCount];
			int orderCount = 0;

			for ( int i = 0; i < nodeCount; i++ ) {
				indegree[i] = incomingGraph.GetNeighbors( i ).Length;
				if ( indegree[i] == 0 ) {
					queue.Enqueue( i );
				}
			}

			while ( queue.Count != 0 ) {
				int node = queue.Dequeue();
				order[orderCount] = nodes[node].QuestId;
				orderCount++;

				ReadOnlySpan<int> children = outgoingGraph.GetNeighbors( node );
				for ( int i = 0; i < children.Length; i++ ) {
					int child = children[i];
					indegree[child]--;
					if ( indegree[child] == 0 ) {
						queue.Enqueue( child );
					}
				}
			}

			hasCycles = orderCount != nodeCount;
			if ( orderCount == order.Length ) {
				return order;
			}

			QuestDefinitionId[] result = new QuestDefinitionId[nodeCount];
			Array.Copy( order, result, orderCount );

			for ( int i = 0; i < nodeCount; i++ ) {
				if ( indegree[i] > 0 ) {
					result[orderCount] = nodes[i].QuestId;
					orderCount++;
				}
			}

			return result;
		}

		private static ulong[] BuildReachability( int nodeCount, CompactGraph graph, out int wordCount )
		{
			wordCount = Math.Max( 1, (nodeCount + 63) >> 6 );
			ulong[] reachable = new ulong[Math.Max( 1, nodeCount * wordCount )];
			int[] visitMarks = new int[Math.Max( 1, nodeCount )];
			int[] stack = new int[Math.Max( 1, nodeCount )];
			int visitVersion = 0;

			for ( int start = 0; start < nodeCount; start++ ) {
				visitVersion++;
				int stackCount = 0;
				stack[stackCount] = start;
				stackCount++;
				visitMarks[start] = visitVersion;

				while ( stackCount > 0 ) {
					stackCount--;
					int node = stack[stackCount];
					ReadOnlySpan<int> children = graph.GetNeighbors( node );

					for ( int i = 0; i < children.Length; i++ ) {
						int child = children[i];
						if ( visitMarks[child] == visitVersion ) {
							continue;
						}

						visitMarks[child] = visitVersion;
						reachable[(start * wordCount) + (child >> 6)] |= 1UL << (child & 63);
						stack[stackCount] = child;
						stackCount++;
					}
				}
			}

			return reachable;
		}
	};
};
