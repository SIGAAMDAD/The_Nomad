// <copyright file="Action.cs" company="Chris Muller">
// Copyright (c) Chris Muller. All rights reserved.
// </copyright>
#pragma warning disable S1172 // Unused method parameters should be removed
namespace MountainGoap {
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Represents an action in a GOAP system.
	/// </summary>
	public sealed class Action {
		private readonly float cost;
		private readonly Dictionary<string, PermutationSelectorCallback> permutationSelectors;
		private readonly ExecutorCallback executor;
		private readonly CostCallback costCallback;
		private readonly Dictionary<string, object> preconditions;
		private readonly Dictionary<string, ComparisonValuePair> comparativePreconditions;
		private readonly Dictionary<string, object> postconditions;
		private readonly Dictionary<string, object> arithmeticPostconditions;
		private readonly Dictionary<string, string> parameterPostconditions;
		private readonly StateMutatorCallback? stateMutator;
		private readonly StateCheckerCallback? stateChecker;
		private Dictionary<string, object> parameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="Action"/> class.
		/// </summary>
		/// <param name="name">Name for the action, for eventing and logging purposes.</param>
		/// <param name="permutationSelectors">The permutation selector callback for the action's parameters.</param>
		/// <param name="executor">The executor callback for the action.</param>
		/// <param name="cost">Cost of the action.</param>
		/// <param name="costCallback">Callback for determining the cost of the action.</param>
		/// <param name="preconditions">Preconditions required in the world state in order for the action to occur.</param>
		/// <param name="comparativePreconditions">Preconditions indicating relative value requirements needed for the action to occur.</param>
		/// <param name="postconditions">Postconditions applied after the action is successfully executed.</param>
		/// <param name="arithmeticPostconditions">Arithmetic postconditions added to state after the action is successfully executed.</param>
		/// <param name="parameterPostconditions">Parameter postconditions copied to state after the action is successfully executed.</param>
		/// <param name="stateMutator">Callback for modifying state after action execution or evaluation.</param>
		/// <param name="stateChecker">Callback for checking state before action execution or evaluation.</param>
		/// <param name="stateCostDeltaMultiplier">Callback for multiplier for delta value to provide delta cost.</param>
		public Action(
			string? name = null,
			Dictionary<string, PermutationSelectorCallback>? permutationSelectors = null,
			ExecutorCallback? executor = null,
			float cost = 1f,
			CostCallback? costCallback = null,
			Dictionary<string, object?>? preconditions = null,
			Dictionary<string, ComparisonValuePair>? comparativePreconditions = null,
			Dictionary<string, object?>? postconditions = null,
			Dictionary<string, object>? arithmeticPostconditions = null,
			Dictionary<string, string>? parameterPostconditions = null,
			StateMutatorCallback? stateMutator = null,
			StateCheckerCallback? stateChecker = null,
			StateCostDeltaMultiplierCallback? stateCostDeltaMultiplier = null ) {
			this.permutationSelectors = permutationSelectors ?? new Dictionary<string, PermutationSelectorCallback>();
			this.executor = executor ?? DefaultExecutorCallback;
			Name = name ?? $"Action {Guid.NewGuid()} ({this.executor.Method.Name})";
			this.cost = cost;
			this.costCallback = costCallback ?? DefaultCostCallback;
			this.preconditions = preconditions?.ToDictionary( kvp => kvp.Key, kvp => kvp.Value! )
				?? new Dictionary<string, object>();
			this.comparativePreconditions = comparativePreconditions?.ToDictionary( kvp => kvp.Key, kvp => kvp.Value )
				?? new Dictionary<string, ComparisonValuePair>();
			this.postconditions = postconditions?.ToDictionary( kvp => kvp.Key, kvp => kvp.Value! )
				?? new Dictionary<string, object>();
			this.arithmeticPostconditions = arithmeticPostconditions ?? new Dictionary<string, object>();
			this.parameterPostconditions = parameterPostconditions ?? new Dictionary<string, string>();
			this.stateMutator = stateMutator;
			this.stateChecker = stateChecker;
			StateCostDeltaMultiplier = stateCostDeltaMultiplier ?? DefaultStateCostDeltaMultiplier;
			parameters = new Dictionary<string, object>();
		}

