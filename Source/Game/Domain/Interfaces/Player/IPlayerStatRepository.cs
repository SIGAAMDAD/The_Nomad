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

using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;

namespace Nomad.Game.Domain.Interfaces.Player {
	/*
	===================================================================================
	
	IPlayerStatsRepository
	
	===================================================================================
	*/
	/// <summary>
	/// The base abstraction contract for managing player related numbers.
	/// </summary>
	
	public interface IPlayerStatsRepository {
		/// <summary>
		/// 
		/// </summary>
		IGameEvent<PlayerStatChangedEventArgs> StatChanged { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		void SetStatValue( StatType type, float value );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		float GetStatValue( StatType type );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="statId"></param>
		/// <param name="limits"></param>
		/// <param name="initialValue"></param>
		void AddDynamicStat( InternString statId, StatLimits limits, float initialValue );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="statId"></param>
		/// <returns></returns>
		float GetDynamicStatValue( InternString statId );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="statId"></param>
		/// <returns></returns>
		bool HasDynamicStat( InternString statId );
	};
};