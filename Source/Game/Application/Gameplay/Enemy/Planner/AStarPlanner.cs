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
using Nomad.Game.Application.Gameplay.Enemy.Planner.Goals;
using Nomad.Game.Application.Gameplay.Enemy.Planner;

namespace Nomad.Game.Application.Gameplay.Enemy.Planner
{
	public sealed class AStarPlanner
	{
		public static bool TryPlan( WorldState start, GoalDef goal, PlanningContext context, PlannerAction[] actions, int maxExpansions, out Plan plan )
		{
			if ( goal == null ) {
				throw new ArgumentNullException( nameof( goal ) );
			}
			if ( actions == null ) {
				throw new ArgumentNullException( nameof( actions ) );
			}

			if ( goal.IsSatisfied( start ) ) {
				plan = Plan.Empty;
				return true;
			}

			// FIXME: mayhaps use a Span<int> with actions.Length + maxExpansions length?
			List<PlannerNode> nodes = new List<PlannerNode>( 64 );
			List<int> open = new List<int>( 64 );
			Dictionary<WorldState, int> bestG = new Dictionary<WorldState, int>();

			PlannerNode startNode = new PlannerNode {
				State = start,
				ParentIndex = -1,
				ActionIndex = -1,
				G = 0,
				H = Heuristic( start, goal )
			};

			nodes.Add( startNode );
			open.Add( 0 );
			bestG[start] = 0;

			int expansions = 0;

			while ( open.Count > 0 && expansions < maxExpansions ) {
				int openListIndex = FindLowestFIndex( open, nodes );
				int currentNodeIndex = open[openListIndex];
				open.RemoveAt( openListIndex );

				PlannerNode current = nodes[currentNodeIndex];

				if ( goal.IsSatisfied( current.State ) ) {
					plan = ReconstructPlan( nodes, currentNodeIndex, actions );
					return true;
				}

				expansions++;

				for ( int i = 0; i < actions.Length; i++ ) {
					PlannerAction action = actions[i];

					if ( !current.State.Meets( action.Preconditions ) ) {
						continue;
					}

					if ( action.ValidateContext != null && !action.ValidateContext( context ) ) {
						continue;
					}

					WorldState nextState = current.State.Apply( action.Effects );

					int stepCost = action.BaseCost;
					if ( action.GetDynamicCost != null ) {
						stepCost += action.GetDynamicCost( context );
					}

					int nextG = current.G + stepCost;

					if ( bestG.TryGetValue( nextState, out int knownG ) && nextG >= knownG ) {
						continue;
					}

					bestG[nextState] = nextG;

					PlannerNode nextNode = new PlannerNode {
						State = nextState,
						ParentIndex = currentNodeIndex,
						ActionIndex = i,
						G = nextG,
						H = Heuristic( nextState, goal )
					};

					nodes.Add( nextNode );
					open.Add( nodes.Count - 1 );
				}
			}

			plan = Plan.Empty;
			return false;
		}

		private static int Heuristic( WorldState state, GoalDef goal )
		{
			return state.CountUnmet( goal.DesiredState );
		}

		private static int FindLowestFIndex( List<int> open, List<PlannerNode> nodes )
		{
			int bestOpenIndex = 0;
			int bestNodeIndex = open[0];
			int bestF = nodes[bestNodeIndex].F;
			int bestH = nodes[bestNodeIndex].H;

			for ( int i = 1; i < open.Count; i++ ) {
				int nodeIndex = open[i];
				PlannerNode node = nodes[nodeIndex];

				if ( node.F < bestF || (node.F == bestF && node.H < bestH) ) {
					bestOpenIndex = i;
					bestNodeIndex = nodeIndex;
					bestF = node.F;
					bestH = node.H;
				}
			}

			return bestOpenIndex;
		}

		private static Plan ReconstructPlan( List<PlannerNode> nodes, int goalNodeIndex, PlannerAction[] actions )
		{
			List<PlanStep> reversed = new List<PlanStep>( 8 );

			int current = goalNodeIndex;
			while ( current >= 0 ) {
				PlannerNode node = nodes[current];
				if ( node.ActionIndex >= 0 ) {
					reversed.Add( new PlanStep( actions[node.ActionIndex] ) );
				}
				current = node.ParentIndex;
			}

			reversed.Reverse();
			return new Plan( reversed.ToArray() );
		}
	}
};