		/// <summary>
		/// Name of the action.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets or sets multiplier for delta value to provide delta cost.
		/// </summary>
		public StateCostDeltaMultiplierCallback StateCostDeltaMultiplier { get; set; }

		/// <summary>
		/// Event that triggers when an action begins executing.
		/// </summary>
		public static event BeginExecuteActionEvent OnBeginExecuteAction = ( agent, action, parameters ) => { };

		/// <summary>
		/// Event that triggers when an action finishes executing.
		/// </summary>
		public static event FinishExecuteActionEvent OnFinishExecuteAction = ( agent, action, status, parameters ) => { };

		/// <summary>
		/// Gets or sets the execution status of the action.
		/// </summary>
		internal ExecutionStatus ExecutionStatus { get; set; } = ExecutionStatus.NotYetExecuted;

		/// <summary>
		/// Default state cost delta multiplier callback.
		/// </summary>
		public static float DefaultStateCostDeltaMultiplier( Action action, string stateKey ) => 1f;

		/// <summary>
		/// Makes a copy of the action.
		/// </summary>
		/// <returns>A copy of the action.</returns>
		public Action Copy() {
			var newAction = new Action(
				Name,
				permutationSelectors,
				executor,
				cost,
				costCallback,
				new Dictionary<string, object?>( preconditions ),
				new Dictionary<string, ComparisonValuePair>( comparativePreconditions ),
				new Dictionary<string, object?>( postconditions ),
				new Dictionary<string, object>( arithmeticPostconditions ),
				new Dictionary<string, string>( parameterPostconditions ),
				stateMutator,
				stateChecker,
				StateCostDeltaMultiplier ) {
				parameters = new Dictionary<string, object>( parameters )
			};
			return newAction;
		}

		/// <summary>
		/// Sets a parameter to the action.
		/// </summary>
		/// <param name="key">Key to be set.</param>
		/// <param name="value">Value to be set.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void SetParameter( string key, object value ) {
			if ( parameters.ContainsKey( key ) ) {
				parameters[ key ] = value;
			} else {
				throw new KeyNotFoundException( key );
			}
		}

