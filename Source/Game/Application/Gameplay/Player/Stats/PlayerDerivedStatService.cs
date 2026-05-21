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
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player.Stats;
using Nomad.Game.Application.Gameplay.Player.Stats.DerivedStatEvaluators;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Core.Numerics;

namespace Nomad.Game.Application.Gameplay.Player.Stats
{
	/*
	===================================================================================

	PlayerDerivedStatService

	===================================================================================
	*/
	/// <summary>
	/// Service responsible for calculating and managing derived player statistics.
	/// Derived stats are calculated from base stats and may have dependencies on other derived stats.
	/// This service handles caching, dependency tracking, and automatic recalculation when base stats change.
	/// </summary>

	internal sealed class PlayerDerivedStatService : IPlayerDerivedStatService
	{
		private readonly IPlayerBaseStatsRepository _baseStats;
		private readonly PlayerStatDependencyGraph _graph;
		private readonly List<IPlayerDerivedStatEvaluator> _evaluators = new();

		private readonly float[] _values = new float[(int)DerivedStatType.Count];
		private readonly PackedBitSet _dirty = new PackedBitSet( (int)DerivedStatType.Count );

		private readonly PlayerId _playerId;
		private readonly IDisposable _baseStatChanged;

		private bool _isDisposed = false;

		public IGameEvent<PlayerDerivedStatChangedEventArgs> DerivedStatChanged => _derivedStatChanged;
		private readonly IGameEvent<PlayerDerivedStatChangedEventArgs> _derivedStatChanged;

