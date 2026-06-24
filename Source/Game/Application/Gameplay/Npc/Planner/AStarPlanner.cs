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
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Sdk.Npc.Planner;
using Nomad.Game.Sdk.Npc.Planner.Goals;

namespace Nomad.Game.Application.Gameplay.Npc.Planner
{
	/*
	===================================================================================

	AStarPlanner

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class AStarPlanner
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="start"></param>
		/// <param name="goal"></param>
		/// <param name="context"></param>
		/// <param name="actions"></param>
		/// <param name="maxExpansions"></param>
		/// <param name="plan"></param>
		/// <returns></returns>
		public static bool TryPlan(
			WorldState start,
			GoalDef goal,
			PlanningContext context,
			PlannerScratch scratch,
			PlannerAction[] actions,
			int maxExpansions,
			out Plan plan
		)
		{
#if DEBUG
			ArgumentGuard.ThrowIfNull( goal, nameof( goal ) );
			ArgumentGuard.ThrowIfNull( actions, nameof( actions ) );
#endif
			if ( goal.IsSatisfied( start ) ) {
				plan = Plan.Empty;
				return true;
			}

			PlannerNode startNode = new PlannerNode {
				State = start,
				ParentIndex = -1,
				ActionIndex = -1,
				G = 0,
				H = Heuristic( start, goal )
			};

			scratch.Prepare( actions.Length );

			for ( int i = 0; i < actions.Length; i++ ) {
				PlannerAction action = actions[i];

				scratch.ActionContextValid[i] =
					action.ValidateContext == null || action.ValidateContext.Invoke( context );

				scratch.ActionCosts[i] =
					action.BaseCost + (action.GetDynamicCost != null ? action.GetDynamicCost.Invoke( context ) : 0);
			}

			scratch.Nodes.Add( startNode );
			scratch.Open.Add( 0 );
			scratch.BestG[start] = 0;

			int expansions = 0;

			while ( scratch.Open.Count > 0 && expansions < maxExpansions ) {
				int openListIndex = FindLowestFIndex( scratch.Open, scratch.Nodes );
				int currentNodeIndex = scratch.Open[openListIndex];
				scratch.Open.RemoveAt( openListIndex );

				PlannerNode current = scratch.Nodes[currentNodeIndex];

				if ( goal.IsSatisfied( current.State ) ) {
					plan = ReconstructPlan( scratch.Nodes, currentNodeIndex, actions );
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
					int nextG = current.G + scratch.ActionCosts[i];

					if ( scratch.BestG.TryGetValue( nextState, out int knownG ) && nextG >= knownG ) {
						continue;
					}

					scratch.BestG[nextState] = nextG;
					scratch.Nodes.Add( new PlannerNode {
						State = nextState,
						ParentIndex = currentNodeIndex,
						ActionIndex = i,
						G = nextG,
						H = Heuristic( nextState, goal )
					} );
					scratch.Open.Add( scratch.Nodes.Count - 1 );
				}
			}

			plan = Plan.Empty;
			return false;
		}

		/*
		===============
		Heuristic
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="state"></param>
		/// <param name="goal"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static int Heuristic( WorldState state, GoalDef goal )
		{
			return state.CountUnmet( goal.DesiredMask );
		}

		/*
		===============
		FindLowestFIndex
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="open"></param>
		/// <param name="nodes"></param>
		/// <returns></returns>
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