		/// <summary>
		/// Gets a parameter to the action.
		/// </summary>
		/// <param name="key">Key for the value to be retrieved.</param>
		/// <returns>The value stored at the key specified.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public object? GetParameter( string key ) {
			return parameters.TryGetValue( key, out object? value ) ? value : null;
		}

		/// <summary>
		/// Gets the cost of the action.
		/// </summary>
		/// <param name="currentState">State as it will be when cost is relevant.</param>
		/// <returns>The cost of the action.</returns>
		public float GetCost( ConcurrentDictionary<string, object> currentState ) {
			try {
				return costCallback( this, currentState );
			} catch {
				return float.MaxValue;
			}
		}

		/// <summary>
		/// Executes a step of work for the agent.
		/// </summary>
		/// <param name="agent">Agent executing the action.</param>
		/// <returns>The execution status of the action.</returns>
		internal ExecutionStatus Execute( Agent agent ) {
			OnBeginExecuteAction( agent, this, parameters );
			if ( IsPossible( agent.State ) ) {
				var newState = executor( agent, this );
				if ( newState == ExecutionStatus.Succeeded ) {
					ApplyEffects( agent.State );
				}
				ExecutionStatus = newState;
				OnFinishExecuteAction( agent, this, ExecutionStatus, parameters );
				return newState;
			} else {
				OnFinishExecuteAction( agent, this, ExecutionStatus.NotPossible, parameters );
				return ExecutionStatus.NotPossible;
			}
		}

		/// <summary>
		/// Determines whether or not an action is possible.
		/// </summary>
		/// <param name="state">The current world state.</param>
		/// <returns>True if the action is possible, otherwise false.</returns>
		internal bool IsPossible( ConcurrentDictionary<string, object> state ) {
			foreach ( var kvp in preconditions ) {
				if ( !state.TryGetValue( kvp.Key, out object? value ) || !Equals( value, kvp.Value ) ) {
					return false;
				}
			}

			foreach ( var kvp in comparativePreconditions ) {
				if ( !state.TryGetValue( kvp.Key, out object? value ) || value is null ) {
					return false;
				}

				if ( kvp.Value.Value is not object conditionValue ) {
					return false;
				}

				bool comparisonValid = kvp.Value.Operator switch {
					ComparisonOperator.LessThan => Utils.IsLowerThan( value, conditionValue ),
					ComparisonOperator.GreaterThan => Utils.IsHigherThan( value, conditionValue ),
					ComparisonOperator.LessThanOrEquals => Utils.IsLowerThanOrEquals( value, conditionValue ),
					ComparisonOperator.GreaterThanOrEquals => Utils.IsHigherThanOrEquals( value, conditionValue ),
					_ => false
				};

				if ( !comparisonValid ) {
					return false;
				}
			}

			return stateChecker?.Invoke( this, state ) != false;
		}

		/// <summary>
		/// Gets all permutations of parameters possible for an action.
		/// </summary>
		/// <param name="state">World state when the action would be performed.</param>
		/// <returns>A list of possible parameter dictionaries that could be used.</returns>
		internal IEnumerable<Dictionary<string, object>> GetPermutations( ConcurrentDictionary<string, object> state ) {
			if ( permutationSelectors.Count == 0 ) {
				yield return new Dictionary<string, object>();
				yield break;
			}

			var keys = permutationSelectors.Keys.ToArray();
			var valueLists = new List<object[]>( keys.Length );
			foreach ( var key in keys ) {
				var values = permutationSelectors[ key ]( state ).ToArray();
				if ( values.Length == 0 ) {
					yield break;
				}
				valueLists.Add( values );
			}

			var indices = new int[ keys.Length ];
			while ( true ) {
				var result = new Dictionary<string, object>( keys.Length );
				for ( int i = 0; i < keys.Length; i++ ) {
					result[ keys[ i ] ] = valueLists[ i ][ indices[ i ] ];
				}
				yield return result;

				int currentIndex = 0;
				while ( currentIndex < keys.Length && indices[ currentIndex ] == valueLists[ currentIndex ].Length - 1 ) {
					indices[ currentIndex ] = 0;
					currentIndex++;
				}

				if ( currentIndex == keys.Length ) {
					yield break;
				}
				indices[ currentIndex ]++;
			}
		}

		/// <summary>
		/// Applies the effects of the action.
		/// </summary>
		/// <param name="state">World state to which to apply effects.</param>
		internal void ApplyEffects( ConcurrentDictionary<string, object?> state ) {
			foreach ( var kvp in postconditions ) state[ kvp.Key ] = kvp.Value;

			foreach ( var kvp in arithmeticPostconditions ) {
				if ( !state.TryGetValue( kvp.Key, out object? currentValue ) ) {
					continue;
				}

				switch ( currentValue, kvp.Value ) {
					case ( int i, int j ):
						state[ kvp.Key ] = i + j;
						break;
					case ( float f, float g ):
						state[ kvp.Key ] = f + g;
						break;
					case ( double d, double e ):
						state[ kvp.Key ] = d + e;
						break;
					case ( long l, long m ):
						state[ kvp.Key ] = l + m;
						break;
					case ( decimal a, decimal b ):
						state[ kvp.Key ] = a + b;
						break;
					case ( DateTime dt, TimeSpan ts ):
						state[ kvp.Key ] = dt + ts;
						break;
				}
			}

			foreach ( var kvp in parameterPostconditions ) {
				if ( parameters.TryGetValue( kvp.Key, out object? value ) ) {
					state[ kvp.Value ] = value;
				}
			}

			stateMutator?.Invoke( this, state );
		}

		/// <summary>
		/// Sets all parameters to the action.
		/// </summary>
		/// <param name="parameters">Dictionary of parameters to be passed to the action.</param>
		internal void SetParameters( Dictionary<string, object?> parameters ) {
			this.parameters = new Dictionary<string, object>( parameters! );
		}

		private static ExecutionStatus DefaultExecutorCallback( Agent agent, Action action ) {
			return ExecutionStatus.Failed;
		}

		private static float DefaultCostCallback( Action action, ConcurrentDictionary<string, object?> currentState ) {
			return action.cost;
		}
	}
}