		/*
		===============
		PlayerDerivedStatService
		===============
		*/
		/// <summary>
		/// Initializes a new instance of the PlayerDerivedStatService.
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="baseStats">Repository providing access to base player statistics.</param>
		/// <param name="graph">Dependency graph defining relationships between derived stats.</param>
		/// <param name="eventFactory">Factory for creating game events.</param>
		public PlayerDerivedStatService( PlayerId playerId, IPlayerBaseStatsRepository baseStats, PlayerStatDependencyGraph graph, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			if ( playerId == PlayerId.Invalid ) {
				throw new InvalidOperationException( "PlayerDerivedStatsService given an invalid PlayerId!" );
			}

			_playerId = playerId;

			_baseStats = baseStats ?? throw new ArgumentNullException( nameof( baseStats ) );
			_graph = graph ?? throw new ArgumentNullException( nameof( graph ) );

			_derivedStatChanged = eventFactory.GetEvent<PlayerDerivedStatChangedEventArgs>(
				PlayerDerivedStatChangedEventArgs.Name,
				PlayerDerivedStatChangedEventArgs.NameSpace
			);

			for ( int i = 0; i < (int)DerivedStatType.Count; i++ ) {
				_dirty.Set( i, true );
			}

			_evaluators.Add( new MovementDerivedStatEvaluator() );
			_evaluators.Add( new HealthDerivedStatEvaluator() );
			_evaluators.Add( new RageDerivedStatEvaluator() );
			_evaluators.Add( new SanityDerivedStatEvaluator() );
			_evaluators.Add( new DashKitDerivedStatEvaluator() );

			_baseStatChanged = _baseStats.BaseStatChanged.Subscribe( OnBaseStatChanged );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// Cleans up resources and unsubscribes from base stat change events.
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_derivedStatChanged.Dispose();
			_baseStatChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		GetValue
		===============
		*/
		/// <summary>
		/// Gets the current value of a derived stat, recalculating it if necessary.
		/// </summary>
		/// <param name="type">The type of derived stat to retrieve.</param>
		/// <returns>The calculated value of the derived stat.</returns>
		public float GetValue( DerivedStatType type )
		{
			if ( _dirty.Get( (int)type ) ) {
				RecalculateWithDependencies( type );
			}
			return _values[(int)type];
		}

		/*
		===============
		FlushDirty
		===============
		*/
		/// <summary>
		/// Recalculates all dirty derived stats and publishes change events for any that have changed.
		/// </summary>
		public void FlushDirty()
		{
			for ( int i = 0; i < (int)DerivedStatType.Count; i++ ) {
				if ( _dirty.Get( i ) ) {
					RecalculateWithDependencies( (DerivedStatType)i );
				}
			}
		}

		/*
		===============
		RecalculateWithDependencies
		===============
		*/
		/// <summary>
		/// Recalculates a specific derived stat, ensuring all dependencies are calculated first.
		/// </summary>
		/// <param name="type">The derived stat type to recalculate.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported derived stat type is provided.</exception>
		private void RecalculateWithDependencies( DerivedStatType type )
		{
			EnsureDependencies( type );
			RecalculateSingle( type, () => EvaluateFromEvaluator( type ) );
		}

		/*
		===============
		EvaluateMovementSpeedMultiplier
		===============
		*/
		/// <summary>
		/// Recalculates a single derived stat using the provided evaluator function.
		/// Publishes change events and marks dependent stats as dirty if the value changed.
		/// </summary>
		/// <param name="type">The derived stat type being recalculated.</param>
		/// <param name="evaluator">Function that calculates the new value for the stat.</param>
		private void RecalculateSingle( DerivedStatType type, Func<float> evaluator )
		{
			float oldValue = _values[(int)type];
			float newValue = evaluator();

			_values[(int)type] = newValue;
			_dirty.Set( (int)type, false );

			if ( !ScalarMath.NearlyEqual( oldValue, newValue ) ) {
				_derivedStatChanged.Publish(
					new PlayerDerivedStatChangedEventArgs(
						newValue,
						oldValue,
						type
					)
				);

				// If this derived stat feeds others, mark them dirty
				HashSet<DerivedStatType> affected = new HashSet<DerivedStatType>();
				_graph.CollectAffectedFromDerived( type, affected );

				foreach ( DerivedStatType child in affected ) {
					_dirty.Set( (int)child, true );
				}
			}
		}

		/*
		===============
		EnsureDependencies
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private void EnsureDependencies( DerivedStatType type )
		{
			switch ( type ) {
				case DerivedStatType.EffectiveMovementSpeed:
					GetValue( DerivedStatType.MovementSpeedMultiplier );
					break;
				case DerivedStatType.EffectiveDashSpeed:
					GetValue( DerivedStatType.DashSpeedMultiplier );
					break;
				case DerivedStatType.MovementSpeedMultiplier:
				case DerivedStatType.DashSpeedMultiplier:
				case DerivedStatType.EffectiveHealthMax:
				case DerivedStatType.EffectiveRageMax:
				case DerivedStatType.EffectiveSanityMax:
				case DerivedStatType.SanityDrainMultiplier:
					break;
				default:
					throw new ArgumentOutOfRangeException( nameof( type ), $"Unsupported derived stat '{type}'." );
			}
		}

		/*
		===============
		EvaluateFromEvaluator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private float EvaluateFromEvaluator( DerivedStatType type )
		{
			PlayerDerivedStatEvaluationContext context = new PlayerDerivedStatEvaluationContext( _baseStats, GetValue );

			for ( int i = 0; i < _evaluators.Count; i++ ) {
				if ( _evaluators[i].CanEvaluate( type ) ) {
					return _evaluators[i].Evaluate( type, in context );
				}
			}

			throw new ArgumentOutOfRangeException( nameof( type ), $"No derived stat evaluator registered for '{type}'." );
		}

		/*
		===============
		OnBaseStatChanged
		===============
		*/
		/// <summary>
		/// Handles base stat change events by marking affected derived stats as dirty and recalculating them.
		/// </summary>
		/// <param name="args">Event arguments containing information about the changed base stat.</param>
		private void OnBaseStatChanged( in PlayerBaseStatChangedEventArgs args )
		{
			HashSet<DerivedStatType> affected = new HashSet<DerivedStatType>();
			_graph.CollectAffectedFromBase( args.StatId, affected );

			foreach ( DerivedStatType type in affected ) {
				_dirty.Set( (int)type, true );
			}

			foreach ( DerivedStatType type in affected ) {
				if ( _dirty.Get( (int)type ) ) {
					RecalculateWithDependencies( type );
				}
			}
		}
	};
};
