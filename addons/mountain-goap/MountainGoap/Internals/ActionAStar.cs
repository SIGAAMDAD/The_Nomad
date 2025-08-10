// <copyright file="ActionAStar.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>

namespace MountainGoap {
	using System;
	using System.Collections.Concurrent;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using Priority_Queue;

	/// <summary>
	/// AStar calculator for an action graph.
	/// </summary>
	internal class ActionAStar {
		/// <summary>
		/// Final point at which the calculation arrived.
		/// </summary>
		internal readonly ActionNode? FinalPoint = null;

		/// <summary>
		/// Cost so far to get to each node.
		/// </summary>
		internal readonly ConcurrentDictionary<ActionNode, float> CostSoFar = new();

		/// <summary>
		/// Steps so far to get to each node.
		/// </summary>
		internal readonly ConcurrentDictionary<ActionNode, int> StepsSoFar = new();

		/// <summary>
		/// Dictionary giving the path from start to goal.
		/// </summary>
		internal readonly ConcurrentDictionary<ActionNode, ActionNode> CameFrom = new();

		/// <summary>
		/// Goal state that AStar is trying to achieve.
		/// </summary>
		private readonly BaseGoal goal;

		/// <summary>
		/// Initializes a new instance of the <see cref="ActionAStar"/> class.
		/// </summary>
		/// <param name="graph">Graph to be traversed.</param>
		/// <param name="start">Action from which to start.</param>
		/// <param name="goal">Goal state to be achieved.</param>
		/// <param name="costMaximum">Maximum allowable cost for a plan.</param>
		/// <param name="stepMaximum">Maximum allowable steps for a plan.</param>
		internal ActionAStar( ActionGraph graph, ActionNode start, BaseGoal goal, float costMaximum, int stepMaximum ) {
			this.goal = goal;
			FastPriorityQueue<ActionNode> frontier = new( 1024 );
			frontier.Enqueue( start, 0 );
			CameFrom[ start ] = start;
			CostSoFar[ start ] = 0;
			StepsSoFar[ start ] = 0;
			while ( frontier.Count > 0 ) {
				var current = frontier.Dequeue();
				if ( MeetsGoal( current, start ) ) {
					FinalPoint = current;
					break;
				}
				foreach ( var next in graph.Neighbors( current ) ) {
					float newCost = CostSoFar[ current ] + next.Cost( current.State );
					int newStepCount = StepsSoFar[ current ] + 1;
					if ( newCost > costMaximum || newStepCount > stepMaximum ) continue;
					if ( !CostSoFar.TryGetValue( next, out float value ) || newCost < value ) {
						value = newCost;
						CostSoFar[ next ] = value;
						StepsSoFar[ next ] = newStepCount;
						float priority = newCost + Heuristic( next, goal, current );
						frontier.Enqueue( next, priority );
						CameFrom[ next ] = current;
						Agent.TriggerOnEvaluatedActionNode( next, CameFrom );
					}
				}
			}
		}

		private static float Heuristic( ActionNode actionNode, BaseGoal goal, ActionNode current ) {
			var cost = 0f;
			if ( goal is Goal normalGoal ) {
				normalGoal.DesiredState.Select( kvp => kvp.Key ).ToList().ForEach( key => {
					if ( !actionNode.State.TryGetValue( key, out object value ) ) cost++;
					else if ( value == null && value != normalGoal.DesiredState[ key ] ) cost++;
					else if ( !value.Equals( normalGoal.DesiredState[ key ] ) ) cost++;
				} );
			} else if ( goal is ExtremeGoal extremeGoal ) {
				foreach ( var kvp in extremeGoal.DesiredState ) {
					var valueDiff = 0f;
					var valueDiffMultiplier = ( actionNode?.Action?.StateCostDeltaMultiplier ?? Action.DefaultStateCostDeltaMultiplier ).Invoke( actionNode?.Action, kvp.Key );
					bool hasValue = actionNode.State.TryGetValue( kvp.Key, out object value );
					if ( hasValue && value == null ) {
						cost += float.PositiveInfinity;
						continue;
					}
					if ( hasValue && extremeGoal.DesiredState.ContainsKey( kvp.Key ) ) {
						valueDiff = Convert.ToSingle( value ) - Convert.ToSingle( value );
					}
					if ( !hasValue ) {
						cost += float.PositiveInfinity;
					} else if ( !current.State.TryGetValue( kvp.Key, out object currentState ) ) {
						cost += float.PositiveInfinity;
					} else if ( !kvp.Value && hasValue && IsLowerThanOrEquals( value, currentState ) ) {
						cost += valueDiff * valueDiffMultiplier;
					} else if ( kvp.Value && hasValue && IsHigherThanOrEquals( value, currentState ) ) {
						cost -= valueDiff * valueDiffMultiplier;
					}
				}
			} else if ( goal is ComparativeGoal comparativeGoal ) {
				foreach ( var kvp in comparativeGoal.DesiredState ) {
					var valueDiff2 = 0f;
					var valueDiffMultiplier = ( actionNode?.Action?.StateCostDeltaMultiplier ?? Action.DefaultStateCostDeltaMultiplier ).Invoke( actionNode?.Action, kvp.Key );
					bool hasValue = actionNode.State.TryGetValue( kvp.Key, out object value );
					bool comparativeHasValue = comparativeGoal.DesiredState.TryGetValue( kvp.Key, out ComparisonValuePair compareValue );
					if ( hasValue && comparativeHasValue ) {
						valueDiff2 = Math.Abs( Convert.ToSingle( value ) - Convert.ToSingle( value ) );
					}
					if ( !hasValue ) {
						cost += float.PositiveInfinity;
					} else if ( !current.State.ContainsKey( kvp.Key ) ) {
						cost += float.PositiveInfinity;
					} else {
						bool diff = false;
						switch ( kvp.Value.Operator ) {
						case ComparisonOperator.Equals:
							diff = !value.Equals( compareValue.Value );
							break;
						case ComparisonOperator.LessThan:
							diff = !IsLowerThan( value, compareValue.Value );
							break;
						case ComparisonOperator.GreaterThan:
							diff = !IsHigherThan( value, compareValue.Value );
							break;
						case ComparisonOperator.LessThanOrEquals:
							diff = !IsLowerThanOrEquals( value, compareValue.Value );
							break;
						case ComparisonOperator.GreaterThanOrEquals:
							diff = !IsHigherThanOrEquals( value, compareValue.Value );
							break;
						case ComparisonOperator.Undefined:
						default:
							cost += float.PositiveInfinity;
							break;
						};
						if ( diff ) {
							cost += valueDiff2 * valueDiffMultiplier;
						}
					}
				}
			}
			return cost;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsLowerThan( object a, object b ) => Utils.IsLowerThan( a, b );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsHigherThan( object a, object b ) => Utils.IsHigherThan( a, b );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsLowerThanOrEquals( object a, object b ) => Utils.IsLowerThanOrEquals( a, b );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsHigherThanOrEquals( object a, object b ) => Utils.IsHigherThanOrEquals( a, b );
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private bool MeetsGoal( ActionNode actionNode, ActionNode current ) => Utils.MeetsGoal( goal, actionNode, current );
	}
}
