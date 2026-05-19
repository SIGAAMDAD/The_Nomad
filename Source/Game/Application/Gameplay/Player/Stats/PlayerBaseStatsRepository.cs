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
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player.Stats
{
	/*
	===================================================================================

	PlayerBaseStatsRepository

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class PlayerBaseStatsRepository : IPlayerBaseStatsRepository
	{
		private readonly float[] _statValues = new float[(int)BaseStatType.Count];

		private readonly ILoggerCategory _category;
		private readonly PlayerId _playerId;

		public IGameEvent<PlayerBaseStatChangedEventArgs> BaseStatChanged => _baseStatChanged;
		private readonly IGameEvent<PlayerBaseStatChangedEventArgs> _baseStatChanged;

		/*
		===============
		PlayerBaseStatsRepository
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="id"></param>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		public PlayerBaseStatsRepository( PlayerId playerId, IGameEventRegistryService eventFactory, ILoggerService logger )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			ArgumentGuard.ThrowIfNull( logger, nameof( logger ) );

			_playerId = playerId;

			_baseStatChanged = eventFactory.GetEvent<PlayerBaseStatChangedEventArgs>(
				PlayerBaseStatChangedEventArgs.Name,
				PlayerBaseStatChangedEventArgs.NameSpace
			);

			_category = logger?.CreateCategory( nameof( PlayerBaseStatsRepository ), LogLevel.Info, true );
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
		public float GetBaseStatValue( BaseStatType type )
		{
			RangeGuard.ThrowIfOutOfRange( (int)type, (int)BaseStatType.Min, (int)BaseStatType.Max, nameof( type ) );
			return _statValues[(int)type];
		}

		/*
		===============
		SetBaseStatValue
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		public void SetBaseStatValue( BaseStatType type, float value )
		{
			RangeGuard.ThrowIfOutOfRange( (int)type, (int)BaseStatType.Min, (int)BaseStatType.Max, nameof( type ) );

			ref float stat = ref _statValues[(int)type];
			if ( stat == value ) {
				return;
			}

			float oldValue = stat;
			stat = value;

			_baseStatChanged.Publish(
				new PlayerBaseStatChangedEventArgs(
					_playerId,
					oldValue,
					value,
					type
				)
			);
		}
	};
};
