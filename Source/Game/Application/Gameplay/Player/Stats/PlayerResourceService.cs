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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerResourceService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerResourceService : IPlayerResourceService, IDisposable {
		public IGameEvent<PlayerResourceChangedEventArgs> ResourceChanged => _resourceChanged;
		private readonly IGameEvent<PlayerResourceChangedEventArgs> _resourceChanged;

		private readonly IPlayerDerivedStatService _derivedStats;
		private readonly float[] _values = new float[ (int)PlayerResourceType.Count ];

		private readonly ISubscriptionHandle _derivedStatChanged;
		private bool _isDisposed = false;

		/*
		===============
		PlayerResourceService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="derivedStats"></param>
		/// <param name="eventFactory"></param>
		public PlayerResourceService( Guid id, IPlayerDerivedStatService derivedStats, IGameEventRegistryService eventFactory ) {
			ArgumentGuard.ThrowIfNull( derivedStats );
			ArgumentGuard.ThrowIfNull( eventFactory );

			_derivedStats = derivedStats;
			_resourceChanged = eventFactory.GetEvent<PlayerResourceChangedEventArgs>(
				$"{id}:{EventNames.PLAYER_RESOURCE_CHANGED}",
				EventNames.NAMESPACE
			);

			_derivedStatChanged = _derivedStats.DerivedStatChanged.Subscribe( OnDerivedStatChanged );
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
				_derivedStatChanged?.Dispose();
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
		public float GetValue( PlayerResourceType type ) {
			return _values[ (int)type ];
		}

		/*
		===============
		GetMaxValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public float GetMaxValue( PlayerResourceType type ) {
			return type switch {
				PlayerResourceType.Health => _derivedStats.GetValue( DerivedStatType.EffectiveHealthMax ),
				PlayerResourceType.Rage => _derivedStats.GetValue( DerivedStatType.EffectiveRageMax ),
				PlayerResourceType.Sanity => _derivedStats.GetValue( DerivedStatType.EffectiveSanityMax ),
				_ => throw new ArgumentOutOfRangeException( nameof( type ) )
			};
		}

		/*
		===============
		SetValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		public void SetValue( PlayerResourceType type, float value ) {
			float oldValue = _values[ (int)type ];
			float newValue = Clamp( type, value );

			if ( NearlyEqual( oldValue, newValue ) ) {
				return;
			}

			_values[ (int)type ] = newValue;
			_resourceChanged.Publish( new PlayerResourceChangedEventArgs( newValue, oldValue, type ) );
		}

		/*
		===============
		AddValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="delta"></param>
		public void AddValue( PlayerResourceType type, float delta ) {
			SetValue( type, GetValue( type ) + delta );
		}

		/*
		===============
		TryConsume
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool TryConsume( PlayerResourceType type, float amount ) {
			RangeGuard.ThrowIfLessThan( amount, 0.0f, nameof( amount ) );

			float current = GetValue( type );
			if ( current < amount ) {
				return false;
			}

			SetValue( type, current - amount );
			return true;
		}

		/*
		===============
		HasAtLeast
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool HasAtLeast( PlayerResourceType type, float amount ) {
			return GetValue( type ) >= amount;
		}

		/*
		===============
		Fill
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public void Fill( PlayerResourceType type ) {
			SetValue( type, GetMaxValue( type ) );
		}

		/*
		===============
		Empty
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public void Empty( PlayerResourceType type ) {
			SetValue( type, 0.0f );
		}

		/*
		===============
		NormalizeToCurrentMaxes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void NormalizeToCurrentMaxes() {
			SetValue( PlayerResourceType.Health, GetValue( PlayerResourceType.Health ) );
			SetValue( PlayerResourceType.Rage, GetValue( PlayerResourceType.Rage ) );
			SetValue( PlayerResourceType.Sanity, GetValue( PlayerResourceType.Sanity ) );
		}

		/*
		===============
		OnDerivedStatChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnDerivedStatChanged( in PlayerDerivedStatChangedEventArgs args ) {
			switch ( args.StatId ) {
				case DerivedStatType.EffectiveHealthMax:
					SetValue( PlayerResourceType.Health, GetValue( PlayerResourceType.Health ) );
					break;
				case DerivedStatType.EffectiveRageMax:
					SetValue( PlayerResourceType.Rage, GetValue( PlayerResourceType.Rage ) );
					break;
				case DerivedStatType.EffectiveSanityMax:
					SetValue( PlayerResourceType.Sanity, GetValue( PlayerResourceType.Sanity ) );
					break;
			}
		}

		/*
		===============
		Clamp
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private float Clamp( PlayerResourceType type, float value ) {
			float max = GetMaxValue( type );
			if ( max < 0.0f ) {
				max = 0.0f;
			}
			if ( value < 0.0f ) {
				return 0.0f;
			}
			if ( value > max ) {
				return max;
			}
			return value;
		}

		/*
		===============
		NearlyEqual
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private static bool NearlyEqual( float a, float b ) {
			return Math.Abs( a - b ) < 0.0001f;
		}
	}
}