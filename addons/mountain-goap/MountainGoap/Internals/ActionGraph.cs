// <copyright file="ActionGraph.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a traversable action graph.
    /// </summary>
    internal class ActionGraph {
        /// <summary>
        /// The set of actions for the graph.
        /// </summary>
        internal List<ActionNode> ActionNodes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionGraph"/> class.
        /// </summary>
        /// <param name="actions">List of actions to include in the graph.</param>
        /// <param name="state">Initial state to be used.</param>
        internal ActionGraph(List<Action> actions, ConcurrentDictionary<string, object?> state) {
			for ( int i = 0; i < actions.Count; i++ ) {
				var permutations = actions[ i ].GetPermutations( state );
				foreach ( var permutation in permutations ) {
					ActionNodes.Add( new ActionNode( actions[ i ], state, permutation ) );
				}
			}
        }

        /// <summary>
        /// Gets the list of neighbors for a node.
        /// </summary>
        /// <param name="node">Node for which to retrieve neighbors.</param>
        /// <returns>The set of action/state combinations that can be executed after the current action/state combination.</returns>
        internal IEnumerable<ActionNode> Neighbors(ActionNode node) {
			for ( int i = 0; i < ActionNodes.Count; i++ ) {
				if ( ActionNodes[ i ].Action is not null && ActionNodes[ i ].Action.IsPossible( node.State ) ) {
					var newNode = new ActionNode( ActionNodes[ i ].Action.Copy(), node.State.Copy(), ActionNodes[ i ].Parameters.Copy() );
					newNode.Action?.ApplyEffects( newNode.State );
					yield return newNode;
				}
			}
        }
    }
}