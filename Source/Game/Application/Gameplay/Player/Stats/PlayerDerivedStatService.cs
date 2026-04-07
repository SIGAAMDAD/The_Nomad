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
	/// 
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
		/// 
		/// </summary>
		/// <param name="baseStats"></param>
		/// <param name="graph"></param>
		/// <param name="eventFactory"></param>
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
		/// 
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
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
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
		/// 
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
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
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
				default:
					throw new ArgumentOutOfRangeException( nameof( type ) );
			}
		}

		/*
		===============
		EvaluateMovementSpeedMultiplier
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="evaluator"></param>
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
		/// 
		/// </summary>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float EvaluateSanityDrainMultiplier() {
			return 1.0f;
		}

		/*
		===============
		FloatEquals
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <param name="e"></param>
		private void OnBaseStatChanged( in PlayerBaseStatChangedEventArgs e ) {
			HashSet<DerivedStatType> affected = new HashSet<DerivedStatType>();
			_graph.CollectAffectedFromBase( e.StatId, affected );

			foreach ( DerivedStatType type in affected ) {
				_dirty[(int)type] = true;
			}

			// Eager refresh hot-path values if desired:
			FlushDirty();
		}
	};
};