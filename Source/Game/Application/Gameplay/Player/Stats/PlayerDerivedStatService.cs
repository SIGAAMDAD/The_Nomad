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
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player.Stats {
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

	internal sealed class PlayerDerivedStatService : IPlayerDerivedStatService {
		private readonly IPlayerBaseStatsRepository _baseStats;
		private readonly PlayerStatDependencyGraph _graph;

		private readonly float[] _values = new float[(int)DerivedStatType.Count];
		private readonly bool[] _dirty = new bool[(int)DerivedStatType.Count];

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
		/// <param name="baseStats">Repository providing access to base player statistics.</param>
		/// <param name="graph">Dependency graph defining relationships between derived stats.</param>
		/// <param name="eventFactory">Factory for creating game events.</param>
		public PlayerDerivedStatService( IPlayerBaseStatsRepository baseStats, PlayerStatDependencyGraph graph, IGameEventRegistryService eventFactory ) {
			_baseStats = baseStats;
			_graph = graph;
			_derivedStatChanged = eventFactory.GetEvent<PlayerDerivedStatChangedEventArgs>(
				EventNames.PLAYER_DERIVED_STAT_CHANGED,
				EventNames.NAMESPACE
			);

			for ( int i = 0; i < _dirty.Length; i++ ) {
				_dirty[i] = true;
			}

			_baseStats.BaseStatChanged.Subscribe( OnBaseStatChanged );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// Cleans up resources and unsubscribes from base stat change events.
		/// </summary>
		public void Dispose() {
			if ( !_isDisposed ) {
				_baseStats.BaseStatChanged.Unsubscribe( OnBaseStatChanged );
			}
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
		public float GetValue( DerivedStatType type ) {
			if ( _dirty[(int)type] ) {
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
		public void FlushDirty() {
			for ( int i = 0; i < _dirty.Length; i++ ) {
				if ( _dirty[i] ) {
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
		private void RecalculateWithDependencies( DerivedStatType type ) {
			switch ( type ) {
				case DerivedStatType.MovementSpeedMultiplier:
					RecalculateSingle( type, EvaluateMovementSpeedMultiplier );
					break;
				case DerivedStatType.EffectiveMovementSpeed:
					// Ensure dependency is valid first
					GetValue( DerivedStatType.MovementSpeedMultiplier );
					RecalculateSingle( type, EvaluateEffectiveMovementSpeed );
					break;
				case DerivedStatType.DashSpeedMultiplier:
					RecalculateSingle( type, EvaluateDashSpeedMultiplier );
					break;
				case DerivedStatType.EffectiveDashSpeed:
					GetValue( DerivedStatType.DashSpeedMultiplier );
					RecalculateSingle( type, EvaluateEffectiveDashSpeed );
					break;
				case DerivedStatType.EffectiveHealthMax:
					RecalculateSingle( type, EvaluateEffectiveHealthMax );
					break;
				case DerivedStatType.EffectiveRageMax:
					RecalculateSingle( type, EvaluateEffectiveRageMax );
					break;
				case DerivedStatType.SanityDrainMultiplier:
					RecalculateSingle( type, EvaluateSanityDrainMultiplier );
					break;
				case DerivedStatType.EffectiveSanityMax:
					RecalculateSingle( type, EvaluateEffectiveSanityMax );
					break;
				default:
					throw new ArgumentOutOfRangeException( $"{type}" );
			}
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
		private void RecalculateSingle( DerivedStatType type, Func<float> evaluator ) {
			float oldValue = _values[(int)type];
			float newValue = evaluator();

			_values[(int)type] = newValue;
			_dirty[(int)type] = false;

			if ( !FloatEquals( oldValue, newValue ) ) {
				_derivedStatChanged.Publish(
					new PlayerDerivedStatChangedEventArgs( newValue, oldValue, type )
				);

				// If this derived stat feeds others, mark them dirty
				HashSet<DerivedStatType> affected = new HashSet<DerivedStatType>();
				_graph.CollectAffectedFromDerived( type, affected );

				foreach ( DerivedStatType child in affected ) {
					_dirty[(int)child] = true;
				}
			}
		}

		/*
		===============
		EvaluateMovementSpeedMultiplier
		===============
		*/
		/// <summary>
		/// Calculates the movement speed multiplier based on inventory weight and encumbrance threshold.
		/// Applies a penalty when carrying weight exceeds the threshold.
		/// </summary>
		/// <returns>The movement speed multiplier (0.2 to 1.0).</returns>
		private float EvaluateMovementSpeedMultiplier() {
			float weight = _baseStats.GetBaseStatValue( BaseStatType.InventoryWeight );
			float threshold = _baseStats.GetBaseStatValue( BaseStatType.EncumbranceThreshold );

			if ( threshold <= 0.0f ) {
				return 1.0f;
			}

			if ( weight <= threshold ) {
				return 1.0f;
			}

			float overRatio = (weight - threshold) / threshold;

			// Example curve: up to 80% slowdown cap
			float penalty = Math.Min( overRatio * 0.35f, 0.80f );
			return Math.Max( 0.20f, 1.0f - penalty );
		}

		/*
		===============
		EvaluateEffectiveMovementSpeed
		===============
		*/
		/// <summary>
		/// Calculates the effective movement speed by multiplying base speed with the movement speed multiplier.
		/// </summary>
		/// <returns>The effective movement speed (base speed * multiplier).</returns>
		private float EvaluateEffectiveMovementSpeed() {
			float baseSpeed = _baseStats.GetBaseStatValue( BaseStatType.BaseMovementSpeed );
			float moveMultiplier = _values[(int)DerivedStatType.MovementSpeedMultiplier];
			return Math.Max( 0.0f, baseSpeed * moveMultiplier );
		}

		/*
		===============
		EvaluateDashSpeedMultiplier
		===============
		*/
		/// <summary>
		/// Calculates the dash speed multiplier. Currently returns 1.0 (no modification).
		/// </summary>
		/// <returns>The dash speed multiplier.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float EvaluateDashSpeedMultiplier() {
			return 1.0f;
		}

		/*
		===============
		EvaluateEffectiveDashSpeed
		===============
		*/
		/// <summary>
		/// Calculates the effective dash speed by multiplying base dash speed with the dash speed multiplier.
		/// </summary>
		/// <returns>The effective dash speed (base dash speed * multiplier).</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float EvaluateEffectiveDashSpeed() {
			float baseDashSpeed = _baseStats.GetBaseStatValue( BaseStatType.BaseDashSpeed );
			float multiplier = _values[(int)DerivedStatType.DashSpeedMultiplier];
			return Math.Max( 0.0f, baseDashSpeed * multiplier );
		}

		/*
		===============
		EvaluateEffectiveHealthMax
		===============
		*/
		/// <summary>
		/// Calculates the effective maximum health, currently equal to base health.
		/// </summary>
		/// <returns>The effective maximum health.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float EvaluateEffectiveHealthMax() {
			return _baseStats.GetBaseStatValue( BaseStatType.BaseHealth );
		}

		/*
		===============
		EvaluateEffectiveRageMax
		===============
		*/
		/// <summary>
		/// Calculates the effective maximum rage, currently equal to base rage.
		/// </summary>
		/// <returns>The effective maximum rage.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float EvaluateEffectiveRageMax() {
			return _baseStats.GetBaseStatValue( BaseStatType.BaseRage );
		}

		/*
		===============
		EvaluateSanityDrainMultiplier
		===============
		*/
		/// <summary>
		/// Calculates the sanity drain multiplier. Currently returns 1.0 (no modification).
		/// </summary>
		/// <returns>The sanity drain multiplier.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float EvaluateSanityDrainMultiplier() {
			return 1.0f;
		}

		/*
		===============
		EvaluateEffectiveSanityMax
		===============
		*/
		/// <summary>
		/// Calculates the effective maximum sanity, currently equal to base sanity.
		/// </summary>
		/// <returns>The effective maximum sanity.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float EvaluateEffectiveSanityMax() {
			return _baseStats.GetBaseStatValue( BaseStatType.BaseSanity );
		}

		/*
		===============
		FloatEquals
		===============
		*/
		/// <summary>
		/// Compares two float values for equality within a small tolerance.
		/// </summary>
		/// <param name="a">First float value.</param>
		/// <param name="b">Second float value.</param>
		/// <returns>True if the values are approximately equal.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool FloatEquals( float a, float b ) {
			return Math.Abs( a - b ) < 0.0001f;
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
		private void OnBaseStatChanged( in PlayerBaseStatChangedEventArgs args ) {
			HashSet<DerivedStatType> affected = new HashSet<DerivedStatType>();
			_graph.CollectAffectedFromBase( args.StatId, affected );

			foreach ( DerivedStatType type in affected ) {
				_dirty[(int)type] = true;
			}

			// Eager refresh hot-path values if desired:
			FlushDirty();
		}
	};
};