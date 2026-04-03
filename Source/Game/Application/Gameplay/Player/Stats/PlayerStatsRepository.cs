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

using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using System;
using System.Collections.Concurrent;

namespace Nomad.Game.Application.Gameplay.Player.Stats {
	/*
	===================================================================================
	
	PlayerStatsRepository
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class PlayerStatsRepository : IPlayerStatsRepository {
		private readonly PlayerStatValue[] _statValues = new PlayerStatValue[ (int)StatType.Count ];

		private ConcurrentDictionary<InternString, DynamicPlayerStatValue> _dynamicValues => _dynamicStatValueCache.Value;
		private readonly Lazy<ConcurrentDictionary<InternString, DynamicPlayerStatValue>> _dynamicStatValueCache = new Lazy<ConcurrentDictionary<InternString, DynamicPlayerStatValue>>();

		private readonly ILoggerCategory _category;

		public IGameEvent<PlayerStatChangedEventArgs> StatChanged => _statChanged;
		private readonly IGameEvent<PlayerStatChangedEventArgs> _statChanged;

		/*
		===============
		PlayerStatsRepository
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		public PlayerStatsRepository( IGameEventRegistryService eventFactory, ILoggerService logger ) {
			ArgumentGuard.ThrowIfNull( eventFactory );
			ArgumentGuard.ThrowIfNull( logger );

			_statChanged = eventFactory.GetEvent<PlayerStatChangedEventArgs>( EventNames.PLAYER_STAT_CHANGED, EventNames.NAMESPACE );
			_category = logger?.CreateCategory( nameof( PlayerStatsRepository ), LogLevel.Info, true );
		}

		/*
		===============
		AddDynamicStat
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="statId"></param>
		/// <param name="limits"></param>
		/// <param name="initialValue"></param>
		public void AddDynamicStat( InternString statId, StatLimits limits, float initialValue ) {
			if ( HasDynamicStat( statId ) ) {
				return;
			}
			_dynamicValues[ statId ] = new DynamicPlayerStatValue( statId, initialValue, limits );
			_category.PrintLine( $"Created a new dynamic player stat '{(string)statId}'" );
		}

		/*
		===============
		GetDynamicStatValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="statId"></param>
		/// <returns></returns>
		public float GetDynamicStatValue( InternString statId ) {
			if ( !_dynamicValues.TryGetValue( statId, out var value ) ) {
				AddDynamicStat( statId, new StatLimits(), 0.0f );
			}
			return value.Value;
		}

		/*
		===============
		HasDynamicStat
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="statId"></param>
		/// <returns></returns>
		public bool HasDynamicStat( InternString statId ) {
			return _dynamicValues.ContainsKey( statId );
		}

		/*
		===============
		GetStatValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public float GetStatValue( StatType type ) {
			RangeGuard.ThrowIfOutOfRange( (int)type, (int)StatType.Min, (int)StatType.Max, nameof( type ) );
			return _statValues[ (int)type ].Value;
		}

		/*
		===============
		SetStatValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		public void SetStatValue( StatType type, float value ) {
			RangeGuard.ThrowIfOutOfRange( (int)type, (int)StatType.Min, (int)StatType.Max, nameof( type ) );

			ref var stat = ref _statValues[ (int)type ];
			float oldValue = stat.Value;
			stat.Value = value;

			_statChanged.Publish( new PlayerStatChangedEventArgs( value, oldValue, type ) );
		}
	};
};